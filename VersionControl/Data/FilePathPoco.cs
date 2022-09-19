namespace VersionControl.Data;

internal class FilePathPoco
{
    public uint Id { get; set; }

    public uint FileId { get; set; }

    public string RelativePath { get; set; } = "";

    public override bool Equals(object? obj)
    {
        return obj is FilePathPoco poco &&
               Id == poco.Id &&
               FileId == poco.FileId &&
               RelativePath == poco.RelativePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FileId, RelativePath);
    }
}
