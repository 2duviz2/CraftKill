namespace CraftKill.Classes;

using GameConsole;
using Logger = plog.Logger;

public class BlockCommand : ICommand, IConsoleLogger
{
    public Logger Log { get; } = new Logger("Minefart");

    public string Name
    {
        get
        {
            return "Minefart";
        }
    }

    public string Description
    {
        get
        {
            return "Lets you place blocks on whichever level you are";
        }
    }

    public string Command
    {
        get
        {
            return "minefart";
        }
    }

    public void Execute(Console con, string[] args)
    {
        CameraController.Instance.gameObject.GetOrAddComponent<CubePlacer>();
        foreach (var b in CubePlacer.instance.blocks)
        {
            CubePlacer.instance.ClearBlock(b, 1000000);
            CubePlacer.instance.GiveBlock(b, 10000);
        }
    }
}