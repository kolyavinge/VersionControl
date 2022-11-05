using System.Collections.Generic;
using VersionControl.Core;

namespace VersionControl.Data;

internal interface IDataRepository
{
    CommitPoco? GetLastCommit();

    IReadOnlyCollection<CommitPoco> FindCommits(FindCommitsFilter filter);

    IEnumerable<ActualFileInfoPoco> GetActualFileInfo();

    ActualFileInfoPoco? GetActualFileInfoByUniqueId(ulong uniqueFileId);

    IReadOnlyCollection<ActualFileInfoPoco> GetActualFileInfoByUniqueId(IReadOnlyCollection<ulong> uniqueFileIdCollection);

    uint GetCommitDetailsCount();

    IEnumerable<CommitDetailPoco> GetCommitDetails(long commitId);

    IEnumerable<FileContentPoco> GetFileContents(IEnumerable<uint> idCollection);

    IEnumerable<FilePathPoco> GetFilePathes(IEnumerable<uint> idCollection);

    FilePathPoco GetFilePathFor(uint commitDetailId, uint fileId);

    FileContentPoco GetFileContent(uint commitDetailId, uint fileId);

    FileContentPoco? GetFileContentBefore(uint commitDetailId, uint fileId);

    FileContentPoco? GetActualFileContent(uint fileId);

    void SaveCommit(CommitPoco commit);

    void SaveCommitDetails(IReadOnlyCollection<CommitDetailPoco> commitDetails);

    void SaveFileContents(IReadOnlyCollection<FileContentPoco> fileContents);

    void SaveFilePathes(IReadOnlyCollection<FilePathPoco> filePathes);

    void SaveActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> added);

    void UpdateActualFileInfo(IReadOnlyCollection<ActualFileInfoPoco> updated);

    void DeleteActualFileInfo(IEnumerable<ulong> deleted);
}
