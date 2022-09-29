using System;
using System.Linq;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Tests.Core;

public class StatusTest
{
    private DateTime _created;
    private long _createdBinary;
    private string _projectPath;
    private Mock<IPathHolder> _pathHolder;
    private Mock<IDataRepository> _dataRepository;
    private Mock<IPathResolver> _pathResolver;
    private Mock<IFileComparator> _fileComparator;
    private Mock<IFileSystem> _fileSystem;
    private Status _status;

    [SetUp]
    public void Setup()
    {
        _created = new DateTime(2000, 1, 1);
        _createdBinary = _created.ToFileTimeUtc();
        _projectPath = "c:\\";
        _pathHolder = new Mock<IPathHolder>();
        _pathHolder.SetupGet(x => x.ProjectPath).Returns(_projectPath);
        _pathHolder.SetupGet(x => x.RepositoryPath).Returns("c:\\.vc");
        _dataRepository = new Mock<IDataRepository>();
        _pathResolver = new Mock<IPathResolver>();
        _fileComparator = new Mock<IFileComparator>();
        _fileSystem = new Mock<IFileSystem>();
        _status = new Status(_pathHolder.Object, _dataRepository.Object, _pathResolver.Object, _fileComparator.Object, _fileSystem.Object);
    }

    [Test]
    public void GetStatus_NoLastCommitEmptyProject()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(Enumerable.Empty<ActualFileInfoPoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(Enumerable.Empty<string>());

        var result = _status.GetStatus().ToList();

        Assert.That(result, Is.Empty);
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_IgnoreRepositoryFiles()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(Enumerable.Empty<ActualFileInfoPoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\projectFile", "c:\\.vc\\fileInRepo" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\projectFile")).Returns(new FileInformation(1, "c:\\projectFile", 128, 100, 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\projectFile")).Returns("projectFile");

        var result = _status.GetStatus().ToList();

        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\projectFile", "projectFile", 128, FileActionKind.Add)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_NoLastCommitProjectWithFiles()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(Enumerable.Empty<ActualFileInfoPoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\added", "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\added")).Returns(new FileInformation(1, "c:\\added", 128, 100, 100));
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", 128, 100, 200));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\added", "added", 128, FileActionKind.Add)));
        Assert.That(result[1], Is.EqualTo(new VersionedFile(2, "c:\\modified", "modified", 128, FileActionKind.Add)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_OldUnmodifiedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 1, RelativePath = "old", Size = 128 }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\old" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\old")).Returns(new FileInformation(1, "c:\\old", 128, _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\old")).Returns("old");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(0));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_AddedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(Enumerable.Empty<ActualFileInfoPoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\added" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\added")).Returns(new FileInformation(1, "c:\\added", 128, _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\added", "added", 128, FileActionKind.Add)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_ModifiedFile_DifferentSizes()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 2, RelativePath = "modified", Size = 512 }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", 128, _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(2, "c:\\modified", "modified", 128, FileActionKind.Modify)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_ModifiedFile_DifferentContents()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 2, FileId = 111, RelativePath = "modified", Size = 128 }
        });
        _dataRepository.Setup(x => x.GetActualFileContent(111)).Returns(new byte[] { 1, 2, 3 });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", 128, _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");
        _fileComparator.Setup(x => x.AreEqual(new byte[] { 1, 2, 3 }, "c:\\modified")).Returns(false);

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(2, "c:\\modified", "modified", 128, FileActionKind.Modify)));
        _fileComparator.Verify(x => x.AreEqual(new byte[] { 1, 2, 3 }, "c:\\modified"), Times.Exactly(1));
    }

    [Test]
    public void GetStatus_ModifiedFileAndUndo_SameSizesAndContents()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 2, FileId = 111, RelativePath = "modified", Size = 128 }
        });
        _dataRepository.Setup(x => x.GetActualFileContent(111)).Returns(new byte[] { 1, 2, 3 });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", 128, _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");
        _fileComparator.Setup(x => x.AreEqual(new byte[] { 1, 2, 3 }, "c:\\modified")).Returns(true);

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(0));
        _fileComparator.Verify(x => x.AreEqual(new byte[] { 1, 2, 3 }, "c:\\modified"), Times.Exactly(1));
    }

    [Test]
    public void GetStatus_ReplacedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 3, RelativePath = "old_replaced", Size = 128 }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\new_replaced" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\new_replaced")).Returns(new FileInformation(3, "c:\\new_replaced", 128, _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\new_replaced")).Returns("new_replaced");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(3, "c:\\new_replaced", "new_replaced", 128, FileActionKind.Replace)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_ModifiedReplacedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 4, RelativePath = "old_modifiedReplaced", Size = 512 }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\new_modifiedReplaced" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\new_modifiedReplaced")).Returns(new FileInformation(4, "c:\\new_modifiedReplaced", 128, _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\new_modifiedReplaced")).Returns("new_modifiedReplaced");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(4, "c:\\new_modifiedReplaced", "new_modifiedReplaced", 128, FileActionKind.ModifyAndReplace)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }

    [Test]
    public void GetStatus_DeletedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetActualFileInfo()).Returns(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 5, RelativePath = "deleted", Size = 128 }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(Enumerable.Empty<string>());
        _pathResolver.Setup(x => x.RelativePathToFull("deleted")).Returns("c:\\deleted");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(5, "c:\\deleted", "deleted", 0, FileActionKind.Delete)));
        _fileComparator.Verify(x => x.AreEqual(It.IsAny<byte[]>(), It.IsAny<string>()), Times.Never());
    }
};
