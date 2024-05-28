using SimsModules.Modules;
using SimsModules.Modules.CrashingMod;
using SimsModules.Modules.RemergeMod;
using Spectre.Console;


CrashingModule crashingModule = new();
RemergingModule remergingModule = new();
var modules = new Dictionary<string, IModule>
{
    { crashingModule.Description, crashingModule },
    { remergingModule.Description, remergingModule },
};

var selectedDescription = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which [green]module[/] would you like to run?")
        .PageSize(10)
        .MoreChoicesText("[grey](Move up and down to reveal more fruits)[/]")
        .AddChoices(modules.Keys));

// Call the Run method of the selected module:
modules[selectedDescription].Run();
AnsiConsole.MarkupLine("[yellow]Script has finished...[/]");