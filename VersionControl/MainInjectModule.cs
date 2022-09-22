using DependencyInjection;
using VersionControl.Core;
using VersionControl.Data;
using VersionControl.Infrastructure;

namespace VersionControl;

internal class MainInjectModule : InjectModule
{
    public override void Init(IBindingProvider provider)
    {
        provider.Bind<IDateTimeProvider, DateTimeProvider>().ToSingleton();
        provider.Bind<IFileSystem, FileSystem>().ToSingleton();
        provider.Bind<IFileComparator, FileComparator>().ToSingleton();
        provider.Bind<ISerializer, Serializer>().ToSingleton();
        provider.Bind<IWindowsEnvironment, WindowsEnvironment>().ToSingleton();
        provider.Bind<IDataRepository, DataRepository>().ToSingleton();
        provider.Bind<ISettings, Settings>().ToSingleton();
        provider.Bind<IPathResolver, PathResolver>().ToSingleton();
        provider.Bind<IStatus, Status>().ToSingleton();
        provider.Bind<ICommitBuilder, CommitBuilder>().ToSingleton();
        provider.Bind<ICommitDetails, CommitDetails>().ToSingleton();
        provider.Bind<ICommitFinder, CommitFinder>().ToSingleton();
        provider.Bind<IVersionControlRepository, VersionControlRepository>().ToSingleton();
    }
}
