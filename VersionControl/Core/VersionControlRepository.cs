﻿using System.Collections.Generic;
using System.Linq;

namespace VersionControl.Core;

internal class VersionControlRepository : IVersionControlRepository
{
    private readonly IStatus _status;
    private readonly ICommitBuilder _commitBuilder;
    private readonly ICommitDetails _commitDetails;
    private readonly ICommitFinder _commitFinder;
    private readonly IUndoLogic _undoLogic;

    public VersionControlRepository(
        IStatus status,
        ICommitBuilder commitBuilder,
        ICommitDetails commitDetails,
        ICommitFinder commitFinder,
        IUndoLogic undoLogic)
    {
        _status = status;
        _commitBuilder = commitBuilder;
        _commitDetails = commitDetails;
        _commitFinder = commitFinder;
        _undoLogic = undoLogic;
    }

    public VersionedStatus GetStatus()
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

    public IReadOnlyCollection<CommitDetail> GetCommitDetails(Commit commit)
    {
        return _commitDetails.GetCommitDetails(commit.Id).ToList();
    }

    public byte[] GetFileContent(CommitDetail commitDetail)
    {
        return _commitDetails.GetFileContent(commitDetail.Id, commitDetail.FileId);
    }

    public void UndoChanges(IReadOnlyCollection<VersionedFile> files)
    {
        _undoLogic.UndoChanges(files);
    }
}
