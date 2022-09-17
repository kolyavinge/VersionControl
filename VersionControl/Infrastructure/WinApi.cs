using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace VersionControl.Infrastructure;

internal static class WinApi
{
    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetFileInformationByHandle(SafeFileHandle hFile, out BY_HANDLE_FILE_INFORMATION lpFileInformation);

    public struct FILETIME
    {
        public uint dwLowDateTime;
        public uint dwHighDateTime;
    }

    public struct BY_HANDLE_FILE_INFORMATION
    {
        public uint FileAttributes;
        public FILETIME CreationTime;
        public FILETIME LastAccessTime;
        public FILETIME LastWriteTime;
        public uint VolumeSerialNumber;
        public uint FileSizeHigh;
        public uint FileSizeLow;
        public uint NumberOfLinks;
        public uint FileIndexHigh;
        public uint FileIndexLow;
    }
}
