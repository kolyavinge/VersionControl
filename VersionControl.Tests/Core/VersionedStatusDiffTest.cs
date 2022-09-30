using NUnit.Framework;
using VersionControl.Core;

namespace VersionControl.Tests.Core;

internal class VersionedStatusDiffTest
{
	[Test]
	public void MakeDiff_NoDifference()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add), new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });
		var newStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add), new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference);
	}

	[Test]
	public void MakeDiff_NoDifference_NoOrder()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add), new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });
		var newStatus = new VersionedStatus(new VersionedFile[] { new(2, @"c:\file2", "file2", 10, FileActionKind.Modify), new(1, @"c:\file1", "file1", 10, FileActionKind.Add) });

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference);
	}

	[Test]
	public void MakeDiff_Added_1()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[0]);
		var newStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add) });

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference, Is.False);
		Assert.That(result.Added, Has.Count.EqualTo(1));
		Assert.That(result.Deleted, Has.Count.EqualTo(0));
	}

	[Test]
	public void MakeDiff_Added_2()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[] { new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });
		var newStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add), new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference, Is.False);
		Assert.That(result.Added, Has.Count.EqualTo(1));
		Assert.That(result.Deleted, Has.Count.EqualTo(0));
	}

	[Test]
	public void MakeDiff_Deleted_1()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add) });
		var newStatus = new VersionedStatus(new VersionedFile[0]);

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference, Is.False);
		Assert.That(result.Added, Has.Count.EqualTo(0));
		Assert.That(result.Deleted, Has.Count.EqualTo(1));
	}

	[Test]
	public void MakeDiff_Deleted_2()
	{
		var oldStatus = new VersionedStatus(new VersionedFile[] { new(1, @"c:\file1", "file1", 10, FileActionKind.Add), new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });
		var newStatus = new VersionedStatus(new VersionedFile[] { new(2, @"c:\file2", "file2", 10, FileActionKind.Modify) });

		var result = VersionedStatusDiff.MakeDiff(oldStatus, newStatus);

		Assert.That(result.NoDifference, Is.False);
		Assert.That(result.Added, Has.Count.EqualTo(0));
		Assert.That(result.Deleted, Has.Count.EqualTo(1));
	}
}
