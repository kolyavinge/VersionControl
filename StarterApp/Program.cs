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
        bool showFileContent = true;
        bool findCommits = false;
        bool undo = false;

        if (clearProjectRepository) ClearProjectRepository();
        if (createManyFiles) CreateManyFiles();

        var repo = VersionControlRepositoryFactory.OpenRepository(_projectPath);

        var versionedFiles = repo.GetStatus().Files;
        Console.WriteLine($"versioned files count: {versionedFiles.Count}");

        if (makeCommit && versionedFiles.Any())
        {
            var commitResult = repo.MakeCommit("first", versionedFiles);
            Console.WriteLine($"commit id: {commitResult.CommitId}");
        }

        if (showFileContent)
        {
            var fileToModify = Directory.GetFiles(_projectPath).First();
            File.WriteAllText(fileToModify, "modified_file");
            versionedFiles = repo.GetStatus().Files;
            repo.MakeCommit("second", versionedFiles);
            var secondCommit = repo.FindCommits(new() { PageIndex = 0, PageSize = 10 }).First();
            var secondCommitDetail = repo.GetCommitDetails(secondCommit).First();
            var lastFileContent = repo.GetFileContent(secondCommitDetail);
            var lastFileText = Encoding.UTF8.GetString(lastFileContent);
            Console.WriteLine($"last file text: {lastFileText}");
            var beforeFileContent = repo.GetFileContentBefore(secondCommitDetail);
            var beforeFileText = beforeFileContent != null ? Encoding.UTF8.GetString(beforeFileContent) : null;
            Console.WriteLine($"before file text: {beforeFileText}");
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
        var commitDetail = repo.GetCommitDetails(firstCommit).First();
        var fileContent = repo.GetFileContent(commitDetail);
        var fileText = Encoding.UTF8.GetString(fileContent);
        Console.WriteLine($"file text: {fileText}");

        if (undo)
        {
            var files = Directory.GetFiles(_projectPath).Take(4).ToList();

            File.WriteAllText(Path.Combine(_projectPath, "new_file_for_undo"), "new_file_for_undo");

            File.WriteAllText(files[0], "modified_file_for_undo");

            File.Move(files[1], files[1] + "_new_path");

            File.WriteAllText(files[2], "modified_and_replaced_file_for_undo");
            File.Move(files[2], files[2] + "_new_path");

            File.Delete(files[3]);

            var statusForUndo = repo.GetStatus();
            Console.WriteLine($"files before undo: {statusForUndo.Files.Count}");
            repo.UndoChanges(statusForUndo.Files);
            statusForUndo = repo.GetStatus();
            Console.WriteLine($"files after undo: {statusForUndo.Files.Count}");
        }

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
