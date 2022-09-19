using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;

namespace VersionControl.Tests.Data;

internal class DataRepositoryIntegration
{
    private string _repositoryPath;
    private Mock<IPathHolder> _pathHolder;
    private DataRepository _dataRepository;

    [SetUp]
    public void Setup()
    {
        _repositoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestDatabase");
        if (Directory.Exists(_repositoryPath)) Directory.Delete(_repositoryPath, true);
        Directory.CreateDirectory(_repositoryPath);
        _pathHolder = new Mock<IPathHolder>();
        _pathHolder.SetupGet(x => x.RepositoryPath).Returns(_repositoryPath);
        _dataRepository = new DataRepository(_pathHolder.Object);
    }

    [Test]
    public void GetFileByUniqueId()
    {
        _dataRepository.SaveFiles(new FilePoco[]
        {
            new() { Id = 1, UniqueFileId = 100 },
            new() { Id = 2, UniqueFileId = 200 },
            new() { Id = 3, UniqueFileId = 300 }
        });

        var result = _dataRepository.GetFileByUniqueId(200);

        Assert.That(result, Is.EqualTo(new FilePoco { Id = 2, UniqueFileId = 200 }));
    }

    [Test]
    public void ClearUniqueFileIdFor()
    {
        _dataRepository.SaveFiles(new FilePoco[]
        {
            new() { Id = 1, UniqueFileId = 100 },
            new() { Id = 2, UniqueFileId = 200 },
            new() { Id = 3, UniqueFileId = 300 }
        });

        _dataRepository.ClearUniqueFileIdFor(3);

        var result = _dataRepository.GetFileByUniqueId(0);
        Assert.That(result, Is.EqualTo(new FilePoco { Id = 3, UniqueFileId = 0 }));
    }

    [Test]
    public void GetLastCommit()
    {
        var result = _dataRepository.GetLastCommit();
        Assert.That(result, Is.Null);

        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author", Comment = "first", CreatedUtc = created });
        result = _dataRepository.GetLastCommit();
        Assert.That(result, Is.EqualTo(new CommitPoco { Id = 1, Author = "author", Comment = "first", CreatedUtc = created }));

        _dataRepository.SaveCommit(new() { Id = 2, Author = "author", Comment = "second", CreatedUtc = created });
        result = _dataRepository.GetLastCommit();
        Assert.That(result, Is.EqualTo(new CommitPoco { Id = 2, Author = "author", Comment = "second", CreatedUtc = created }));
    }

