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
            var bb = UnityEngine.Object.Instantiate(b);
            bb.type = bb.type == BlockType.bedrock ? BlockType.block : bb.type;
            CubePlacer.instance.ClearBlock(b, 1000000);
            if (bb.type == BlockType.lava) continue;
            CubePlacer.instance.ClearBlock(bb, 1000000);
            CubePlacer.instance.GiveBlock(bb, 10000);
        }
    }
}

public class SoftBlocks : ICommand, IConsoleLogger
{
    public Logger Log { get; } = new Logger("SoftBlocks");

    public string Name
    {
        get
        {
            return "SoftBlocks";
        }
    }

    public string Description
    {
        get
        {
            return "Makes every block breakable";
        }
    }

    public string Command
    {
        get
        {
            return "softblocks";
        }
    }

    public void Execute(Console con, string[] args)
    {
        CameraController.Instance.gameObject.GetOrAddComponent<CubePlacer>();
        foreach (var b in CubePlacer.instance.blocks)
        {
            b.type = BlockType.block;
        }
    }
}

public class ChangeDimension : ICommand, IConsoleLogger
{
    public Logger Log { get; } = new Logger("ChangeDimension");

    public string Name
    {
        get
        {
            return "ChangeDimension";
        }
    }

    public string Description
    {
        get
        {
            return "Changes your current dimension (usage: changedimension <DIMENSION>)";
        }
    }

    public string Command
    {
        get
        {
            return "changedimension";
        }
    }

    public void Execute(Console con, string[] args)
    {
        if (!Generation.instance) return;

        if (args.Length != 1)
        {
            Log.Error("You need to specify a dimension");
            return;
        }

        var di = int.Parse(args[0]);
        Generation.instance.ChangeDimension(di);
    }
}

public class FillCommand : ICommand, IConsoleLogger
{
    public Logger Log { get; } = new Logger("Fill");

    public string Name
    {
        get
        {
            return "Fill";
        }
    }

    public string Description
    {
        get
        {
            return "Fills a gap with blocks";
        }
    }

    public string Command
    {
        get
        {
            return "fill";
        }
    }

    public void Execute(Console con, string[] args)
    {
        CameraController.Instance.gameObject.GetOrAddComponent<CubePlacer>();

        var x = 10;
        var y = 10;
        var z = 10;
        var b = 0;
        if (args.Length == 2)
        {
            b = int.Parse(args[0]);
            x = y = z = int.Parse(args[1]);
        }
        else if (args.Length == 3)
        {
            b = int.Parse(args[0]);
            x = z = int.Parse(args[1]);
            y = int.Parse(args[2]);
        }
        else if (args.Length == 4)
        {
            b = int.Parse(args[0]);
            x = int.Parse(args[1]);
            y = int.Parse(args[2]);
            z = int.Parse(args[3]);
        }

        var startpos = NewMovement.Instance.Position;

        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                for (int k = 0; k < z; k++)
                {
                    CubePlacer.instance.PlaceCube(CubePlacer.V3ToGrid(startpos + new UnityEngine.Vector3(i, j, k) * CubePlacer.blockSize), CubePlacer.instance.blocks[b], optimized : true);
                }
            }
        }
    }
}