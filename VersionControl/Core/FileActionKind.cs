namespace VersionControl.Core;

public enum FileActionKind : byte
{
    Add = 1,
    Modify = 2,
    Replace = 3,
    ModifyAndReplace = 4,
    Delete = 5
}
