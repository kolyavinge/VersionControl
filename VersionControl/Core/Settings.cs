using System.IO;
using System.Text;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal interface ISettings
{
    string Author { get; }
}

internal class Settings : ISettings
{
    private readonly string _settingFileFullPath;
    private readonly ISerializer _serializer;
    private readonly IFileSystem _fileSystem;
    private readonly IWindowsEnvironment _windowsEnvironment;

    public string Author { get; private set; } = "";

    public Settings(string repositoryPath, ISerializer serializer, IFileSystem fileSystem, IWindowsEnvironment windowsEnvironment)
    {
        _settingFileFullPath = Path.Combine(repositoryPath, Constants.SettingFileName);
        _serializer = serializer;
        _fileSystem = fileSystem;
        _windowsEnvironment = windowsEnvironment;
        LoadOrCreateFile();
    }

    private void LoadOrCreateFile()
    {
        SettingsContent content;
        if (_fileSystem.IsFileExist(_settingFileFullPath))
        {
            var json = _fileSystem.ReadFileText(_settingFileFullPath, Encoding.UTF8);
            content = _serializer.Deserialize<SettingsContent>(json);
        }
        else
        {
            content = new SettingsContent
            {
                Author = _windowsEnvironment.UserName
            };
            var json = _serializer.Serialize(content);
            _fileSystem.WriteFileText(_settingFileFullPath, json, Encoding.UTF8);
        }
        Author = content.Author;
    }
}

class SettingsContent
{
    public string Author { get; set; } = "";

    public override bool Equals(object? obj)
    {
        return obj is SettingsContent content &&
               Author == content.Author;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Author);
    }
}
