using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

public static class RepositoryFactory
{
    public static IRepository OpenRepository(string projectPath)
    {
        var pathHolder = new PathHolder(projectPath);
        var fileSystem = new FileSystem();
        fileSystem.CreateHiddenFolderIfNotExist(pathHolder.RepositoryPath);
        var serializer = new Serializer();
        var windowsEnvironment = new WindowsEnvironment();
        var dataRepository = new DataRepository(pathHolder);
        var settings = new Settings(pathHolder, serializer, fileSystem, windowsEnvironment);
        var pathResolver = new PathResolver(pathHolder);
        var status = new Status(pathHolder, dataRepository, pathResolver, fileSystem);
        var commitBuilder = new CommitBuilder(dataRepository, settings, pathResolver, fileSystem, DateTimeProvider.Instance);
        var commitDetails = new CommitDetails(dataRepository);
        var commitFinder = new CommitFinder(dataRepository);
        var repository = new Repository(status, commitBuilder, commitDetails, commitFinder);

        return repository;
    }
}
