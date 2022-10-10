using System.Collections.Generic;

namespace VersionControl.Core;

public class DummyVersionControlRepository : IVersionControlRepository
{
    public static readonly DummyVersionControlRepository Instance = new();

    private const string _errorMessage = "The repository hasn't opened.";

    private DummyVersionControlRepository() { }

    public IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter) => new List<Commit>();

    public IReadOnlyCollection<CommitDetail> GetCommitDetails(Commit commit) => new List<CommitDetail>();

    public byte[] GetFileContent(CommitDetail commitDetail) => throw new ArgumentException(_errorMessage);

    public VersionedStatus GetStatus() => VersionedStatus.Empty;

    public CommitResult MakeCommit(string comment, IReadOnlyCollection<VersionedFile> files) => throw new ArgumentException(_errorMessage);
}
