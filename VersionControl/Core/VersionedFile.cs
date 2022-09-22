namespace VersionControl.Core;

public class VersionedFile
{
    internal ulong UniqueId { get; }

    public string FullPath { get; }

    internal ulong FileSize { get; }

    public FileActionKind ActionKind { get; }

    internal VersionedFile(ulong uniqueId, string fullPath, ulong fileSize, FileActionKind actionKind)
    {
        UniqueId = uniqueId;
        FullPath = fullPath;
        FileSize = fileSize;
        ActionKind = actionKind;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionedFile file &&
               UniqueId == file.UniqueId &&
               FullPath == file.FullPath &&
               FileSize == file.FileSize &&
               ActionKind == file.ActionKind;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId, FullPath, ActionKind);
    }
}
