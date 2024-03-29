﻿using System.IO;
using System.Linq;

namespace VersionControl.Core;

internal interface IPathResolver
{
    string FullPathToRelative(string fullPath);
    string RelativePathToFull(string relativePath);
}

internal class PathResolver : IPathResolver
{
    private readonly string _projectPath;
    private readonly int _projectPathLength;

    public PathResolver(IPathHolder pathHolder)
    {
        _projectPath = pathHolder.ProjectPath;
        _projectPathLength = _projectPath.Length;
        if (_projectPath.Last() != '\\') _projectPathLength++;
    }

    public string FullPathToRelative(string fullPath)
    {
        return fullPath.Substring(_projectPathLength);
    }

    public string RelativePathToFull(string relativePath)
    {
        return Path.Combine(_projectPath, relativePath);
    }
}
