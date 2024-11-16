using System.Diagnostics;
using Spectre.Console;

namespace SimsModules.Modules.CrashingMod;

public class CrashingModule : IModule
{
    public string Name { get; set; } = "Crashing Module";
    public string Description { get; set; } = "Find mod crashing game";
    private string _modFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Electronic Arts\The Sims 4\Mods";
    private string _tempFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TempMods";
    private string _gameProcessName { get; set; }
    private string _pathToGameExe { get; set; }

    public void Run()
    {
        if (!AnsiConsole.Confirm("Confirm you have ran the game and it is definitely crashing."))
            return;

        // take start time
        DateTime startTime = DateTime.Now;

        // take path to game
        _pathToGameExe = AnsiConsole.Ask<string>("Paste the path to the [lime bold]Sims 4[/] executable:");
        _gameProcessName = Path.GetFileNameWithoutExtension(_pathToGameExe);

        AnsiConsole.Status()
            .Start("Loading...", ctx =>
            {
                try
                {
                    // Simulate some work
                    ctx.Status("[yellow]Current task:[/] Searching for mod folder that is causing the crash...");
                    string failedFolder = FindFailedModFolder();

                    ctx.Status("[yellow]Current task:[/] Searching for individual mod causing the crash");
                    string failedModName = FindFailedFile(failedFolder);
                    AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Mod causing the crash is - [lime bold]{failedModName.EscapeMarkup()}[/]");
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine($"Something went wrong - {ex.Message}");
                }
                finally
                {
                    ctx.Status("[yellow]Current task:[/] Cleaning up folders...");

                    CloseGame();
                    Thread.Sleep(5000);
                    CleanUp();

                    AnsiConsole.WriteLine("============================");
                    AnsiConsole.MarkupLine($"Start time: [yellow]{startTime}[/] ==> End time: [yellow]{DateTime.Now}[/]");
                    AnsiConsole.WriteLine("============================");
                }
            });
    }


    private string FindFailedModFolder()
    {
        string failureFolderPath = string.Empty;

        foreach (DirectoryInfo folder in GetAllFoldersFromModLocation())
        {
            ShowRemainingFolderCountAndSize();
            failureFolderPath = MoveFolderToTempLocation(folder);

            // load game and wait
            Process.Start(_pathToGameExe);
            Thread.Sleep(5000);

            DateTime gameExpirationTime = DateTime.Now.AddMinutes(1);
            while (GameProcessIsRunning())
            {
                if (DateTime.Now > gameExpirationTime)
                {
                    CloseGame();

                    AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Mod causing the crash can be found at location - [lime bold]{failureFolderPath}[/]");
                    return failureFolderPath;
                }
            }
        }

        CloseGame();

        AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Mod causing the crash can be found at location - [lime bold]{failureFolderPath}[/]");
        return failureFolderPath;
    }

    private string FindFailedFile(string folderName)
    {
        Thread.Sleep(5000);
        string failedModName = string.Empty;

        foreach (DirectoryInfo folder in GetAllFoldersFromModLocation())
        {
            MoveFolderToTempLocation(folder);
        }

        AnsiConsole.MarkupLine("[cyan1]LOG:[/] Rebuilding Mod folder to find individual file");
        string updatedPath = CopyFailedFolderToModFolder(folderName);
        string[] files = Directory.GetFiles(updatedPath);

        foreach (string fileName in files)
        {
            DateTime gameExpirationTime = DateTime.Now.AddMinutes(1);

            AnsiConsole.MarkupLine($"[cyan1]LOG:[/] File removed ==> [blue]{fileName.EscapeMarkup()}[/]");
            DeleteFile(fileName);

            // load game and wait
            Process.Start(_pathToGameExe);
            Thread.Sleep(5000);

            while (GameProcessIsRunning())
            {
                // If current time is greater than expiry then more than likely it's running ok so return file name
                if (DateTime.Now > gameExpirationTime)
                {
                    return fileName;
                }
            }
        }

        return failedModName;
    }

    private void DeleteFile(string fileName)
    {
        File.Delete(fileName);
    }

    private string MoveFolderToTempLocation(DirectoryInfo folderToMove)
    {
        if (Directory.Exists(_tempFolder) is false)
            Directory.CreateDirectory(_tempFolder);

        AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Moving folder: [blue]{folderToMove.Name}[/] to temp location");

        string destinationFolder = Path.Combine(_tempFolder, folderToMove.Name);
        Directory.Move(folderToMove.FullName, destinationFolder);

        return destinationFolder;
    }

    private string CopyFailedFolderToModFolder(string folderToCopy)
    {
        DirectoryInfo folder = new DirectoryInfo(folderToCopy);
        string destinationFolder = Path.Combine(_modFolder, folder.Name);
        if (Directory.Exists(destinationFolder) is false)
            Directory.CreateDirectory(destinationFolder);

        AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Copying files back to to sims mod location... please wait...");
        foreach (FileInfo fileName in folder.GetFiles())
        {
            fileName.CopyTo(Path.Combine(destinationFolder, fileName.Name));
        }

        return destinationFolder;
    }

    private void ShowRemainingFolderCountAndSize()
    {
        List<DirectoryInfo> modFolders = GetAllFoldersFromModLocation();

        long totalSizeInBytes = 0;
        foreach (var folder in modFolders)
        {
            totalSizeInBytes += folder.EnumerateFiles().Sum(f => f.Length);
        }

        decimal totalSizeInGB = Math.Round((decimal)totalSizeInBytes / (1024 * 1024 * 1024), 2);
        AnsiConsole.MarkupLine($"[cyan1]LOG:[/] Total mod folders remaining - [yellow bold]{modFolders.Count} ({totalSizeInGB} GB)[/]");
    }

    private bool GameProcessIsRunning()
    {
        foreach (Process clsProcess in Process.GetProcesses())
        {
            if (clsProcess.ProcessName.Contains(_gameProcessName))
                return true;
        }

        return false;
    }

    private void CleanUp()
    {
        DirectoryInfo tempFolder = new DirectoryInfo(_tempFolder);
        DirectoryInfo modFolder = new DirectoryInfo(_modFolder);

        foreach (var folder in modFolder.EnumerateDirectories())
        {
            var pathToMods = Path.Combine(_modFolder, folder.Name);
            Directory.Delete(pathToMods, true);
        }

        AnsiConsole.MarkupLine("[cyan1]LOG:[/] Restoring mods back to original location");
        foreach (var folder in tempFolder.EnumerateDirectories())
        {
            var pathToMods = Path.Combine(_modFolder, folder.Name);
            Directory.Move(folder.FullName, pathToMods);
        }

        AnsiConsole.MarkupLine("[cyan1]LOG:[/] Deleting all temp folders & files");
        Directory.Delete(_tempFolder, true);
    }

    private void CloseGame()
    {
        try
        {
            Process[] procs = Process.GetProcessesByName(_gameProcessName);
            foreach (Process proc in procs)
                proc.Kill(true);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"Something went wrong closing the game - {ex.Message}");
        }
    }

    private List<DirectoryInfo> GetAllFoldersFromModLocation()
    {
        List<DirectoryInfo> modFolders = new DirectoryInfo(_modFolder)
            .EnumerateDirectories()
            .ToList();

        return modFolders;
    }
}