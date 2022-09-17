using System.Text;
using Moq;
using NUnit.Framework;
using VersionControl.Core;
using VersionControl.Infrastructure;

namespace VersionControl.Tests.Core;

internal class SettingsTest
{
    private string _repositoryPath;
    private Mock<ISerializer> _serializer;
    private Mock<IFileSystem> _fileSystem;
    private Mock<IWindowsEnvironment> _windowsEnvironment;
    private Settings _settings;

    [SetUp]
    public void Setup()
    {
        _repositoryPath = "repository path";
        _serializer = new Mock<ISerializer>();
        _fileSystem = new Mock<IFileSystem>();
        _windowsEnvironment = new Mock<IWindowsEnvironment>();
        _windowsEnvironment.SetupGet(x => x.UserName).Returns("user name");
    }

    [Test]
    public void CreateSettingFile()
    {
        _fileSystem.Setup(x => x.IsFileExist("repository path\\settings")).Returns(false);
        var content = new SettingsContent
        {
            Author = "user name"
        };
        _serializer.Setup(x => x.Serialize(content)).Returns("serialized content");

        _settings = new Settings(_repositoryPath, _serializer.Object, _fileSystem.Object, _windowsEnvironment.Object);

        Assert.That(_settings.Author, Is.EqualTo("user name"));
        _fileSystem.Verify(x => x.WriteFileText("repository path\\settings", "serialized content", Encoding.UTF8), Times.Once());
    }

    [Test]
    public void ReadSettingFile()
    {
        _fileSystem.Setup(x => x.IsFileExist("repository path\\settings")).Returns(true);
        _fileSystem.Setup(x => x.ReadFileText("repository path\\settings", Encoding.UTF8)).Returns("serialized content");
        var content = new SettingsContent
        {
            Author = "user name"
        };
        _serializer.Setup(x => x.Deserialize<SettingsContent>("serialized content")).Returns(content);

        _settings = new Settings(_repositoryPath, _serializer.Object, _fileSystem.Object, _windowsEnvironment.Object);

        Assert.That(_settings.Author, Is.EqualTo("user name"));
    }
}
