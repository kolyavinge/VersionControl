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
    private string _repositoryPath;
    private Mock<IDataRepository> _dataRepository;
    private Mock<IPathResolver> _pathResolver;
    private Mock<IFileSystem> _fileSystem;
    private Status _status;

    [SetUp]
    public void Setup()
    {
        _created = new DateTime(2000, 1, 1);
        _createdBinary = _created.ToFileTimeUtc();
        _projectPath = "c:\\";
        _repositoryPath = "c:\\.vc";
        _dataRepository = new Mock<IDataRepository>();
        _pathResolver = new Mock<IPathResolver>();
        _fileSystem = new Mock<IFileSystem>();
        _status = new Status(_projectPath, _repositoryPath, _dataRepository.Object, _pathResolver.Object, _fileSystem.Object);
    }

    [Test]
    public void GetStatus_NoLastCommitEmptyProject()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(Enumerable.Empty<LastPathFilePoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(Enumerable.Empty<string>());

        var result = _status.GetStatus().ToList();

        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetStatus_IgnoreRepositoryFiles()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(Enumerable.Empty<LastPathFilePoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\projectFile", "c:\\.vc\\fileInRepo" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\projectFile")).Returns(new FileInformation(1, "c:\\projectFile", 100, 100));

        var result = _status.GetStatus().ToList();

        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\projectFile", FileActionKind.Add)));
    }

    [Test]
    public void GetStatus_NoLastCommitProjectWithFiles()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns((CommitPoco)null);
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(Enumerable.Empty<LastPathFilePoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\added", "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\added")).Returns(new FileInformation(1, "c:\\added", 100, 100));
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", 100, 200));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\added", FileActionKind.Add)));
        Assert.That(result[1], Is.EqualTo(new VersionedFile(2, "c:\\modified", FileActionKind.Add)));
    }

    [Test]
    public void GetStatus_WithLastCommitOldUnmodifiedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(new LastPathFilePoco[]
        {
            new() { UniqueId = 1, Path = "old" }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\old" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\old")).Returns(new FileInformation(1, "c:\\old", _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\old")).Returns("old");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(0));
    }

    [Test]
    public void GetStatus_WithLastCommitAddedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(Enumerable.Empty<LastPathFilePoco>());
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\added" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\added")).Returns(new FileInformation(1, "c:\\added", _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\added")).Returns("added");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(1, "c:\\added", FileActionKind.Add)));
    }

    [Test]
    public void GetStatus_WithLastCommitModifiedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(new LastPathFilePoco[]
        {
            new() { UniqueId = 2, Path = "modified" }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\modified" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\modified")).Returns(new FileInformation(2, "c:\\modified", _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\modified")).Returns("modified");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(2, "c:\\modified", FileActionKind.Modify)));
    }

    [Test]
    public void GetStatus_WithLastCommitReplacedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(new LastPathFilePoco[]
        {
            new() { UniqueId = 3, Path = "old_replaced" }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\new_replaced" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\new_replaced")).Returns(new FileInformation(3, "c:\\new_replaced", _createdBinary, _createdBinary));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\new_replaced")).Returns("new_replaced");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(3, "c:\\new_replaced", FileActionKind.Replace)));
    }

    [Test]
    public void GetStatus_WithLastCommitModifiedReplacedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(new LastPathFilePoco[]
        {
            new() { UniqueId = 4, Path = "old_modifiedReplaced" }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(new string[] { "c:\\new_modifiedReplaced" });
        _fileSystem.Setup(x => x.GetFileInformation("c:\\new_modifiedReplaced")).Returns(new FileInformation(4, "c:\\new_modifiedReplaced", _createdBinary, _createdBinary + 100));
        _pathResolver.Setup(x => x.FullPathToRelative("c:\\new_modifiedReplaced")).Returns("new_modifiedReplaced");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(4, "c:\\new_modifiedReplaced", FileActionKind.ModifyAndReplace)));
    }

    [Test]
    public void GetStatus_WithLastCommitDeletedFile()
    {
        _dataRepository.Setup(x => x.GetLastCommit()).Returns(new CommitPoco { CreatedUtc = _created });
        _dataRepository.Setup(x => x.GetLastPathFiles()).Returns(new LastPathFilePoco[]
        {
            new() { UniqueId = 5, Path = "deleted" }
        });
        _fileSystem.Setup(x => x.GetFilesRecursively(_projectPath)).Returns(Enumerable.Empty<string>());
        _pathResolver.Setup(x => x.RelativePathToFull("deleted")).Returns("c:\\deleted");

        var result = _status.GetStatus().ToList();

        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new VersionedFile(5, "c:\\deleted", FileActionKind.Delete)));
    }
};
