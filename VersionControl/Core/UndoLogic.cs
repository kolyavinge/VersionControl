using System.Collections.Generic;
using System.Data;
using System.Linq;
using DependencyInjection.Utils;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

internal interface IUndoLogic
{
    void UndoChanges(IReadOnlyCollection<VersionedFile> versionedFile);
}

internal class UndoLogic : IUndoLogic
{
    private readonly IDataRepository _repository;
    private readonly IFileSystem _fileSystem;
    private readonly IPathResolver _pathResolver;

    public UndoLogic(IDataRepository repository, IFileSystem fileSystem, IPathResolver pathResolver)
    {
        _repository = repository;
        _fileSystem = fileSystem;
        _pathResolver = pathResolver;
    }

    public void UndoChanges(IReadOnlyCollection<VersionedFile> versionedFile)
    {
        var allModfiedUniqueId = versionedFile
            .Where(x => x.ActionKind is FileActionKind.Modify or FileActionKind.Replace or FileActionKind.ModifyAndReplace or FileActionKind.Delete)
            .Select(x => x.UniqueId)
            .ToList();

        var actualFilesDictionary = allModfiedUniqueId.Any()
            ? _repository.GetActualFileInfoByUniqueId(allModfiedUniqueId).ToDictionary(k => k.UniqueFileId, v => v)
            : new Dictionary<ulong, ActualFileInfoPoco>();

        versionedFile.Each(file => UndoChanges(file, actualFilesDictionary));

        var deleted = versionedFile.Where(x => x.ActionKind is FileActionKind.Delete).ToList();
        if (deleted.Any())
        {
            _repository.DeleteActualFileInfo(deleted.Select(x => x.UniqueId));
            _repository.SaveActualFileInfo(deleted.Select(x => actualFilesDictionary[x.UniqueId]).ToList());
        }
    }

    private void UndoChanges(VersionedFile file, Dictionary<ulong, ActualFileInfoPoco> actualFilesDictionary)
    {
        if (file.ActionKind == FileActionKind.Add)
        {
            _fileSystem.DeleteFile(file.FullPath);
        }
        else if (file.ActionKind == FileActionKind.Modify)
        {
            var content = _repository.GetActualFileContent(actualFilesDictionary[file.UniqueId].FileId);
            _fileSystem.WriteFile(file.FullPath, content);
        }
        else if (file.ActionKind == FileActionKind.Replace)
        {
            _fileSystem.MoveFile(file.FullPath, _pathResolver.RelativePathToFull(actualFilesDictionary[file.UniqueId].RelativePath));
        }
        else if (file.ActionKind == FileActionKind.ModifyAndReplace)
        {
            var content = _repository.GetActualFileContent(actualFilesDictionary[file.UniqueId].FileId);
            _fileSystem.WriteFile(file.FullPath, content);
            _fileSystem.MoveFile(file.FullPath, _pathResolver.RelativePathToFull(actualFilesDictionary[file.UniqueId].RelativePath));
        }
        else if (file.ActionKind == FileActionKind.Delete)
        {
            var content = _repository.GetActualFileContent(actualFilesDictionary[file.UniqueId].FileId);
            _fileSystem.WriteFile(file.FullPath, content);
            var info = _fileSystem.GetFileInformation(file.FullPath);
            actualFilesDictionary[file.UniqueId].UniqueFileId = info.UniqueId;
        }
        else throw new ArgumentException();
    }
}
