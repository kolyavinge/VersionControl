using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleDB;
using VersionControl.Core;

namespace VersionControl.Data;

internal class DataRepository : IDataRepository
{
    private readonly IDBEngine _engine;

    public DataRepository(string repositoryPath)
    {
        var builder = DBEngineBuilder.Make();

        var databaseFilePath = Path.Combine(repositoryPath, Constants.DBFileName);
        builder.DatabaseFilePath(databaseFilePath);

        builder.Map<CommitPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.Author)
            .Field(2, x => x.Comment)
            .Field(3, x => x.CreatedUtc);

        builder.Map<LastPathFilePoco>()
            .PrimaryKey(x => x.UniqueId)
            .Field(1, x => x.Path);

        builder.Map<CommitDetailPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.CommitId)
            .Field(2, x => x.FileId)
            .Field(3, x => x.FileActionKind);

        builder.Map<AddFileActionPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.RelativePath)
            .Field(2, x => x.FileContent, new() { Compressed = true });

        builder.Map<ModifyFileActionPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.FileContent, new() { Compressed = true });

        builder.Map<ReplaceFileActionPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.RelativePath);

        builder.Map<VersionedFilePoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.UniqueFileId);

        _engine = builder.BuildEngine();
    }

    public VersionedFilePoco GetFileByUniqueId(ulong uniqueFileId)
    {
        return _engine.GetCollection<VersionedFilePoco>().Query()
            .Where(x => x.UniqueFileId == uniqueFileId).ToList().First();
    }

    public void ClearUniqueFileIdFor(uint fileId)
    {
        _engine.GetCollection<VersionedFilePoco>().Query()
            .Update(x => new() { UniqueFileId = 0 }, x => x.Id == fileId);
    }

    public CommitPoco? GetLastCommit()
    {
        return _engine.GetCollection<CommitPoco>().Query()
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .FirstOrDefault();
    }

    public IReadOnlyCollection<CommitPoco> FindCommits(FindCommitsFilter filter)
    {
        var query = _engine.GetCollection<CommitPoco>().Query()
            .Skip(filter.PageIndex * filter.PageSize)
            .Limit(filter.PageSize);

        if (!String.IsNullOrWhiteSpace(filter.Author))
        {
            query = query.Where(x => x.Author.Contains(filter.Author));
        }

        if (!String.IsNullOrWhiteSpace(filter.Comment))
        {
            query = query.Where(x => x.Comment.Contains(filter.Comment));
        }

        var result = query.ToList();

        return result;
    }

    public IEnumerable<LastPathFilePoco> GetLastPathFiles()
    {
        return _engine.GetCollection<LastPathFilePoco>().GetAll();
    }

    public uint GetCommitDetailsCount()
    {
        return (uint)_engine.GetCollection<CommitDetailPoco>().Count();
    }

    public IEnumerable<CommitDetailPoco> GetCommitDetails(long commitId)
    {
        return _engine.GetCollection<CommitDetailPoco>().Query()
            .Where(x => x.CommitId == commitId)
            .ToList();
    }

    public IEnumerable<AddFileActionPoco> GetAddActions(IEnumerable<uint> idCollection)
    {
        return _engine.GetCollection<AddFileActionPoco>().GetRange(idCollection.Cast<object>().ToList());
    }

    public IEnumerable<ModifyFileActionPoco> GetModifyActions(IEnumerable<uint> idCollection)
    {
        return _engine.GetCollection<ModifyFileActionPoco>().GetRange(idCollection.Cast<object>().ToList());
    }

    public IEnumerable<ReplaceFileActionPoco> GetReplaceActions(IEnumerable<uint> idCollection)
    {
        return _engine.GetCollection<ReplaceFileActionPoco>().GetRange(idCollection.Cast<object>().ToList());
    }

    public CommitDetailPoco? GetLastCommitDetailForReplace(uint commitDetailId, uint fileId)
    {
        return _engine.GetCollection<CommitDetailPoco>().Query()
            .Where(x =>
                x.Id <= commitDetailId &&
                x.FileId == fileId &&
                (x.FileActionKind == (byte)FileActionKind.Replace || x.FileActionKind == (byte)FileActionKind.ModifyAndReplace))
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .FirstOrDefault();
    }

    public CommitDetailPoco? GetLastCommitDetailForModify(uint commitDetailId, uint fileId)
    {
        return _engine.GetCollection<CommitDetailPoco>().Query()
            .Where(x =>
                x.Id <= commitDetailId &&
                x.FileId == fileId &&
                (x.FileActionKind == (byte)FileActionKind.Modify || x.FileActionKind == (byte)FileActionKind.ModifyAndReplace))
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .FirstOrDefault();
    }

    public void SaveVersionedFiles(IReadOnlyCollection<VersionedFilePoco> versionedFiles)
    {
        _engine.GetCollection<VersionedFilePoco>().InsertRange(versionedFiles);
    }

    public void SaveCommit(CommitPoco commit)
    {
        _engine.GetCollection<CommitPoco>().Insert(commit);
    }

    public void SaveCommitDetails(IReadOnlyCollection<CommitDetailPoco> commitDetails)
    {
        _engine.GetCollection<CommitDetailPoco>().InsertRange(commitDetails);
    }

    public void SaveAddFileActions(IReadOnlyCollection<AddFileActionPoco> addFileActions)
    {
        _engine.GetCollection<AddFileActionPoco>().InsertRange(addFileActions);
    }

    public void SaveModifyFileActions(IReadOnlyCollection<ModifyFileActionPoco> modifyFileActions)
    {
        _engine.GetCollection<ModifyFileActionPoco>().InsertRange(modifyFileActions);
    }

    public void SaveReplaceFileActions(IReadOnlyCollection<ReplaceFileActionPoco> replaceFileActions)
    {
        _engine.GetCollection<ReplaceFileActionPoco>().InsertRange(replaceFileActions);
    }

    public void SaveLastPathFiles(IReadOnlyCollection<LastPathFilePoco> added)
    {
        _engine.GetCollection<LastPathFilePoco>().InsertRange(added);
    }

    public void UpdateLastPathFiles(IReadOnlyCollection<LastPathFilePoco> updated)
    {
        _engine.GetCollection<LastPathFilePoco>().UpdateRange(updated);
    }

    public void DeleteLastPathFiles(IEnumerable<ulong> deleted)
    {
        _engine.GetCollection<LastPathFilePoco>().DeleteRange(deleted.Cast<object>().ToList());
    }
}
