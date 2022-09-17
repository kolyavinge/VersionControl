using NUnit.Framework;
using VersionControl.Core;

namespace VersionControl.Tests.Core;

internal class PathResolverTest
{
    [Test]
    public void FullPathToRelative_NoLastSlash()
    {
        var resolver = new PathResolver("C:\\project");

        var result = resolver.FullPathToRelative("C:\\project\\file");

        Assert.That(result, Is.EqualTo("file"));
    }

    [Test]
    public void FullPathToRelative_WithLastSlash()
    {
        var resolver = new PathResolver("C:\\project\\");

        var result = resolver.FullPathToRelative("C:\\project\\file");

        Assert.That(result, Is.EqualTo("file"));
    }

    [Test]
    public void RelativePathToFull_NoLastSlash()
    {
        var resolver = new PathResolver("C:\\project");

        var result = resolver.RelativePathToFull("file");

        Assert.That(result, Is.EqualTo("C:\\project\\file"));
    }

    [Test]
    public void RelativePathToFull_WithLastSlash()
    {
        var resolver = new PathResolver("C:\\project\\");

        var result = resolver.RelativePathToFull("file");

        Assert.That(result, Is.EqualTo("C:\\project\\file"));
    }
}
