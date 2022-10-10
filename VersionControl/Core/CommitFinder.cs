using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;

namespace VersionControl.Core;

internal interface ICommitFinder
{
    IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter);
}

internal class CommitFinder : ICommitFinder
{
    private readonly IDataRepository _dataRepository;

    public CommitFinder(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter)
    {
        var commitsPoco = _dataRepository.FindCommits(filter);
        var commits = commitsPoco
            .Select(x => new Commit(x.Id, x.Author, x.Comment, x.CreatedUtc))
            .OrderByDescending(x => x.CreatedUtc)
            .ToList();

        return commits;
    }
}
