namespace VersionControl.Data;

internal class CommitPoco
{
    public long Id { get; set; }

    public string Author { get; set; } = "";

    public string Comment { get; set; } = "";

    public DateTime CreatedUtc { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is CommitPoco poco &&
               Id == poco.Id &&
               Author == poco.Author &&
               Comment == poco.Comment &&
               CreatedUtc == poco.CreatedUtc;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Author, Comment, CreatedUtc);
    }
}
