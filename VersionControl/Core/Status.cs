using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal class Status
{
    private readonly IPathHolder _pathHolder;
    private readonly IDataRepository _dataRepository;
    private readonly IPathResolver _pathResolver;
    private readonly IFileSystem _fileSystem;

    public Status(IPathHolder pathHolder, IDataRepository dataRepository, IPathResolver pathResolver, IFileSystem fileSystem)
    {
        _pathHolder = pathHolder;
        _dataRepository = dataRepository;
        _pathResolver = pathResolver;
        _fileSystem = fileSystem;
    }

    public IReadOnlyCollection<VersionedFile> GetStatus()
    {
        var lastCommit = _dataRepository.GetLastCommit();
        var lastCommitDate = lastCommit?.CreatedUtc.ToFileTimeUtc() ?? 0;
        var lastCommitFiles = _dataRepository.GetLastPathFiles().ToDictionary(k => k.UniqueId, v => v.Path);
        var projectFilePathes = _fileSystem.GetFilesRecursively(_pathHolder.ProjectPath).ToList();
        projectFilePathes.RemoveAll(x => x.StartsWith(_pathHolder.RepositoryPath, StringComparison.OrdinalIgnoreCase)); // ignore repository files
        var projectFiles = projectFilePathes.Select(_fileSystem.GetFileInformation).ToList();
        var result = new List<VersionedFile>();
        CheckAddedAndModified(projectFiles, lastCommitDate, lastCommitFiles, result);
        CheckDeleted(projectFiles, lastCommitFiles, result);

        return result;
    }

    private void CheckAddedAndModified(
        IEnumerable<FileInformation> projectFiles, long lastCommitDate, Dictionary<ulong, string> lastCommitFiles, List<VersionedFile> versionedFiles)
    {
        foreach (var projectFile in projectFiles)
        {
            if (lastCommitFiles.ContainsKey(projectFile.UniqueId))
            {
                var projectFileRelative = _pathResolver.FullPathToRelative(projectFile.Path);
                var isModified = projectFile.ModifiedUtc > lastCommitDate;
                var isReplaced = !lastCommitFiles[projectFile.UniqueId].Equals(projectFileRelative, StringComparison.OrdinalIgnoreCase);
                if (isModified && isReplaced)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, FileActionKind.ModifyAndReplace));
                }
                else if (isModified)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, FileActionKind.Modify));
                }
                else if (isReplaced)
                {
                    versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, FileActionKind.Replace));
                }
            }
            else
            {
                versionedFiles.Add(new(projectFile.UniqueId, projectFile.Path, FileActionKind.Add));
            }
        }
    }

    private void CheckDeleted(IEnumerable<FileInformation> projectFiles, Dictionary<ulong, string> lastCommitFiles, List<VersionedFile> versionedFiles)
    {
        var projectFilesUniqueIdSet = projectFiles.Select(x => x.UniqueId).ToHashSet();
        foreach (var lastCommitFile in lastCommitFiles)
        {
            if (!projectFilesUniqueIdSet.Contains(lastCommitFile.Key))
            {
                var lastCommitFileFullPath = _pathResolver.RelativePathToFull(lastCommitFile.Value);
                versionedFiles.Add(new(lastCommitFile.Key, lastCommitFileFullPath, FileActionKind.Delete));
            }
        }
    }
}
