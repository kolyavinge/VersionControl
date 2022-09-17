using System.Collections.Generic;
using VersionControl.Core;

namespace VersionControl.Data;

internal interface IDataRepository
{
    VersionedFilePoco GetFileByUniqueId(ulong uniqueFileId);

    void ClearUniqueFileIdFor(uint fileId);

    CommitPoco? GetLastCommit();

    IReadOnlyCollection<CommitPoco> FindCommits(FindCommitsFilter filter);

    IEnumerable<LastPathFilePoco> GetLastPathFiles();

    uint GetCommitDetailsCount();

    IEnumerable<CommitDetailPoco> GetCommitDetails(long commitId);

    IEnumerable<AddFileActionPoco> GetAddActions(IEnumerable<uint> idCollection);

    IEnumerable<ModifyFileActionPoco> GetModifyActions(IEnumerable<uint> idCollection);

    IEnumerable<ReplaceFileActionPoco> GetReplaceActions(IEnumerable<uint> idCollection);

    CommitDetailPoco? GetLastCommitDetailForReplace(uint commitDetailId, uint fileId);

    CommitDetailPoco? GetLastCommitDetailForModify(uint commitDetailId, uint fileId);

    void SaveVersionedFiles(IReadOnlyCollection<VersionedFilePoco> versionedFiles);

    void SaveCommit(CommitPoco commit);

    void SaveCommitDetails(IReadOnlyCollection<CommitDetailPoco> commitDetails);

    void SaveAddFileActions(IReadOnlyCollection<AddFileActionPoco> addFileActions);

    void SaveModifyFileActions(IReadOnlyCollection<ModifyFileActionPoco> modifyFileActions);

    void SaveReplaceFileActions(IReadOnlyCollection<ReplaceFileActionPoco> replaceFileActions);

    void SaveLastPathFiles(IReadOnlyCollection<LastPathFilePoco> added);

    void UpdateLastPathFiles(IReadOnlyCollection<LastPathFilePoco> updated);

    void DeleteLastPathFiles(IEnumerable<ulong> deleted);
}
