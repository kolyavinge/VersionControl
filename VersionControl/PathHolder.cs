using System.IO;

namespace VersionControl;

internal interface IPathHolder
{
    string ProjectPath { get; }

    string RepositoryPath { get; }
}

internal class PathHolder : IPathHolder
{
    public string ProjectPath { get; }

    public string RepositoryPath { get; }

    public PathHolder(string projectPath)
    {
        ProjectPath = projectPath;
        RepositoryPath = Path.Combine(projectPath, Constants.RepositoryFolderName);
    }
}
