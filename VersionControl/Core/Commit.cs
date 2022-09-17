namespace VersionControl.Core;

public class Commit
{
    public long Id { get; }

    public string Author { get; }

    public string Comment { get; }

    public DateTime CreatedUtc { get; }

    internal Commit(long id, string author, string comment, DateTime createdUtc)
    {
        Id = id;
        Author = author;
        Comment = comment;
        CreatedUtc = createdUtc;
    }

    public override bool Equals(object? obj)
    {
        return obj is Commit commit &&
               Id == commit.Id &&
               Author == commit.Author &&
               Comment == commit.Comment &&
               CreatedUtc == commit.CreatedUtc;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Author, Comment, CreatedUtc);
    }
}

public class CommitDetail
{
    internal uint Id { get; }

    internal uint FileId { get; }

    public FileActionKind FileActionKind { get; }

    public string RelativePath { get; } = "";

    internal CommitDetail(uint id, FileActionKind fileActionKind, uint fileId, string relativePath)
    {
        Id = id;
        FileActionKind = fileActionKind;
        FileId = fileId;
        RelativePath = relativePath;
    }

    public override bool Equals(object? obj)
    {
        return obj is CommitDetail detail &&
               Id == detail.Id &&
               FileId == detail.FileId &&
               FileActionKind == detail.FileActionKind &&
               RelativePath == detail.RelativePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FileId, FileActionKind, RelativePath);
    }
}
