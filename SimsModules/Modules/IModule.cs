namespace SimsModules.Modules;

public interface IModule
{
    string Name { get; set;  }
    string Description { get; set; }
    void Run();
}