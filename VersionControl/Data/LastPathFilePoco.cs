namespace VersionControl.Data;

internal class LastPathFilePoco
{
    public ulong UniqueId { get; set; }

    public string Path { get; set; } = "";

    public override bool Equals(object? obj)
    {
        return obj is LastPathFilePoco poco &&
               UniqueId == poco.UniqueId &&
               Path == poco.Path;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId, Path);
    }
}