    [Test]
    public void FindCommits_Pagination()
    {
        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author", Comment = "1", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 2, Author = "author", Comment = "2", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 3, Author = "author", Comment = "3", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 4, Author = "author", Comment = "4", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 5, Author = "author", Comment = "5", CreatedUtc = created });

        var result = _dataRepository.FindCommits(new() { PageIndex = 0, PageSize = 3 }).ToList();
        Assert.That(result, Has.Count.EqualTo(3));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 1, Author = "author", Comment = "1", CreatedUtc = created }));
        Assert.That(result[1], Is.EqualTo(new CommitPoco { Id = 2, Author = "author", Comment = "2", CreatedUtc = created }));
        Assert.That(result[2], Is.EqualTo(new CommitPoco { Id = 3, Author = "author", Comment = "3", CreatedUtc = created }));

        result = _dataRepository.FindCommits(new() { PageIndex = 1, PageSize = 3 }).ToList();
        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 4, Author = "author", Comment = "4", CreatedUtc = created }));
        Assert.That(result[1], Is.EqualTo(new CommitPoco { Id = 5, Author = "author", Comment = "5", CreatedUtc = created }));
    }

    [Test]
    public void FindCommits_AuthorEquals()
    {
        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author1", Comment = "1", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 2, Author = "author2", Comment = "2", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 3, Author = "author3", Comment = "3", CreatedUtc = created });

        var result = _dataRepository.FindCommits(new() { PageIndex = 0, PageSize = 3, Author = "author2" }).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 2, Author = "author2", Comment = "2", CreatedUtc = created }));
    }

    [Test]
    public void FindCommits_AuthorLike()
    {
        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author1", Comment = "1", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 2, Author = "author2", Comment = "2", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 3, Author = "author3", Comment = "3", CreatedUtc = created });

        var result = _dataRepository.FindCommits(new() { PageIndex = 0, PageSize = 3, Author = "thor3" }).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 3, Author = "author3", Comment = "3", CreatedUtc = created }));
    }

    [Test]
    public void FindCommits_CommentEquals()
    {
        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author1", Comment = "1", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 2, Author = "author2", Comment = "2", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 3, Author = "author3", Comment = "3", CreatedUtc = created });

        var result = _dataRepository.FindCommits(new() { PageIndex = 0, PageSize = 3, Comment = "1" }).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 1, Author = "author1", Comment = "1", CreatedUtc = created }));
    }

    [Test]
    public void FindCommits_CommentLike()
    {
        var created = DateTime.UtcNow;
        _dataRepository.SaveCommit(new() { Id = 1, Author = "author1", Comment = "comment1", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 2, Author = "author2", Comment = "comment2", CreatedUtc = created });
        _dataRepository.SaveCommit(new() { Id = 3, Author = "author3", Comment = "comment3", CreatedUtc = created });

        var result = _dataRepository.FindCommits(new() { PageIndex = 0, PageSize = 3, Comment = "nt1" }).ToList();
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(new CommitPoco { Id = 1, Author = "author1", Comment = "comment1", CreatedUtc = created }));
    }

    [Test]
    public void GetLastPathFiles_Save()
    {
        _dataRepository.SaveActualFileInfo(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 123, FileId = 1, Path = "file1", Size = 123 },
            new() { UniqueId = 321, FileId = 2, Path = "file2", Size = 456 }
        });

        var result = _dataRepository.GetActualFileInfo().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new ActualFileInfoPoco { UniqueId = 123, FileId = 1, Path = "file1", Size = 123 }));
        Assert.That(result[1], Is.EqualTo(new ActualFileInfoPoco { UniqueId = 321, FileId = 2, Path = "file2", Size = 456 }));
    }

    [Test]
    public void GetLastPathFiles_Update()
    {
        _dataRepository.SaveActualFileInfo(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 123, FileId = 1, Path = "file1", Size = 123 },
            new() { UniqueId = 321, FileId = 2, Path = "file2", Size = 456 }
        });
        _dataRepository.UpdateActualFileInfo(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 123, FileId = 10, Path = "file100", Size = 1230 },
            new() { UniqueId = 321, FileId = 20, Path = "file200", Size = 4560 }
        });

        var result = _dataRepository.GetActualFileInfo().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new ActualFileInfoPoco { UniqueId = 123, FileId = 10, Path = "file100", Size = 1230 }));
        Assert.That(result[1], Is.EqualTo(new ActualFileInfoPoco { UniqueId = 321, FileId = 20, Path = "file200", Size = 4560 }));
    }

    [Test]
    public void GetLastPathFiles_Delete()
    {
        _dataRepository.SaveActualFileInfo(new ActualFileInfoPoco[]
        {
            new() { UniqueId = 123, Path = "file1" },
            new() { UniqueId = 321, Path = "file2" }
        });
        _dataRepository.DeleteLastPathFiles(new ulong[] { 123, 321 });

        var result = _dataRepository.GetActualFileInfo().ToList();

        Assert.That(result, Has.Count.EqualTo(0));
    }

    [Test]
    public void GetCommitDetailsCount()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 1, FileActionKind = (byte)FileActionKind.Add, CommitId = 123, FileId = 456 },
            new() { Id = 2, FileActionKind = (byte)FileActionKind.Modify, CommitId = 123, FileId = 789 }
        });

        var result = _dataRepository.GetCommitDetailsCount();

        Assert.That(result, Is.EqualTo(2));
    }

    [Test]
    public void GetCommitDetails()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 1, FileActionKind = (byte)FileActionKind.Add, CommitId = 123, FileId = 456 },
            new() { Id = 2, FileActionKind = (byte)FileActionKind.Modify, CommitId = 123, FileId = 789 }
        });

        var result = _dataRepository.GetCommitDetails(123).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new CommitDetailPoco { Id = 1, FileActionKind = (byte)FileActionKind.Add, CommitId = 123, FileId = 456 }));
        Assert.That(result[1], Is.EqualTo(new CommitDetailPoco { Id = 2, FileActionKind = (byte)FileActionKind.Modify, CommitId = 123, FileId = 789 }));
    }

    [Test]
    public void GetModifyActions()
    {
        var content = new byte[] { 1, 2, 3 };
        _dataRepository.SaveFileContents(new FileContentPoco[]
        {
            new() { Id = 123, FileContent = content },
            new() { Id = 456, FileContent = content }
        });

        var result = _dataRepository.GetFileContents(new uint[] { 123, 456 }).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new FileContentPoco { Id = 123, FileContent = content }));
        Assert.That(result[1], Is.EqualTo(new FileContentPoco { Id = 456, FileContent = content }));
    }

    [Test]
    public void GetReplaceActions()
    {
        _dataRepository.SaveFilePathes(new FilePathPoco[]
        {
            new() { Id = 123, RelativePath = "file1" },
            new() { Id = 456, RelativePath = "file2" }
        });

        var result = _dataRepository.GetFilePathes(new uint[] { 123, 456 }).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new FilePathPoco { Id = 123, RelativePath = "file1" }));
        Assert.That(result[1], Is.EqualTo(new FilePathPoco { Id = 456, RelativePath = "file2" }));
    }

    [Test]
    public void GetFilePathFor_1()
    {
        _dataRepository.SaveFilePathes(new FilePathPoco[]
        {
            new() { Id = 123, FileId = 111 },
            new() { Id = 456, FileId = 111 }
        });

        var result = _dataRepository.GetFilePathFor(456, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetFilePathFor_2()
    {
        _dataRepository.SaveFilePathes(new FilePathPoco[]
        {
            new() { Id = 123, FileId = 111 },
            new() { Id = 456, FileId = 111 },
            new() { Id = 789, FileId = 111 }
        });

        var result = _dataRepository.GetFilePathFor(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(789));
    }

    [Test]
    public void GetFileContentFor_1()
    {
        _dataRepository.SaveFileContents(new FileContentPoco[]
        {
            new() { Id = 123, FileId = 111 },
            new() { Id = 456, FileId = 111 }
        });

        var result = _dataRepository.GetFileContentFor(456, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetFileContentFor_2()
    {
        _dataRepository.SaveFileContents(new FileContentPoco[]
        {
            new() { Id = 123, FileId = 111 },
            new() { Id = 456, FileId = 111 },
            new() { Id = 789, FileId = 111 }
        });

        var result = _dataRepository.GetFileContentFor(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(789));
    }

    [Test]
    public void GetActualFileContent()
    {
        _dataRepository.SaveFileContents(new FileContentPoco[]
        {
            new() { Id = 123, FileId = 111, FileContent = new byte[] { 1 } },
            new() { Id = 456, FileId = 111, FileContent = new byte[] { 1 } },
            new() { Id = 789, FileId = 222, FileContent = new byte[] { 1 } }
        });

        var result = _dataRepository.GetActualFileContent(111);

        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0], Is.EqualTo(1));
    }
}
