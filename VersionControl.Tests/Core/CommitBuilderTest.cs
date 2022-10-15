using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Tests.Core;

internal class CommitBuilderTest
{
    private Mock<IDataRepository> _dataRepository;
    private Mock<ISettings> _settings;
    private Mock<IPathResolver> _pathResolver;
    private Mock<IFileSystem> _fileSystem;
    private DateTime _now;
    private Mock<IDateTimeProvider> _dateTimeProvider;
    private CommitBuilder _commitBuilder;

    [SetUp]
    public void Setup()
    {
        _dataRepository = new Mock<IDataRepository>();
        _settings = new Mock<ISettings>();
        _settings.SetupGet(x => x.Author).Returns("author");
        _pathResolver = new Mock<IPathResolver>();
        _fileSystem = new Mock<IFileSystem>();
        _dateTimeProvider = new Mock<IDateTimeProvider>();
        _now = new DateTime(2000, 1, 1);
        _dateTimeProvider.Setup(x => x.UtcNow).Returns(_now);
        _commitBuilder = new CommitBuilder(_dataRepository.Object, _settings.Object, _pathResolver.Object, _fileSystem.Object, _dateTimeProvider.Object);
    }

    [Test]
    public void MakeCommit_NoComment()
    {
        try
        {
            _commitBuilder.MakeCommit("", Enumerable.Empty<VersionedFile>().ToList());
            Assert.Fail();
        }
        catch (ArgumentException exp)
        {
            Assert.That(exp.Message, Is.EqualTo("comment"));
        }
    }

    [Test]
    public void MakeCommit_EmptyFiles()
    {
        try
        {
            _commitBuilder.MakeCommit("comment", Enumerable.Empty<VersionedFile>().ToList());
            Assert.Fail();
        }
        catch (ArgumentException exp)
        {
            Assert.That(exp.Message, Is.EqualTo("files"));
        }
    }

    [Test]
    public void MakeCommit_CommitData()
    {
        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", "added", 128, FileActionKind.Add) });

        var commitResult = new CommitPoco { Id = _now.ToFileTimeUtc(), Author = "author", Comment = "comment", CreatedUtc = _now };
        var fileResult = new FilePoco { Id = 1, UniqueFileId = 10 };
        Assert.That(result.CommitId, Is.EqualTo(commitResult.Id));
        _dataRepository.Verify(x => x.SaveCommit(commitResult), Times.Once());
        _dataRepository.Verify(x => x.SaveFiles(new[] { fileResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_CommitDetails()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", "added", 128, FileActionKind.Add) });

        var fileResult = new FilePoco { Id = 8, UniqueFileId = 10 };
        var commitDetailsResult = new CommitDetailPoco { Id = 8, CommitId = _now.ToFileTimeUtc(), FileId = 8, FileActionKind = (byte)FileActionKind.Add };
        _dataRepository.Verify(x => x.SaveFiles(new[] { fileResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveCommitDetails(new[] { commitDetailsResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_AddFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\added")).Returns(new byte[] { 1, 2, 3 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", "added", 128, FileActionKind.Add) });

        var fileResult = new FilePoco { Id = 8, UniqueFileId = 10 };
        var fileContentResult = new FileContentPoco { Id = 8, FileId = 8, FileContent = new byte[] { 1, 2, 3 } };
        var filePathResult = new FilePathPoco { Id = 8, FileId = 8, RelativePath = "added" };
        _dataRepository.Verify(x => x.SaveFiles(new[] { fileResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveFiles(new[] { fileResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveFilePathes(new[] { filePathResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_ModifyFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new FilePoco { Id = 5, UniqueFileId = 10 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\modify")).Returns(new byte[] { 1, 2, 3 });

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\modify", "modify", 128, FileActionKind.Modify) });

        var fileContentResult = new FileContentPoco { Id = 8, FileId = 5, FileContent = new byte[] { 1, 2, 3 } };
        _dataRepository.Verify(x => x.SaveFileContents(new[] { fileContentResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_ReplaceFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new FilePoco { Id = 5, UniqueFileId = 10 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\replaced")).Returns("replaced");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\replaced", "replaced", 128, FileActionKind.Replace) });

        var filePathResult = new FilePathPoco { Id = 8, FileId = 5, RelativePath = "replaced" };
        _dataRepository.Verify(x => x.SaveFilePathes(new[] { filePathResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_ModifyAndReplaceFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new FilePoco { Id = 5, UniqueFileId = 10 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\modifyReplace")).Returns(new byte[] { 1, 2, 3 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modifyReplace")).Returns("modifyReplace");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\modifyReplace", "modifyReplace", 128, FileActionKind.ModifyAndReplace) });

        var fileContentResult = new FileContentPoco { Id = 8, FileId = 5, FileContent = new byte[] { 1, 2, 3 } };
        var filePathResult = new FilePathPoco { Id = 8, FileId = 5, RelativePath = "modifyReplace" };
        _dataRepository.Verify(x => x.SaveFileContents(new[] { fileContentResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveFilePathes(new[] { filePathResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_DeleteFile()
    {
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new FilePoco { Id = 5, UniqueFileId = 10 });

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\deleted", "deleted", 128, FileActionKind.Delete) });

        _dataRepository.Verify(x => x.SetUniqueFileIdFor(5, 0), Times.Once());
    }

    [Test]
    public void MakeCommit_SaveLastPathFiles()
    {
        _dataRepository.Setup(x => x.GetFileByUniqueId(2)).Returns(new FilePoco { Id = 10, UniqueFileId = 2 });
        _dataRepository.Setup(x => x.GetFileByUniqueId(3)).Returns(new FilePoco { Id = 20, UniqueFileId = 3 });
        _dataRepository.Setup(x => x.GetFileByUniqueId(4)).Returns(new FilePoco { Id = 30, UniqueFileId = 4 });
        _dataRepository.Setup(x => x.GetFileByUniqueId(5)).Returns(new FilePoco { Id = 40, UniqueFileId = 5 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\added")).Returns(new byte[0]);
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\replaced")).Returns("replaced");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modify")).Returns("modify");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modifyReplaced")).Returns("modifyReplaced");

        var result = _commitBuilder.MakeCommit("comment", new[]
        {
            new VersionedFile(1, "c:\\added", "added", 128, FileActionKind.Add),
            new VersionedFile(2, "c:\\replaced", "replaced", 128, FileActionKind.Replace),
            new VersionedFile(3, "c:\\modify", "modify", 128, FileActionKind.Modify),
            new VersionedFile(4, "c:\\modifyReplaced", "modifyReplaced", 128, FileActionKind.ModifyAndReplace),
            new VersionedFile(5, "c:\\deleted", "deleted", 128, FileActionKind.Delete)
        });

        var added = new[] { new ActualFileInfoPoco { UniqueFileId = 1, FileId = 1, RelativePath = "added", Size = 128 } };
        var updated = new[]
        {
            new ActualFileInfoPoco { UniqueFileId = 2, FileId = 10, RelativePath = "replaced", Size = 128 },
            new ActualFileInfoPoco { UniqueFileId = 3, FileId = 20, RelativePath = "modify", Size = 128 },
            new ActualFileInfoPoco { UniqueFileId = 4, FileId = 30, RelativePath = "modifyReplaced", Size = 128 }
        };
        _dataRepository.Verify(x => x.SaveActualFileInfo(added), Times.Once());
        _dataRepository.Verify(x => x.UpdateActualFileInfo(updated), Times.Once());
        _dataRepository.Verify(x => x.DeleteActualFileInfo(new ulong[] { 5 }), Times.Once());
    }
}
