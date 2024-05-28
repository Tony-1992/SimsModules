# SimsModules

### The problem
When Sims4 updates occur, custom mods break the game which prevents Sims4 from opening.
This means a conveluted approach is used to filter the mods 1 by 1 until the problem mod is eventually found and removed.

Currently, this is very time consuming.

### The solution
A C# console application which will iterate through the mods folder, moving clean mods to a temp location until the game finds the problem folder.

Once found, it will then iterate through individual files until the game runs, highlighting the exact mod file causing the issue.

### How to use
Navigate to the location where you have the ModFinder.exe saved, then you can run run the below from the command line with your chosen parameters;

Run the console script from the command line(terminal) and pass in 2 parameters;
- Full path to .exe file of the game
- Your preferred wait time (in minutes), once the game starts to load, it will wait for this amount time to declare it is running correctly.

Example:
```
ModFinder.exe "full\path\to\game\sims.exe" 10 
```