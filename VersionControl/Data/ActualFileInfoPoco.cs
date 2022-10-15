namespace VersionControl.Data;

internal class ActualFileInfoPoco
{
    public ulong UniqueFileId { get; set; }

    public uint FileId { get; set; }

    public string RelativePath { get; set; } = "";

    public ulong Size { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is ActualFileInfoPoco poco &&
               UniqueFileId == poco.UniqueFileId &&
               FileId == poco.FileId &&
               RelativePath == poco.RelativePath &&
               Size == poco.Size;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueFileId, FileId, RelativePath, Size);
    }
}
