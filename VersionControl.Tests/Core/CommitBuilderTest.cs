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
        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", FileActionKind.Add) });

        var commitResult = new CommitPoco { Id = _now.ToFileTimeUtc(), Author = "author", Comment = "comment", CreatedUtc = _now };
        var versionedFileResult = new VersionedFilePoco { Id = 1, UniqueFileId = 10 };
        Assert.That(result.CommitId, Is.EqualTo(commitResult.Id));
        _dataRepository.Verify(x => x.SaveCommit(commitResult), Times.Once());
        _dataRepository.Verify(x => x.SaveVersionedFiles(new[] { versionedFileResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_CommitDetails()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", FileActionKind.Add) });

        var versionedFileResult = new VersionedFilePoco { Id = 8, UniqueFileId = 10 };
        var commitDetailsResult = new CommitDetailPoco { Id = 8, CommitId = _now.ToFileTimeUtc(), FileId = 8, FileActionKind = (byte)FileActionKind.Add };
        _dataRepository.Verify(x => x.SaveVersionedFiles(new[] { versionedFileResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveCommitDetails(new[] { commitDetailsResult }), Times.Once());
    }

    [Test]
    public void MakeCommit_AddFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\added")).Returns(new byte[] { 1, 2, 3 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\added", FileActionKind.Add) });

        var versionedFileResult = new VersionedFilePoco { Id = 8, UniqueFileId = 10 };
        var addFileActionPoco = new AddFileActionPoco { Id = 8, RelativePath = "added", FileContent = new byte[] { 1, 2, 3 } };
        _dataRepository.Verify(x => x.SaveVersionedFiles(new[] { versionedFileResult }), Times.Once());
        _dataRepository.Verify(x => x.SaveAddFileActions(new[] { addFileActionPoco }), Times.Once());
    }

    [Test]
    public void MakeCommit_ModifyFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new VersionedFilePoco { Id = 5, UniqueFileId = 10 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\modify")).Returns(new byte[] { 1, 2, 3 });

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\modify", FileActionKind.Modify) });

        var modifyFileActionPoco = new ModifyFileActionPoco { Id = 8, FileContent = new byte[] { 1, 2, 3 } };
        _dataRepository.Verify(x => x.SaveModifyFileActions(new[] { modifyFileActionPoco }), Times.Once());
    }

    [Test]
    public void MakeCommit_ReplaceFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new VersionedFilePoco { Id = 5, UniqueFileId = 10 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\replaced")).Returns("replaced");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\replaced", FileActionKind.Replace) });

        var replaceFileActionPoco = new ReplaceFileActionPoco { Id = 8, RelativePath = "replaced" };
        _dataRepository.Verify(x => x.SaveReplaceFileActions(new[] { replaceFileActionPoco }), Times.Once());
    }

    [Test]
    public void MakeCommit_ModifyAndReplaceFile()
    {
        _dataRepository.Setup(x => x.GetCommitDetailsCount()).Returns(7);
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new VersionedFilePoco { Id = 5, UniqueFileId = 10 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\modifyReplace")).Returns(new byte[] { 1, 2, 3 });
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modifyReplace")).Returns("modifyReplace");

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\modifyReplace", FileActionKind.ModifyAndReplace) });

        var modifyFileActionPoco = new ModifyFileActionPoco { Id = 8, FileContent = new byte[] { 1, 2, 3 } };
        var replaceFileActionPoco = new ReplaceFileActionPoco { Id = 8, RelativePath = "modifyReplace" };
        _dataRepository.Verify(x => x.SaveModifyFileActions(new[] { modifyFileActionPoco }), Times.Once());
        _dataRepository.Verify(x => x.SaveReplaceFileActions(new[] { replaceFileActionPoco }), Times.Once());
    }

    [Test]
    public void MakeCommit_DeleteFile()
    {
        _dataRepository.Setup(x => x.GetFileByUniqueId(10)).Returns(new VersionedFilePoco { Id = 5, UniqueFileId = 10 });

        var result = _commitBuilder.MakeCommit("comment", new[] { new VersionedFile(10, "c:\\deleted", FileActionKind.Delete) });

        _dataRepository.Verify(x => x.ClearUniqueFileIdFor(5), Times.Once());
    }

    [Test]
    public void MakeCommit_SaveLastPathFiles()
    {
        _dataRepository.Setup(x => x.GetFileByUniqueId(2)).Returns(new VersionedFilePoco { Id = 1, UniqueFileId = 2 });
        _dataRepository.Setup(x => x.GetFileByUniqueId(3)).Returns(new VersionedFilePoco { Id = 2, UniqueFileId = 3 });
        _dataRepository.Setup(x => x.GetFileByUniqueId(4)).Returns(new VersionedFilePoco { Id = 3, UniqueFileId = 4 });
        _fileSystem.Setup(x => x.ReadFileBytes("c:\\added")).Returns(new byte[0]);
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\replaced")).Returns("replaced");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modifyReplaced")).Returns("modifyReplaced");

        var result = _commitBuilder.MakeCommit("comment", new[]
        {
            new VersionedFile(1, "c:\\added", FileActionKind.Add),
            new VersionedFile(2, "c:\\replaced", FileActionKind.Replace),
            new VersionedFile(3, "c:\\modifyReplaced", FileActionKind.ModifyAndReplace),
            new VersionedFile(4, "c:\\deleted", FileActionKind.Delete)
        });

        var added = new[] { new LastPathFilePoco { UniqueId = 1, Path = "added" } };
        var updated = new[] { new LastPathFilePoco { UniqueId = 2, Path = "replaced" }, new LastPathFilePoco { UniqueId = 3, Path = "modifyReplaced" } };
        _dataRepository.Verify(x => x.SaveLastPathFiles(added), Times.Once());
        _dataRepository.Verify(x => x.UpdateLastPathFiles(updated), Times.Once());
        _dataRepository.Verify(x => x.DeleteLastPathFiles(new ulong[] { 4 }), Times.Once());
    }
}
