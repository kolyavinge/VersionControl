using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal interface IStatus
{
    IReadOnlyCollection<VersionedFile> GetStatus();
}

internal class Status : IStatus
{
    private readonly IPathHolder _pathHolder;
    private readonly IDataRepository _dataRepository;
    private readonly IPathResolver _pathResolver;
    private readonly IFileComparator _fileComparator;
    private readonly IFileSystem _fileSystem;

    public Status(
        IPathHolder pathHolder,
        IDataRepository dataRepository,
        IPathResolver pathResolver,
        IFileComparator fileComparator,
        IFileSystem fileSystem)
    {
        _pathHolder = pathHolder;
        _dataRepository = dataRepository;
        _pathResolver = pathResolver;
        _fileComparator = fileComparator;
        _fileSystem = fileSystem;
    }

    public IReadOnlyCollection<VersionedFile> GetStatus()
    {
        var lastCommit = _dataRepository.GetLastCommit();
        var lastCommitDate = lastCommit?.CreatedUtc.ToFileTimeUtc() ?? 0;
        var actualFilesInfo = _dataRepository.GetActualFileInfo().ToDictionary(k => k.UniqueId, v => v);
        var projectFilePathes = _fileSystem.GetFilesRecursively(_pathHolder.ProjectPath).ToList();
        projectFilePathes.RemoveAll(x => x.StartsWith(_pathHolder.RepositoryPath, StringComparison.OrdinalIgnoreCase)); // ignore repository files
        var projectFiles = projectFilePathes.Select(_fileSystem.GetFileInformation).ToList();
        var result = new List<VersionedFile>();
        CheckAddedAndModified(projectFiles, lastCommitDate, actualFilesInfo, result);
        CheckDeleted(projectFiles, actualFilesInfo, result);

        return result;
    }

    private void CheckAddedAndModified(
        IEnumerable<FileInformation> projectFiles,
        long lastCommitDate,
        Dictionary<ulong, ActualFileInfoPoco> actualFilesInfo,
        List<VersionedFile> versionedFiles)
    {
        foreach (var projectFile in projectFiles)
        {
            if (actualFilesInfo.ContainsKey(projectFile.UniqueId))
            {
                var actualFileInfo = actualFilesInfo[projectFile.UniqueId];
                var projectFileRelativePath = _pathResolver.FullPathToRelative(projectFile.Path);
                var isModified = IsModified(projectFile, lastCommitDate, actualFileInfo);
                var isReplaced = !actualFileInfo.Path.Equals(projectFileRelativePath, StringComparison.OrdinalIgnoreCase);
                if (isModified && isReplaced)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, projectFile.Size, FileActionKind.ModifyAndReplace));
                }
                else if (isModified)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, projectFile.Size, FileActionKind.Modify));
                }
                else if (isReplaced)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, projectFile.Size, FileActionKind.Replace));
                }
            }
            else
            {
                versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, projectFile.Size, FileActionKind.Add));
            }
        }
    }

    private bool IsModified(FileInformation projectFile, long lastCommitDate, ActualFileInfoPoco actualFileInfo)
    {
        return
            projectFile.ModifiedUtc > lastCommitDate &&
            (projectFile.Size != actualFileInfo.Size || !AreFilesEqual(projectFile, actualFileInfo));
    }

    private bool AreFilesEqual(FileInformation projectFile, ActualFileInfoPoco actualFileInfo)
    {
        var actualFileContent = _dataRepository.GetActualFileContent(actualFileInfo.FileId);
        return _fileComparator.AreEqual(actualFileContent, projectFile.Path);
    }

    private void CheckDeleted(IEnumerable<FileInformation> projectFiles, Dictionary<ulong, ActualFileInfoPoco> actualFilesInfo, List<VersionedFile> versionedFiles)
    {
        var projectFilesUniqueIdSet = projectFiles.Select(x => x.UniqueId).ToHashSet();
        foreach (var actualFileInfo in actualFilesInfo)
        {
            if (!projectFilesUniqueIdSet.Contains(actualFileInfo.Key))
            {
                var lastCommitFileFullPath = _pathResolver.RelativePathToFull(actualFileInfo.Value.Path);
                versionedFiles.Add(new(actualFileInfo.Key, lastCommitFileFullPath, 0, FileActionKind.Delete));
            }
        }
    }
}
