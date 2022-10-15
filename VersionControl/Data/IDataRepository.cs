using System.Collections.Generic;
using VersionControl.Core;

namespace VersionControl.Data;

internal interface IDataRepository
{
    FilePoco GetFileByUniqueId(ulong uniqueFileId);

    void SetUniqueFileIdFor(uint fileId, ulong uniqueFileId);

    CommitPoco? GetLastCommit();

    IReadOnlyCollection<CommitPoco> FindCommits(FindCommitsFilter filter);

    IEnumerable<ActualFileInfoPoco> GetActualFileInfo();

    IReadOnlyCollection<ActualFileInfoPoco> GetActualFileInfoByUniqueId(IReadOnlyCollection<ulong> uniqueFileIdCollection);

    uint GetCommitDetailsCount();

    IEnumerable<CommitDetailPoco> GetCommitDetails(long commitId);

    IEnumerable<FileContentPoco> GetFileContents(IEnumerable<uint> idCollection);

    IEnumerable<FilePathPoco> GetFilePathes(IEnumerable<uint> idCollection);

    FilePathPoco GetFilePathFor(uint commitDetailId, uint fileId);

    FileContentPoco GetFileContentFor(uint commitDetailId, uint fileId);

    byte[] GetActualFileContent(uint fileId);

    void SaveFiles(IReadOnlyCollection<FilePoco> files);

    void SaveCommit(CommitPoco commit);

    void SaveCommitDetails(IReadOnlyCollection<CommitDetailPoco> commitDetails);

    void SaveFileContents(IReadOnlyCollection<FileContentPoco> fileContents);

    void SaveFilePathes(IReadOnlyCollection<FilePathPoco> filePathes);

    void SaveActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> added);

    void UpdateActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> updated);

    void DeleteActualFileInfo(IEnumerable<ulong> deleted);
}
