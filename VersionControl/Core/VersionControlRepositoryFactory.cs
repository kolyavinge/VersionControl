using DependencyInjection;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

public static class VersionControlRepositoryFactory
{
    public static bool IsRepositoryExist(string projectPath)
    {
        var pathHolder = new PathHolder(projectPath);
        var fileSystem = new FileSystem();

        return fileSystem.IsFolderExist(pathHolder.RepositoryPath) && fileSystem.IsFileExist(pathHolder.DBFilePath);
    }

    public static IVersionControlRepository OpenRepository(string projectPath)
    {
        var container = DependencyContainerFactory.MakeLiteContainer();
        container.InitFromModules(new MainInjectModule());

        var pathHolder = new PathHolder(projectPath);
        container.Bind<IPathHolder>().ToMethod(_ => pathHolder);

        container.Resolve<IFileSystem>().CreateHiddenFolderIfNotExist(pathHolder.RepositoryPath);

        return container.Resolve<IVersionControlRepository>();
    }
}
