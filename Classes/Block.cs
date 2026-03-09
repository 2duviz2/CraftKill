namespace CraftKill.Classes;

using UnityEngine;

public enum BlockType
{
    block,
    tnt,
    bedrock,
    lava,
}

public class Block : ScriptableObject
{
    public string ID = "";
    public int chance = 0;
    [Space]
    public Texture texture;
    public Material mat;
    public BlockType type = BlockType.block;

    public Block(string iD, Texture texture, BlockType type, int chance, Material mat = null)
    {
        ID = iD;
        this.texture = texture;
        this.type = type;
        this.chance = chance;
        if (mat) this.mat = mat;
    }
}
