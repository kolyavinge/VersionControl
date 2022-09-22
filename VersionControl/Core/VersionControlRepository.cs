using System.Collections.Generic;
using System.Linq;

namespace VersionControl.Core;

public interface IVersionControlRepository
{
    IReadOnlyCollection<VersionedFile> GetStatus();
    CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files);
    IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter);
    IReadOnlyCollection<CommitDetail> GetCommitDetail(Commit commit);
    byte[] GetFileContent(CommitDetail commitDetail);
}

internal class VersionControlRepository : IVersionControlRepository
{
    private readonly IStatus _status;
    private readonly ICommitBuilder _commitBuilder;
    private readonly ICommitDetails _commitDetails;
    private readonly ICommitFinder _commitFinder;

    public VersionControlRepository(IStatus status, ICommitBuilder commitBuilder, ICommitDetails commitDetails, ICommitFinder commitFinder)
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
