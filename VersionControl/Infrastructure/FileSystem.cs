using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VersionControl.Infrastructure;

internal struct FileInformation
{
    public readonly ulong UniqueId;
    public readonly string Path;
    public readonly ulong Size;
    public readonly long CreatedUtc;
    public readonly long ModifiedUtc;

    public FileInformation(ulong uniqueId, string path, ulong size, long createdUtc, long modifiedUtc)
    {
        UniqueId = uniqueId;
        Path = path;
        Size = size;
        CreatedUtc = createdUtc;
        ModifiedUtc = modifiedUtc;
    }
}

internal interface IFileSystem
{
    bool IsFileExist(string filePath);
    byte[] ReadFileBytes(string filePath);
    string ReadFileText(string filePath, Encoding encoding);
    void WriteFileText(string filePath, string content, Encoding encoding);
    FileInformation GetFileInformation(string filePath);
    bool IsFolderExist(string path);
    void CreateHiddenFolderIfNotExist(string path);
    IEnumerable<string> GetFilesRecursively(string path);
}

internal class FileSystem : IFileSystem
{
    public bool IsFileExist(string filePath)
    {
        return File.Exists(filePath);
    }

    public byte[] ReadFileBytes(string filePath)
    {
        return File.ReadAllBytes(filePath);
    }

    public string ReadFileText(string filePath, Encoding encoding)
    {
        return File.ReadAllText(filePath, encoding);
    }

    public void WriteFileText(string filePath, string content, Encoding encoding)
    {
        File.WriteAllText(filePath, content, encoding);
    }

    public FileInformation GetFileInformation(string filePath)
    {
        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        WinApi.GetFileInformationByHandle(fs.SafeFileHandle, out WinApi.BY_HANDLE_FILE_INFORMATION objectFileInfo);
        var uniqueFileId = ((ulong)objectFileInfo.FileIndexHigh << 32) + objectFileInfo.FileIndexLow;
        var size = ((ulong)objectFileInfo.FileSizeHigh << 32) + objectFileInfo.FileSizeLow;
        var createdUtc = ((long)objectFileInfo.CreationTime.dwHighDateTime << 32) + objectFileInfo.CreationTime.dwLowDateTime;
        var modifiedUtc = ((long)objectFileInfo.LastWriteTime.dwHighDateTime << 32) + objectFileInfo.LastWriteTime.dwLowDateTime;

        return new(uniqueFileId, filePath, size, createdUtc, modifiedUtc);
    }

    public bool IsFolderExist(string path)
    {
        return Directory.Exists(path);
    }

    public void CreateHiddenFolderIfNotExist(string path)
    {
        if (Directory.Exists(path)) return;
        var info = Directory.CreateDirectory(path);
        info.Attributes |= FileAttributes.Hidden;
    }

    public IEnumerable<string> GetFilesRecursively(string path)
    {
        var result = new List<string>();

        void search(string parentPath)
        {
            foreach (var dir in Directory.GetDirectories(parentPath)) search(dir);
            result.AddRange(Directory.GetFiles(parentPath));
        }

        search(path);

        return result;
    }
}
