using System.Linq;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;

namespace VersionControl.Tests.Core;

internal class CommitDetailsTest
{
    private Mock<IDataRepository> _dataRepository;
    private CommitDetails _commitDetails;

    [SetUp]
    public void Setup()
    {
        _dataRepository = new Mock<IDataRepository>();
        _commitDetails = new CommitDetails(_dataRepository.Object);
    }

    [Test]
    public void GetCommitDetails()
    {
        var commitDetails = new CommitDetailPoco[]
        {
            new() { Id = 100, FileActionKind = (byte)FileActionKind.Add, FileId = 10, CommitId = 123 },
            new() { Id = 200, FileActionKind = (byte)FileActionKind.Modify, FileId = 11, CommitId = 123 },
            new() { Id = 300, FileActionKind = (byte)FileActionKind.Replace, FileId = 12, CommitId = 123 },
            new() { Id = 400, FileActionKind = (byte)FileActionKind.ModifyAndReplace, FileId = 13, CommitId = 123 },
            new() { Id = 500, FileActionKind = (byte)FileActionKind.Delete, FileId = 14, CommitId = 123 }
        };
        var filePathes = new FilePathPoco[]
        {
            new() { Id = 100, RelativePath = "add" },
            new() { Id = 300, RelativePath = "replace" },
            new() { Id = 400, RelativePath = "modifyReplace" }
        };
        var filePathForModify = new FilePathPoco
        {
            Id = 200,
            FileId = 11,
            RelativePath = "modify"
        };
        var filePathForDelete = new FilePathPoco
        {
            Id = 14,
            FileId = 11,
            RelativePath = "delete"
        };
        _dataRepository.Setup(x => x.GetCommitDetails(123)).Returns(commitDetails);
        _dataRepository.Setup(x => x.GetFilePathes(new uint[] { 100, 300, 400 })).Returns(filePathes);
        _dataRepository.Setup(x => x.GetFilePathFor(200, 11)).Returns(filePathForModify);
        _dataRepository.Setup(x => x.GetFilePathFor(500, 14)).Returns(filePathForDelete);

        var result = _commitDetails.GetCommitDetails(123).ToList();

        Assert.That(result, Has.Count.EqualTo(5));
        Assert.That(result[0], Is.EqualTo(new CommitDetail(100, FileActionKind.Add, 10, "add")));
        Assert.That(result[1], Is.EqualTo(new CommitDetail(200, FileActionKind.Modify, 11, "modify")));
        Assert.That(result[2], Is.EqualTo(new CommitDetail(300, FileActionKind.Replace, 12, "replace")));
        Assert.That(result[3], Is.EqualTo(new CommitDetail(400, FileActionKind.ModifyAndReplace, 13, "modifyReplace")));
        Assert.That(result[4], Is.EqualTo(new CommitDetail(500, FileActionKind.Delete, 14, "delete")));
    }

    [Test]
    public void GetFileContent_GetFileContentFor()
    {
        var content = new byte[] { 1, 2, 3, 4, 5 };
        var fileContent = new FileContentPoco { Id = 1, FileContent = content };
        _dataRepository.Setup(x => x.GetFileContentFor(111, 5)).Returns(fileContent);

        var result = _commitDetails.GetFileContent(111, 5);

        Assert.That(result, Is.EqualTo(content));
    }
}
