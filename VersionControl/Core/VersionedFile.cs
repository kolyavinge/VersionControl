namespace VersionControl.Core;

public class VersionedFile
{
    internal ulong UniqueId { get; set; }

    public string FullPath { get; internal set; }

    public FileActionKind ActionKind { get; internal set; }

    internal VersionedFile(ulong uniqueId, string fullPath, FileActionKind actionKind)
    {
        UniqueId = uniqueId;
        FullPath = fullPath;
        ActionKind = actionKind;
    }

    public override bool Equals(object? obj)
    {
        return obj is VersionedFile file &&
               UniqueId == file.UniqueId &&
               FullPath == file.FullPath &&
               ActionKind == file.ActionKind;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UniqueId, FullPath, ActionKind);
    }
}
