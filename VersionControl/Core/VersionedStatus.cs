using System.Collections.Generic;
using System.Linq;

namespace VersionControl.Core;

public class VersionedStatus
{
    public static readonly VersionedStatus Empty = new(new VersionedFile[0]);

    public IReadOnlyCollection<VersionedFile> Files { get; }

    public VersionedStatus(IReadOnlyCollection<VersionedFile> files)
    {
        Files = files;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionedStatus result &&
               Files.Count == result.Files.Count &&
               Files.OrderBy(x => x.UniqueId).SequenceEqual(result.Files.OrderBy(x => x.UniqueId));
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var file in Files.OrderBy(x => x.UniqueId))
        {
            hashCode.Add(file);
        }

        return hashCode.ToHashCode();
    }
}
