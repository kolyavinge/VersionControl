using System.Collections.Generic;

namespace VersionControl.Core;

public class DummyVersionControlRepository : IVersionControlRepository
{
    public static readonly DummyVersionControlRepository Instance = new();

    private const string _errorMessage = "The repository hasn't opened.";

    private DummyVersionControlRepository() { }

    public IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter) => throw new ArgumentException(_errorMessage);

    public IReadOnlyCollection<CommitDetail> GetCommitDetail(Commit commit) => throw new ArgumentException(_errorMessage);

    public byte[] GetFileContent(CommitDetail commitDetail) => throw new ArgumentException(_errorMessage);

    public VersionedStatus GetStatus() => throw new ArgumentException(_errorMessage);

    public CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files) => throw new ArgumentException(_errorMessage);
}
