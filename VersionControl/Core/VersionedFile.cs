namespace VersionControl.Core;

public class VersionedFile
{
    internal ulong UniqueId { get; }

    public string FullPath { get; }

    public string RelativePath { get; }

    internal ulong FileSize { get; }

    public FileActionKind ActionKind { get; }

    internal VersionedFile(ulong uniqueId, string fullPath, string relativePath, ulong fileSize, FileActionKind actionKind)
    {
        UniqueId = uniqueId;
        FullPath = fullPath;
        RelativePath = relativePath;
        FileSize = fileSize;
        ActionKind = actionKind;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionedFile file &&
               UniqueId == file.UniqueId &&
               FullPath == file.FullPath &&
               RelativePath == file.RelativePath &&
               FileSize == file.FileSize &&
               ActionKind == file.ActionKind;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId, FullPath, RelativePath, ActionKind);
    }
}
