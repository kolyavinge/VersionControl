using System;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Data;

namespace VersionControl.Tests.Data;

internal class DataRepositoryIntegration
{
    private string _repositoryPath;
    private DataRepository _dataRepository;

    [SetUp]
    public void Setup()
    {
        _repositoryPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "TestDatabase");
        if (Directory.Exists(_repositoryPath)) Directory.Delete(_repositoryPath, true);
        Directory.CreateDirectory(_repositoryPath);
        _dataRepository = new DataRepository(_repositoryPath);
    }

    [Test]
    public void GetFileByUniqueId()
    {
        _dataRepository.SaveVersionedFiles(new VersionedFilePoco[]
        {
            new() { Id = 1, UniqueFileId = 100 },
            new() { Id = 2, UniqueFileId = 200 },
            new() { Id = 3, UniqueFileId = 300 }
        });

        var result = _dataRepository.GetFileByUniqueId(200);

        Assert.That(result, Is.EqualTo(new VersionedFilePoco { Id = 2, UniqueFileId = 200 }));
    }

    [Test]
    public void ClearUniqueFileIdFor()
    {
        _dataRepository.SaveVersionedFiles(new VersionedFilePoco[]
        {
            new() { Id = 1, UniqueFileId = 100 },
            new() { Id = 2, UniqueFileId = 200 },
            new() { Id = 3, UniqueFileId = 300 }
        });

        _dataRepository.ClearUniqueFileIdFor(3);

        var result = _dataRepository.GetFileByUniqueId(0);
        Assert.That(result, Is.EqualTo(new VersionedFilePoco { Id = 3, UniqueFileId = 0 }));
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
        _dataRepository.SaveLastPathFiles(new LastPathFilePoco[]
        {
            new() { UniqueId = 123, Path = "file1" },
            new() { UniqueId = 321, Path = "file2" }
        });

        var result = _dataRepository.GetLastPathFiles().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new LastPathFilePoco { UniqueId = 123, Path = "file1" }));
        Assert.That(result[1], Is.EqualTo(new LastPathFilePoco { UniqueId = 321, Path = "file2" }));
    }

    [Test]
    public void GetLastPathFiles_Update()
    {
        _dataRepository.SaveLastPathFiles(new LastPathFilePoco[]
        {
            new() { UniqueId = 123, Path = "file1" },
            new() { UniqueId = 321, Path = "file2" }
        });
        _dataRepository.UpdateLastPathFiles(new LastPathFilePoco[]
        {
            new() { UniqueId = 123, Path = "file100" },
            new() { UniqueId = 321, Path = "file200" }
        });

        var result = _dataRepository.GetLastPathFiles().ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new LastPathFilePoco { UniqueId = 123, Path = "file100" }));
        Assert.That(result[1], Is.EqualTo(new LastPathFilePoco { UniqueId = 321, Path = "file200" }));
    }

    [Test]
    public void GetLastPathFiles_Delete()
    {
        _dataRepository.SaveLastPathFiles(new LastPathFilePoco[]
        {
            new() { UniqueId = 123, Path = "file1" },
            new() { UniqueId = 321, Path = "file2" }
        });
        _dataRepository.DeleteLastPathFiles(new ulong[] { 123, 321 });

        var result = _dataRepository.GetLastPathFiles().ToList();

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
    public void GetAddActions()
    {
        var content = new byte[] { 1, 2, 3 };
        _dataRepository.SaveAddFileActions(new AddFileActionPoco[]
        {
            new() { Id = 123, FileContent = content, RelativePath = "file1" },
            new() { Id = 456, FileContent = content, RelativePath = "file2" }
        });

        var result = _dataRepository.GetAddActions(new uint[] { 123, 456 }).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new AddFileActionPoco { Id = 123, FileContent = content, RelativePath = "file1" }));
        Assert.That(result[1], Is.EqualTo(new AddFileActionPoco { Id = 456, FileContent = content, RelativePath = "file2" }));
    }

    [Test]
    public void GetModifyActions()
    {
        var content = new byte[] { 1, 2, 3 };
        _dataRepository.SaveModifyFileActions(new ModifyFileActionPoco[]
        {
            new() { Id = 123, FileContent = content },
            new() { Id = 456, FileContent = content }
        });

        var result = _dataRepository.GetModifyActions(new uint[] { 123, 456 }).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new ModifyFileActionPoco { Id = 123, FileContent = content }));
        Assert.That(result[1], Is.EqualTo(new ModifyFileActionPoco { Id = 456, FileContent = content }));
    }

    [Test]
    public void GetReplaceActions()
    {
        _dataRepository.SaveReplaceFileActions(new ReplaceFileActionPoco[]
        {
            new() { Id = 123, RelativePath = "file1" },
            new() { Id = 456, RelativePath = "file2" }
        });

        var result = _dataRepository.GetReplaceActions(new uint[] { 123, 456 }).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result[0], Is.EqualTo(new ReplaceFileActionPoco { Id = 123, RelativePath = "file1" }));
        Assert.That(result[1], Is.EqualTo(new ReplaceFileActionPoco { Id = 456, RelativePath = "file2" }));
    }

    [Test]
    public void GetLastCommitDetailForReplace_1()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Add }
        });

        var result = _dataRepository.GetLastCommitDetailForReplace(123, 111);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLastCommitDetailForReplace_2()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Replace },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.Replace }
        });

        var result = _dataRepository.GetLastCommitDetailForReplace(456, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetLastCommitDetailForReplace_3()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Replace },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.Replace },
            new() { Id = 789, FileId = 111, FileActionKind = (byte)FileActionKind.Modify }
        });

        var result = _dataRepository.GetLastCommitDetailForReplace(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetLastCommitDetailForReplace_4()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Replace },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.ModifyAndReplace },
            new() { Id = 789, FileId = 111, FileActionKind = (byte)FileActionKind.Modify }
        });

        var result = _dataRepository.GetLastCommitDetailForReplace(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetLastCommitDetailForModify_1()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Add }
        });

        var result = _dataRepository.GetLastCommitDetailForModify(123, 111);

        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetLastCommitDetailForModify_2()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Modify },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.Modify }
        });

        var result = _dataRepository.GetLastCommitDetailForModify(456, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetLastCommitDetailForModify_3()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Modify },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.Modify },
            new() { Id = 789, FileId = 111, FileActionKind = (byte)FileActionKind.Replace }
        });

        var result = _dataRepository.GetLastCommitDetailForModify(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }

    [Test]
    public void GetLastCommitDetailForModify_4()
    {
        _dataRepository.SaveCommitDetails(new CommitDetailPoco[]
        {
            new() { Id = 123, FileId = 111, FileActionKind = (byte)FileActionKind.Modify },
            new() { Id = 456, FileId = 111, FileActionKind = (byte)FileActionKind.ModifyAndReplace },
            new() { Id = 789, FileId = 111, FileActionKind = (byte)FileActionKind.Replace }
        });

        var result = _dataRepository.GetLastCommitDetailForModify(789, 111);

        Assert.That(result, !Is.Null);
        Assert.That(result.Id, Is.EqualTo(456));
    }
}
