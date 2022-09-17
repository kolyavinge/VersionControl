using System.Linq;

namespace VersionControl.Data;

internal class ModifyFileActionPoco
{
    public uint Id { get; set; }

    public byte[] FileContent { get; set; } = new byte[0];

    public override bool Equals(object? obj)
    {
        return obj is ModifyFileActionPoco poco &&
               Id == poco.Id &&
               FileContent.SequenceEqual(poco.FileContent);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FileContent);
    }
}
