using System.Collections.Generic;
using System.Linq;
using VersionControl.Data;

namespace VersionControl.Core;

internal interface ICommitDetails
{
    IEnumerable<CommitDetail> GetCommitDetails(long commitId);

    byte[] GetFileContent(uint commitDetailId, uint fileId);

    byte[]? GetFileContentBefore(uint commitDetailId, uint fileId);
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

        var filePathes = _dataRepository.GetFilePathes(
            commitDetails.Where(x => x.FileActionKind is (byte)FileActionKind.Add or (byte)FileActionKind.Replace or (byte)FileActionKind.ModifyAndReplace).Select(x => x.Id));

        var filePathesDictionary = filePathes.ToDictionary(k => fileIdDictionary[k.Id], v => v.RelativePath);

        foreach (var commitDetail in commitDetails)
        {
            var relativePath = "";
            if (commitDetail.FileActionKind is (byte)FileActionKind.Add or (byte)FileActionKind.Replace or (byte)FileActionKind.ModifyAndReplace)
            {
                relativePath = filePathesDictionary[commitDetail.FileId];
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
        var filePath = _dataRepository.GetFilePathFor(commitDetailId, fileId);
        return filePath.RelativePath;
    }

    public byte[] GetFileContent(uint commitDetailId, uint fileId)
    {
        var fileContent = _dataRepository.GetFileContent(commitDetailId, fileId);
        return fileContent.FileContent;
    }

    public byte[]? GetFileContentBefore(uint commitDetailId, uint fileId)
    {
        var fileContent = _dataRepository.GetFileContentBefore(commitDetailId, fileId);
        return fileContent?.FileContent;
    }
}
