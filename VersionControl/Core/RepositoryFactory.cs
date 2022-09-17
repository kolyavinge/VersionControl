using DependencyInjection;
using VersionControl.Infrastructure;

namespace VersionControl.Core;

public static class RepositoryFactory
{
    public static IRepository OpenRepository(string projectPath)
    {
        var container = DependencyContainerFactory.MakeLiteContainer();
        container.InitFromModules(new MainInjectModule());

        var pathHolder = new PathHolder(projectPath);
        container.Bind<IPathHolder>().ToMethod(_ => pathHolder);

        container.Resolve<IFileSystem>().CreateHiddenFolderIfNotExist(pathHolder.RepositoryPath);

        return container.Resolve<IRepository>();
    }
}
