namespace VersionControl.Data;

internal class ActualFileInfoPoco
{
    public ulong UniqueId { get; set; }

    public uint FileId { get; set; }

    public string Path { get; set; } = "";

    public ulong Size { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ActualFileInfoPoco poco &&
               UniqueId == poco.UniqueId &&
               FileId == poco.FileId &&
               Path == poco.Path &&
               Size == poco.Size;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId, FileId, Path, Size);
    }
}
