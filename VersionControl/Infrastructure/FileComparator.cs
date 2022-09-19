using System.IO;

namespace VersionControl.Infrastructure;

internal interface IFileComparator
{
    bool AreEqual(byte[] firstFileContent, string seconfFileFullPath);
}

internal class FileComparator : IFileComparator
{
    public bool AreEqual(byte[] firstFileContent, string secondFileFullPath)
    {
        var secondFileBuffer = new byte[1024];
        int count;
        int offset = 0;
        using var secondFileStream = File.OpenRead(secondFileFullPath);
        while ((count = secondFileStream.Read(secondFileBuffer, 0, secondFileBuffer.Length)) > 0)
        {
            for (int i = 0; i < count; i++)
            {
                if (firstFileContent[offset + i] != secondFileBuffer[i]) return false;
            }
            offset += count;
        }

        return true;
    }
}
