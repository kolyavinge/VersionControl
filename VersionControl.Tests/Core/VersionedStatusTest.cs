using System.Linq;
using NUnit.Framework;
using VersionControl.Core;

namespace VersionControl.Tests.Core;

internal class VersionedStatusTest
{
    [Test]
    public void GetHashCode_NoOrder()
    {
        var files1 = Enumerable.Range(0, 1000).Select(n => new VersionedFile((ulong)n, @"c:\file", "file", 10, FileActionKind.Add)).ToList();
        var files2 = Enumerable.Range(0, 1000).Select(n => new VersionedFile((ulong)n, @"c:\file", "file", 10, FileActionKind.Add)).Reverse().ToList();
        var status1 = new VersionedStatus(files1);
        var status2 = new VersionedStatus(files2);

        var hashCode1 = status1.GetHashCode();
        var hashCode2 = status2.GetHashCode();

        Assert.That(hashCode1, Is.EqualTo(hashCode2));
    }

    [Test]
    public void Equals_Null()
    {
        var status = new VersionedStatus(new[] { new VersionedFile(1, @"c:\file", "file", 10, FileActionKind.Add) });

        var result = status.Equals(null);

        Assert.That(result, Is.False);
    }

    [Test]
    public void Equals_NoOrder()
    {
        var files1 = Enumerable.Range(0, 1000).Select(n => new VersionedFile((ulong)n, @"c:\file", "file", 10, FileActionKind.Add)).ToList();
        var files2 = Enumerable.Range(0, 1000).Select(n => new VersionedFile((ulong)n, @"c:\file", "file", 10, FileActionKind.Add)).Reverse().ToList();
        var status1 = new VersionedStatus(files1);
        var status2 = new VersionedStatus(files2);

        var result = status1.Equals(status2);

        Assert.That(result);
    }
}
