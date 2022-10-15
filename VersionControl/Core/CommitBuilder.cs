using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal interface ICommitBuilder
{
    CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files);
}

internal class CommitBuilder : ICommitBuilder
{
    private readonly IDataRepository _dataRepository;
    private readonly ISettings _settings;
    private readonly IPathResolver _pathResolver;
    private readonly IFileSystem _fileSystem;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CommitBuilder(
        IDataRepository dataRepository,
        ISettings settings,
        IPathResolver pathResolver,
        IFileSystem fileSystem,
        IDateTimeProvider dateTimeProvider)
    {
        _dataRepository = dataRepository;
        _settings = settings;
        _pathResolver = pathResolver;
        _fileSystem = fileSystem;
        _dateTimeProvider = dateTimeProvider;
    }

    public CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files)
    {
        if (String.IsNullOrWhiteSpace(comment)) throw new ArgumentException(nameof(comment));
        if (!files.Any()) throw new ArgumentException(nameof(files));
        var now = _dateTimeProvider.UtcNow;
        var commitId = now.ToFileTimeUtc();
        _dataRepository.SaveCommit(new CommitPoco
        {
            Id = commitId,
            Author = _settings.Author,
            Comment = comment,
            CreatedUtc = now
        });
        var filesId = new Dictionary<ulong, uint>();
        SaveCommitDetailsAndActionsPoco(commitId, files, filesId);
        SaveActualFilesInfo(files, filesId);

        return new CommitResult(commitId);
    }

    private void SaveCommitDetailsAndActionsPoco(long commitId, IEnumerable<VersionedFile> versionedFiles, Dictionary<ulong, uint> filesId)
    {
        var newFiles = new List<FilePoco>();
        var commitDetails = new List<CommitDetailPoco>();
        var fileContents = new List<FileContentPoco>();
        var filePathes = new List<FilePathPoco>();

        var id = _dataRepository.GetCommitDetailsCount() + 1;

        foreach (var versionedFile in versionedFiles)
        {
            uint fileId;
            if (versionedFile.ActionKind == FileActionKind.Add)
            {
                fileId = id;
                newFiles.Add(new FilePoco { Id = fileId });
            }
            else
            {
                fileId = _dataRepository.GetActualFileByUniqueId(versionedFile.UniqueId).FileId; // make index in db
            }
            filesId.Add(versionedFile.UniqueId, fileId);

            commitDetails.Add(new CommitDetailPoco
            {
                Id = id,
                CommitId = commitId,
                FileId = fileId,
                FileActionKind = (byte)versionedFile.ActionKind,
            });

            if (versionedFile.ActionKind is FileActionKind.Add or FileActionKind.ModifyAndReplace)
            {
                fileContents.Add(new FileContentPoco
                {
                    Id = id,
                    FileId = fileId,
                    FileContent = _fileSystem.ReadFileBytes(versionedFile.FullPath)
                });
                filePathes.Add(new FilePathPoco
                {
                    Id = id,
                    FileId = fileId,
                    RelativePath = _pathResolver.FullPathToRelative(versionedFile.FullPath)
                });
            }
            else if (versionedFile.ActionKind is FileActionKind.Modify)
            {
                fileContents.Add(new FileContentPoco
                {
                    Id = id,
                    FileId = fileId,
                    FileContent = _fileSystem.ReadFileBytes(versionedFile.FullPath)
                });
            }
            else if (versionedFile.ActionKind is FileActionKind.Replace)
            {
                filePathes.Add(new FilePathPoco
                {
                    Id = id,
                    FileId = fileId,
                    RelativePath = _pathResolver.FullPathToRelative(versionedFile.FullPath)
                });
            }
            // no action for Delete

            id++;
        }

        _dataRepository.SaveFiles(newFiles);
        _dataRepository.SaveCommitDetails(commitDetails);
        _dataRepository.SaveFileContents(fileContents);
        _dataRepository.SaveFilePathes(filePathes);
    }

    private void SaveActualFilesInfo(IReadOnlyCollection<VersionedFile> files, Dictionary<ulong, uint> filesId)
    {
        var added = files
            .Where(x => x.ActionKind == FileActionKind.Add)
            .Select(x => new ActualFileInfoPoco
            {
                UniqueFileId = x.UniqueId,
                FileId = filesId[x.UniqueId],
                RelativePath = _pathResolver.FullPathToRelative(x.FullPath),
                Size = x.FileSize
            }).ToList();

        var updated = files
            .Where(x => x.ActionKind is FileActionKind.Modify or FileActionKind.Replace or FileActionKind.ModifyAndReplace)
            .Select(x => new ActualFileInfoPoco
            {
                UniqueFileId = x.UniqueId,
                FileId = filesId[x.UniqueId],
                RelativePath = _pathResolver.FullPathToRelative(x.FullPath),
                Size = x.FileSize
            }).ToList();

        var deleted = files.Where(x => x.ActionKind == FileActionKind.Delete).Select(x => x.UniqueId).ToList();

        _dataRepository.SaveActualFileInfo(added);
        _dataRepository.UpdateActualFileInfo(updated);
        _dataRepository.DeleteActualFileInfo(deleted);
    }
}

public readonly struct CommitResult
{
    public readonly long CommitId;

    internal CommitResult(long commitId)
    {
        CommitId = commitId;
    }
}
