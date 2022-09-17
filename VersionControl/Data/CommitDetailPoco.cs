namespace VersionControl.Data;

internal class CommitDetailPoco
{
    public uint Id { get; set; }

    public long CommitId { get; set; }

    public uint FileId { get; set; }

    public byte FileActionKind { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is CommitDetailPoco poco &&
               Id == poco.Id &&
               CommitId == poco.CommitId &&
               FileId == poco.FileId &&
               FileActionKind == poco.FileActionKind;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, CommitId, FileId, FileActionKind);
    }
}
