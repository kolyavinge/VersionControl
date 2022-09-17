using System.Linq;

namespace VersionControl.Data;

internal class AddFileActionPoco
{
    public uint Id { get; set; }

    public string RelativePath { get; set; } = "";

    public byte[] FileContent { get; set; } = new byte[0];

    public override bool Equals(object? obj)
    {
        return obj is AddFileActionPoco poco &&
               Id == poco.Id &&
               RelativePath == poco.RelativePath &&
               FileContent.SequenceEqual(poco.FileContent);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, RelativePath, FileContent);
    }
}
