using Spectre.Console;

namespace SimsModules.Modules.RemergeMod;

public class RemergingModule : IModule
{
    public string Name { get; set; } = "Remerging Module";
    public string Description { get; set; } = "Un-merge and re-merging of mods";
    private string _modFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Electronic Arts\The Sims 4\Mods";
    private string _tempFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\TempMods";

    public void Run()
    {
        // Choose to merge or unmerge
        // if unmerged
        // unmerged will go to a "desktop/unmerged" folder
        // Show a list of folders within Sims4/mods
        // let user select which folder they want to view
        // pull all availble .package files from folder
        // let user select multiple files to unmerge
        // selected files will be unmerged and copied to the desktop/unmerged folder
        // let user choose to delete selected files

    }
}