using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Tests.Core;

internal class UndoLogicTest
{
    private Mock<IDataRepository> _repository;
    private Mock<IFileSystem> _fileSystem;
    private Mock<IPathResolver> _pathResolver;
    private UndoLogic _undoLogic;

    [SetUp]
    public void Setup()
    {
        _repository = new Mock<IDataRepository>();
        _fileSystem = new Mock<IFileSystem>();
        _pathResolver = new Mock<IPathResolver>();
        _undoLogic = new UndoLogic(_repository.Object, _fileSystem.Object, _pathResolver.Object);
    }

    [Test]
    public void EmptyFileCollection()
    {
        _undoLogic.UndoChanges(new VersionedFile[0]);

        _repository.Verify(x => x.GetActualFileInfoByUniqueId(It.IsAny<IReadOnlyCollection<ulong>>()), Times.Never());
        _repository.Verify(x => x.DeleteActualFileInfo(It.IsAny<IReadOnlyCollection<ulong>>()), Times.Never());
        _repository.Verify(x => x.SaveActualFileInfo(It.IsAny<IReadOnlyCollection<ActualFileInfoPoco>>()), Times.Never());
    }

    [Test]
    public void UndoAdded()
    {
        var files = new VersionedFile[] { new(123, "c:\\file", "file", 10, FileActionKind.Add) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[0])).Returns(new ActualFileInfoPoco[0]);

        _undoLogic.UndoChanges(files);

        _fileSystem.Verify(x => x.DeleteFile("c:\\file"), Times.Once());
    }

    [Test]
    public void UndoModified()
    {
        var files = new VersionedFile[] { new(123, "c:\\file", "file", 10, FileActionKind.Modify) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[] { 123 }))
            .Returns(new ActualFileInfoPoco[] { new() { FileId = 1, UniqueFileId = 123 } });
        var content = new FileContentPoco { FileContent = new byte[] { 1, 2, 3 } };
        _repository.Setup(x => x.GetActualFileContent(1)).Returns(content);

        _undoLogic.UndoChanges(files);

        _fileSystem.Verify(x => x.WriteFile("c:\\file", content.FileContent), Times.Once());
    }

    [Test]
    public void UndoReplaced()
    {
        var files = new VersionedFile[] { new(123, "c:\\new\\path", "new\\path", 10, FileActionKind.Replace) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[] { 123 }))
            .Returns(new ActualFileInfoPoco[] { new() { UniqueFileId = 123, RelativePath = "old\\path" } });
        _pathResolver.Setup(x => x.RelativePathToFull("old\\path")).Returns("c:\\old\\path");

        _undoLogic.UndoChanges(files);

        _fileSystem.Verify(x => x.MoveFile("c:\\new\\path", "c:\\old\\path"), Times.Once());
    }

    [Test]
    public void UndoModifiedAndReplaced()
    {
        var files = new VersionedFile[] { new(123, "c:\\new\\path", "new\\path", 10, FileActionKind.ModifyAndReplace) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[] { 123 }))
            .Returns(new ActualFileInfoPoco[] { new() { FileId = 1, UniqueFileId = 123, RelativePath = "old\\path" } });
        var content = new FileContentPoco { FileContent = new byte[] { 1, 2, 3 } };
        _repository.Setup(x => x.GetActualFileContent(1)).Returns(content);
        _pathResolver.Setup(x => x.RelativePathToFull("old\\path")).Returns("c:\\old\\path");

        _undoLogic.UndoChanges(files);

        _fileSystem.Verify(x => x.WriteFile("c:\\new\\path", content.FileContent), Times.Once());
        _fileSystem.Verify(x => x.MoveFile("c:\\new\\path", "c:\\old\\path"), Times.Once());
    }

    [Test]
    public void UndoDeleted()
    {
        var files = new VersionedFile[] { new(123, "c:\\file", "file", 10, FileActionKind.Delete) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[] { 123 }))
            .Returns(new ActualFileInfoPoco[] { new() { FileId = 1, UniqueFileId = 123, RelativePath = "file", Size = 10 } });
        var content = new FileContentPoco { FileContent = new byte[] { 1, 2, 3 } };
        _repository.Setup(x => x.GetActualFileContent(1)).Returns(content);
        _fileSystem.Setup(x => x.GetFileInformation("c:\\file")).Returns(new FileInformation(456, "c:\\file", 10, 0, 0));

        _undoLogic.UndoChanges(files);

        _fileSystem.Verify(x => x.WriteFile("c:\\file", content.FileContent), Times.Once());
        _repository.Verify(x => x.DeleteActualFileInfo(new ulong[] { 123 }), Times.Once());
        _repository.Verify(x => x.SaveActualFileInfo(new ActualFileInfoPoco[] { new() { FileId = 1, UniqueFileId = 456, RelativePath = "file", Size = 10 } }), Times.Once());
    }

    [Test]
    public void WrongActionKind()
    {
        var files = new VersionedFile[] { new(123, "c:\\file", "file", 10, (FileActionKind)99) };
        _repository.Setup(x => x.GetActualFileInfoByUniqueId(new ulong[0])).Returns(new ActualFileInfoPoco[0]);
        try
        {
            _undoLogic.UndoChanges(files);
            Assert.Fail();
        }
        catch (ArgumentException)
        {
            Assert.Pass();
        }
    }
}
