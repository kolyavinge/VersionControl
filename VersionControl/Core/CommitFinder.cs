﻿using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;

namespace VersionControl.Core;

internal class CommitFinder
{
    private readonly IDataRepository _dataRepository;

    public CommitFinder(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public IReadOnlyCollection<Commit> FindCommits(FindCommitsFilter filter)
    {
        var commitsPoco = _dataRepository.FindCommits(filter);
        var commits = commitsPoco.Select(x => new Commit(x.Id, x.Author, x.Comment, x.CreatedUtc)).ToList();

        return commits;
    }
}