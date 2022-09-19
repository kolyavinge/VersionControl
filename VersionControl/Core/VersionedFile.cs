namespace VersionControl.Core;

public class VersionedFile
{
    internal ulong UniqueId { get; set; }

    public string FullPath { get; internal set; }

    public ulong FileSize { get; internal set; }

    public FileActionKind ActionKind { get; internal set; }

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
