namespace VersionControl.Data;

internal class FilePoco
{
    public uint Id { get; set; }

    public override bool Equals(object? obj)
    {
        return obj is FilePoco poco &&
               Id == poco.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id);
    }
}
