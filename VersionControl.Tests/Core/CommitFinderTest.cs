using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;

namespace VersionControl.Tests.Core;

internal class CommitFinderTest
{
    private Mock<IDataRepository> _dataRepository;
    private CommitFinder _commitFinder;

    [SetUp]
    public void Setup()
    {
        _dataRepository = new Mock<IDataRepository>();
        _commitFinder = new CommitFinder(_dataRepository.Object);
    }

    [Test]
    public void FindCommits_OrderByDescendingCreated()
    {
        var filter = new FindCommitsFilter();
        var commitsPoco = new List<CommitPoco>
        {
            new() { Id = 1, CreatedUtc = DateTime.UtcNow },
            new() { Id = 2, CreatedUtc = DateTime.UtcNow.AddDays(1) },
            new() { Id = 3, CreatedUtc = DateTime.UtcNow.AddDays(2) }
        };
        _dataRepository.Setup(x => x.FindCommits(filter)).Returns(commitsPoco);

        var result = _commitFinder.FindCommits(filter).ToList();

        Assert.That(result[0].Id, Is.EqualTo(3));
        Assert.That(result[1].Id, Is.EqualTo(2));
        Assert.That(result[2].Id, Is.EqualTo(1));
    }
}
