﻿using System.Collections.Generic;

namespace VersionControl.Core;

public interface IVersionControlRepository
{
    VersionedStatus GetStatus();

    CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files);

    IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter);

    IReadOnlyCollection<CommitDetail> GetCommitDetails(Commit commit);

    byte[]? GetActualFileContent(VersionedFile file);

    byte[] GetFileContent(CommitDetail commitDetail);

    byte[]? GetFileContentBefore(CommitDetail commitDetail);

    void UndoChanges(IReadOnlyCollection<VersionedFile> files);
}
