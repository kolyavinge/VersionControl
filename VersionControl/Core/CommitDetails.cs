using System.Collections.Generic;
using System.Linq;
using VersionControl.Core;
using VersionControl.Data;

internal interface ICommitDetails
{
    IEnumerable<CommitDetail> GetCommitDetails(long commitId);

    byte[] GetFileContent(uint commitDetailId, uint fileId);
}

internal class CommitDetails : ICommitDetails
{
    private readonly IDataRepository _dataRepository;

    public CommitDetails(IDataRepository dataRepository)
    {
        _dataRepository = dataRepository;
    }

    public IEnumerable<CommitDetail> GetCommitDetails(long commitId)
    {
        var commitDetails = _dataRepository.GetCommitDetails(commitId);
        var fileIdDictionary = commitDetails.ToDictionary(k => k.Id, v => v.FileId);

        var addActions = _dataRepository.GetAddActions(
            commitDetails.Where(x => x.FileActionKind is (byte)FileActionKind.Add).Select(x => x.Id));

        var replaceActions = _dataRepository.GetReplaceActions(
            commitDetails.Where(x => x.FileActionKind is (byte)FileActionKind.Replace or (byte)FileActionKind.ModifyAndReplace).Select(x => x.Id));

        var addActionsDictionary = addActions.ToDictionary(k => fileIdDictionary[k.Id], v => v.RelativePath);
        var replaceActionsDictionary = replaceActions.ToDictionary(k => fileIdDictionary[k.Id], v => v.RelativePath);

        foreach (var commitDetail in commitDetails)
        {
            var relativePath = "";
            if (commitDetail.FileActionKind is (byte)FileActionKind.Add)
            {
                relativePath = addActionsDictionary[commitDetail.FileId];
            }
            else if (commitDetail.FileActionKind is (byte)FileActionKind.Replace or (byte)FileActionKind.ModifyAndReplace)
            {
                relativePath = replaceActionsDictionary[commitDetail.FileId];
            }
            else if (commitDetail.FileActionKind is (byte)FileActionKind.Modify or (byte)FileActionKind.Delete)
            {
                relativePath = GetRelativeFilePath(commitDetail.Id, commitDetail.FileId);
            }

            yield return new CommitDetail(commitDetail.Id, (FileActionKind)commitDetail.FileActionKind, commitDetail.FileId, relativePath);
        }
    }

    private string GetRelativeFilePath(uint commitDetailId, uint fileId)
    {
        var commitDetailForReplace = _dataRepository.GetLastCommitDetailForReplace(commitDetailId, fileId);
        if (commitDetailForReplace != null)
        {
            var replaceAction = _dataRepository.GetReplaceActions(new[] { commitDetailForReplace.Id }).First();
            return replaceAction.RelativePath;
        }
        else
        {
            var addAction = _dataRepository.GetAddActions(new[] { fileId }).First();
            return addAction.RelativePath;
        }
    }

    public byte[] GetFileContent(uint commitDetailId, uint fileId)
    {
        var commitDetailForModify = _dataRepository.GetLastCommitDetailForModify(commitDetailId, fileId);
        if (commitDetailForModify != null)
        {
            var modifyAction = _dataRepository.GetModifyActions(new[] { commitDetailForModify.Id }).First();
            return modifyAction.FileContent;
        }
        else
        {
            var addAction = _dataRepository.GetAddActions(new[] { fileId }).First();
            return addAction.FileContent;
        }
    }
}
