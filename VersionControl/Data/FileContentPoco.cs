using System.Linq;

namespace VersionControl.Data;

internal class FileContentPoco
{
    public uint Id { get; set; }

    public uint FileId { get; set; }

    public byte[] FileContent { get; set; } = new byte[0];

    public override bool Equals(object? obj)
    {
        return obj is FileContentPoco poco &&
               Id == poco.Id &&
               FileId == poco.FileId &&
               FileContent.SequenceEqual(poco.FileContent);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, FileId, FileContent);
    }
}
