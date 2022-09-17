using System.Collections.Generic;
using System.Linq;

namespace VersionControl.Core;

public interface IRepository
{
    IReadOnlyCollection<VersionedFile> GetStatus();
    CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files);
    IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter);
    IReadOnlyCollection<CommitDetail> GetCommitDetail(Commit commit);
    byte[] GetFileContent(CommitDetail commitDetail);
}

internal class Repository : IRepository
{
    public static readonly string FolderName = ".vc";

    private readonly Status _status;
    private readonly CommitBuilder _commitBuilder;
    private readonly CommitDetails _commitDetails;
    private readonly CommitFinder _commitFinder;

    public Repository(Status status, CommitBuilder commitBuilder, CommitDetails commitDetails, CommitFinder commitFinder)
    {
        _status = status;
        _commitBuilder = commitBuilder;
        _commitDetails = commitDetails;
        _commitFinder = commitFinder;
    }

    public IReadOnlyCollection<VersionedFile> GetStatus()
    {
        return _status.GetStatus();
    }

    public CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files)
    {
        return _commitBuilder.MakeCommit(comment, files);
    }

    public IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter)
    {
        return _commitFinder.FindCommits(filter);
    }

    public IReadOnlyCollection<CommitDetail> GetCommitDetail(Commit commit)
    {
        return _commitDetails.GetCommitDetails(commit.Id).ToList();
    }

    public byte[] GetFileContent(CommitDetail commitDetail)
    {
        return _commitDetails.GetFileContent(commitDetail.Id, commitDetail.FileId);
    }
}
