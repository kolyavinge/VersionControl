namespace VersionControl.Data;

internal class VersionedFilePoco
{
    public uint Id { get; set; }

    public ulong UniqueFileId { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is VersionedFilePoco poco &&
               Id == poco.Id &&
               UniqueFileId == poco.UniqueFileId;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, UniqueFileId);
    }
}
