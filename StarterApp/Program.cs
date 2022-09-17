using System;
using System.IO;
using System.Linq;
using System.Text;
using VersionControl.Core;

namespace StarterApp;

internal class Program
{
    private const string _projectPath = @"D:\Projects\VersionControl\StarterApp\bin\Debug\netcoreapp3.1\project";

    static void Main(string[] args)
    {
        bool clearProjectRepository = true;
        bool createManyFiles = true;
        bool makeCommit = true;
        bool findCommits = true;

        if (clearProjectRepository) ClearProjectRepository();
        if (createManyFiles) CreateManyFiles();

        var repo = RepositoryFactory.OpenRepository(_projectPath);

        var versionedFiles = repo.GetStatus();
        Console.WriteLine($"versioned files count: {versionedFiles.Count}");

        if (makeCommit && versionedFiles.Any())
        {
            var commitResult = repo.MakeCommit("first", versionedFiles);
            Console.WriteLine($"commit id: {commitResult.CommitId}");
        }

        if (findCommits)
        {
            Console.WriteLine("history:");
            var commits = repo.FindCommits(new() { PageIndex = 0, PageSize = 10 });
            foreach (var commit in commits)
            {
                Console.WriteLine($"commit {commit.Id} {commit.Comment} {commit.Author}");
            }
        }

        var firstCommit = repo.FindCommits(new() { PageIndex = 0, PageSize = 10 }).First();
        var commitDetail = repo.GetCommitDetail(firstCommit).First();
        var fileContent = repo.GetFileContent(commitDetail);
        var fileText = Encoding.UTF8.GetString(fileContent);
        Console.WriteLine($"file text: {fileText}");

        Console.ReadKey();
    }

    private static void CreateManyFiles()
    {
        var fileContent = new string('A', 100);
        for (int i = 0; i < 1000; i++)
        {
            File.WriteAllText(Path.Combine(_projectPath, i.ToString()), fileContent);
        }
    }

    private static void ClearProjectRepository()
    {
        foreach (var folder in Directory.GetDirectories(_projectPath)) Directory.Delete(folder, true);
        foreach (var file in Directory.GetFiles(_projectPath)) File.Delete(file);
    }
}
