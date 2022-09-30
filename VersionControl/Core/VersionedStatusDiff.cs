using System.Collections.Generic;
using System.Linq;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

public class VersionedStatusDiff
{
	public static VersionedStatusDiff MakeDiff(VersionedStatus oldStatus, VersionedStatus newStatus)
	{
		var equals = oldStatus.Equals(newStatus);
		if (equals) return new();

		var added = new HashSet<VersionedFile>(newStatus.Files);
		added.RemoveRange(oldStatus.Files);

		var deleted = new HashSet<VersionedFile>(oldStatus.Files);
		deleted.RemoveRange(newStatus.Files);

		return new(added, deleted);
	}

	public bool NoDifference => !Added.Any() && !Deleted.Any();

	public IReadOnlyCollection<VersionedFile> Added { get; }

	public IReadOnlyCollection<VersionedFile> Deleted { get; }

	internal VersionedStatusDiff(IReadOnlyCollection<VersionedFile> added, IReadOnlyCollection<VersionedFile> deleted)
	{
		Added = added;
		Deleted = deleted;
	}

	internal VersionedStatusDiff() : this(new VersionedFile[0], new VersionedFile[0]) { }
}
