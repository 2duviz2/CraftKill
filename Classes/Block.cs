namespace CraftKill.Classes;

using UnityEngine;

public enum BlockType
{
    block,
    tnt,
}

public class Block : ScriptableObject
{
    public string ID = "";
    [Space]
    public Texture texture;
    public BlockType type = BlockType.block;

    public Block(string iD, Texture texture, BlockType type)
    {
        ID = iD;
        this.texture = texture;
        this.type = type;
    }
}
