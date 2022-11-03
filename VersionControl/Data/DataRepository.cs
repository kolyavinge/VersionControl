using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleDB;
using VersionControl.Core;

namespace VersionControl.Data;

internal class DataRepository : IDataRepository
{
    private readonly IDBEngine _engine;

    public DataRepository(IPathHolder pathHolder)
    {
        var builder = DBEngineBuilder.Make();

        var databaseFilePath = Path.Combine(pathHolder.RepositoryPath, Constants.DBFileName);
        builder.DatabaseFilePath(databaseFilePath);

        builder.Map<CommitPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.Author)
            .Field(2, x => x.Comment)
            .Field(3, x => x.CreatedUtc);

        builder.Map<ActualFileInfoPoco>()
            .PrimaryKey(x => x.UniqueFileId)
            .Field(1, x => x.FileId)
            .Field(2, x => x.RelativePath)
            .Field(3, x => x.Size);

        builder.Map<CommitDetailPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.CommitId)
            .Field(2, x => x.FileId)
            .Field(3, x => x.FileActionKind);

        builder.Map<FileContentPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.FileId)
            .Field(2, x => x.FileContent, new() { Compressed = true });

        builder.Map<FilePathPoco>()
            .PrimaryKey(x => x.Id)
            .Field(1, x => x.FileId)
            .Field(2, x => x.RelativePath);

        _engine = builder.BuildEngine();
    }

    public ActualFileInfoPoco GetActualFileByUniqueId(ulong uniqueFileId)
    {
        return _engine.GetCollection<ActualFileInfoPoco>().Query()
            .Where(x => x.UniqueFileId == uniqueFileId).ToList().First();
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

    public IEnumerable<ActualFileInfoPoco> GetActualFileInfo()
    {
        return _engine.GetCollection<ActualFileInfoPoco>().GetAll();
    }

    public IReadOnlyCollection<ActualFileInfoPoco> GetActualFileInfoByUniqueId(IReadOnlyCollection<ulong> uniqueFileIdCollection)
    {
        return _engine.GetCollection<ActualFileInfoPoco>().Query()
            .Where(x => uniqueFileIdCollection.Contains(x.UniqueFileId))
            .ToList();
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

    public IEnumerable<FileContentPoco> GetFileContents(IEnumerable<uint> idCollection)
    {
        return _engine.GetCollection<FileContentPoco>().GetRange(idCollection.Cast<object>().ToList());
    }

    public IEnumerable<FilePathPoco> GetFilePathes(IEnumerable<uint> idCollection)
    {
        return _engine.GetCollection<FilePathPoco>().GetRange(idCollection.Cast<object>().ToList());
    }

    public FilePathPoco GetFilePathFor(uint commitDetailId, uint fileId)
    {
        return _engine.GetCollection<FilePathPoco>().Query()
            .Where(x => x.Id <= commitDetailId && x.FileId == fileId)
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .First();
    }

    public FileContentPoco GetFileContent(uint commitDetailId, uint fileId)
    {
        return _engine.GetCollection<FileContentPoco>().Query()
            .Where(x => x.Id <= commitDetailId && x.FileId == fileId)
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .First();
    }

    public FileContentPoco? GetFileContentBefore(uint commitDetailId, uint fileId)
    {
        return _engine.GetCollection<FileContentPoco>().Query()
            .Where(x => x.Id < commitDetailId && x.FileId == fileId)
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .FirstOrDefault();
    }

    public byte[] GetActualFileContent(uint fileId)
    {
        return _engine.GetCollection<FileContentPoco>().Query()
            .Where(x => x.FileId == fileId)
            .OrderBy(x => x.Id, SortDirection.Desc)
            .Limit(1)
            .ToList()
            .First().FileContent;
    }

    public void SaveCommit(CommitPoco commit)
    {
        _engine.GetCollection<CommitPoco>().Insert(commit);
    }

    public void SaveCommitDetails(IReadOnlyCollection<CommitDetailPoco> commitDetails)
    {
        _engine.GetCollection<CommitDetailPoco>().InsertRange(commitDetails);
    }

    public void SaveFileContents(IReadOnlyCollection<FileContentPoco> fileContents)
    {
        _engine.GetCollection<FileContentPoco>().InsertRange(fileContents);
    }

    public void SaveFilePathes(IReadOnlyCollection<FilePathPoco> filePathes)
    {
        _engine.GetCollection<FilePathPoco>().InsertRange(filePathes);
    }

    public void SaveActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> added)
    {
        _engine.GetCollection<ActualFileInfoPoco>().InsertRange(added);
    }

    public void UpdateActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> updated)
    {
        _engine.GetCollection<ActualFileInfoPoco>().UpdateRange(updated);
    }

    public void DeleteActualFileInfo(IEnumerable<ulong> deleted)
    {
        _engine.GetCollection<ActualFileInfoPoco>().DeleteRange(deleted.Cast<object>().ToList());
    }
}
