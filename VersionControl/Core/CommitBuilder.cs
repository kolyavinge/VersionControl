using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal class CommitBuilder
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
        SaveCommitDetailsAndActionsPoco(commitId, files);
        SaveLastPathFiles(files);

        return new CommitResult(commitId);
    }

    private void SaveCommitDetailsAndActionsPoco(long commitId, IEnumerable<VersionedFile> files)
    {
        var versionedFiles = new List<VersionedFilePoco>();
        var commitDetails = new List<CommitDetailPoco>();
        var addFileActions = new List<AddFileActionPoco>();
        var modifyFileActions = new List<ModifyFileActionPoco>();
        var replaceFileActions = new List<ReplaceFileActionPoco>();

        var id = _dataRepository.GetCommitDetailsCount() + 1;

        foreach (var file in files)
        {
            VersionedFilePoco dbfile;
            if (file.ActionKind == FileActionKind.Add)
            {
                dbfile = new VersionedFilePoco { Id = id, UniqueFileId = file.UniqueId };
                versionedFiles.Add(dbfile);
            }
            else
            {
                dbfile = _dataRepository.GetFileByUniqueId(file.UniqueId); // make index in db
            }

            commitDetails.Add(new CommitDetailPoco
            {
                Id = id,
                CommitId = commitId,
                FileId = dbfile.Id,
                FileActionKind = (byte)file.ActionKind,
            });

            if (file.ActionKind == FileActionKind.Add)
            {
                addFileActions.Add(new AddFileActionPoco
                {
                    Id = id,
                    RelativePath = _pathResolver.FullPathToRelative(file.FullPath),
                    FileContent = _fileSystem.ReadFileBytes(file.FullPath)
                });
            }
            else if (file.ActionKind == FileActionKind.Modify)
            {
                modifyFileActions.Add(new ModifyFileActionPoco
                {
                    Id = id,
                    FileContent = _fileSystem.ReadFileBytes(file.FullPath)
                });
            }
            else if (file.ActionKind == FileActionKind.Replace)
            {
                replaceFileActions.Add(new ReplaceFileActionPoco
                {
                    Id = id,
                    RelativePath = _pathResolver.FullPathToRelative(file.FullPath)
                });
            }
            else if (file.ActionKind == FileActionKind.ModifyAndReplace)
            {
                modifyFileActions.Add(new ModifyFileActionPoco
                {
                    Id = id,
                    FileContent = _fileSystem.ReadFileBytes(file.FullPath)
                });
                replaceFileActions.Add(new ReplaceFileActionPoco
                {
                    Id = id,
                    RelativePath = _pathResolver.FullPathToRelative(file.FullPath)
                });
            }
            else // Delete
            {
                _dataRepository.ClearUniqueFileIdFor(dbfile.Id);
            }

            id++;
        }

        _dataRepository.SaveVersionedFiles(versionedFiles);
        _dataRepository.SaveCommitDetails(commitDetails);
        _dataRepository.SaveAddFileActions(addFileActions);
        _dataRepository.SaveModifyFileActions(modifyFileActions);
        _dataRepository.SaveReplaceFileActions(replaceFileActions);
    }

    private void SaveLastPathFiles(IReadOnlyCollection<VersionedFile> files)
    {
        var added = files.Where(x => x.ActionKind == FileActionKind.Add).Select(x => new LastPathFilePoco
        {
            UniqueId = x.UniqueId,
            Path = _pathResolver.FullPathToRelative(x.FullPath)
        }).ToList();

        var replaced = files.Where(x => x.ActionKind is FileActionKind.Replace or FileActionKind.ModifyAndReplace).Select(x => new LastPathFilePoco
        {
            UniqueId = x.UniqueId,
            Path = _pathResolver.FullPathToRelative(x.FullPath)
        }).ToList();

        var deleted = files.Where(x => x.ActionKind == FileActionKind.Delete).Select(x => x.UniqueId).ToList();

        _dataRepository.SaveLastPathFiles(added);
        _dataRepository.UpdateLastPathFiles(replaced);
        _dataRepository.DeleteLastPathFiles(deleted);
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
