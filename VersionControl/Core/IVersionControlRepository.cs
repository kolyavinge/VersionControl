using System.Collections.Generic;

namespace VersionControl.Core;

public interface IVersionControlRepository
{
    VersionedStatus GetStatus();
    CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files);
    IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter);
    IReadOnlyCollection<CommitDetail> GetCommitDetail(Commit commit);
    byte[] GetFileContent(CommitDetail commitDetail);
}
