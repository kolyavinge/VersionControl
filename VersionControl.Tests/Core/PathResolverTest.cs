using Moq;
using NUnit.Framework;
using VersionControl.Core;

namespace VersionControl.Tests.Core;

internal class PathResolverTest
{
    private Mock<IPathHolder> _pathHolder;
    private PathResolver _resolver;

    [SetUp]
    public void Setup()
    {
        _pathHolder = new Mock<IPathHolder>();
        _pathHolder.SetupGet(x => x.ProjectPath).Returns("C:\\project");
        _resolver = new PathResolver(_pathHolder.Object);
    }

    [Test]
    public void FullPathToRelative_NoLastSlash()
    {
        var result = _resolver.FullPathToRelative("C:\\project\\file");

        Assert.That(result, Is.EqualTo("file"));
    }

    [Test]
    public void FullPathToRelative_WithLastSlash()
    {
        var result = _resolver.FullPathToRelative("C:\\project\\file");

        Assert.That(result, Is.EqualTo("file"));
    }

    [Test]
    public void RelativePathToFull_NoLastSlash()
    {
        var result = _resolver.RelativePathToFull("file");

        Assert.That(result, Is.EqualTo("C:\\project\\file"));
    }

    [Test]
    public void RelativePathToFull_WithLastSlash()
    {
        var result = _resolver.RelativePathToFull("file");

        Assert.That(result, Is.EqualTo("C:\\project\\file"));
    }
}
