using System.IO;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

public static class RepositoryFactory
{
    public static IRepository OpenRepository(string projectPath)
    {
        var repositoryPath = Path.Combine(projectPath, Constants.RepositoryFolderName);
        var fileSystem = new FileSystem();
        fileSystem.CreateHiddenFolderIfNotExist(repositoryPath);
        var serializer = new Serializer();
        var windowsEnvironment = new WindowsEnvironment();
        var dataRepository = new DataRepository(repositoryPath);
        var settings = new Settings(repositoryPath, serializer, fileSystem, windowsEnvironment);
        var pathResolver = new PathResolver(projectPath);
        var status = new Status(projectPath, repositoryPath, dataRepository, pathResolver, fileSystem);
        var commitBuilder = new CommitBuilder(dataRepository, settings, pathResolver, fileSystem, DateTimeProvider.Instance);
        var commitDetails = new CommitDetails(dataRepository);
        var commitFinder = new CommitFinder(dataRepository);
        var repository = new Repository(status, commitBuilder, commitDetails, commitFinder);

        return repository;
    }
}
