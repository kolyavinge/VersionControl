namespace VersionControl.Data;

internal class ReplaceFileActionPoco
{
    public uint Id { get; set; }

    public string RelativePath { get; set; } = "";

    public override bool Equals(object? obj)
    {
        return obj is ReplaceFileActionPoco poco &&
               Id == poco.Id &&
               RelativePath == poco.RelativePath;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, RelativePath);
    }
}
