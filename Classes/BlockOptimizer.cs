namespace CraftKill.Classes;

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlockOptimizer : MonoBehaviour
{
    public class blockToSpawn
    {
        public Vector3 pos;
        public Block block;
        public Transform forceParent;
        public bool optimized;
        public GameObject spawned;

        public blockToSpawn(Vector3 pos, Block block, Transform forceParent, bool optimized)
        {
            this.pos = pos;
            this.block = block;
            this.forceParent = forceParent;
            this.optimized = optimized;
        }
    }

    public List<blockToSpawn> blocks = [];

    public bool isDirty = false;

    public void Update()
    {
        if (isDirty) FindCulledBlocks();
    }

    public void AddBlock(Vector3 pos, Block block, Transform forceParent, bool optimized)
    {
        blocks.Add(new blockToSpawn(pos, block, forceParent, optimized));
        isDirty = true;
    }

    public void DestroyBlock(Vector3 pos)
    {
        foreach (var block in blocks.ToList())
            if (Vector3.Distance(block.pos, pos) < 1) blocks.Remove(block);
        isDirty = true;
    }

    public void FindCulledBlocks()
    {
        isDirty = false;

        float size = CubePlacer.instance.blockSize;

        Vector3 px = Vector3.right * size;
        Vector3 nx = Vector3.left * size;
        Vector3 py = Vector3.up * size;
        Vector3 ny = Vector3.down * size;
        Vector3 pz = Vector3.forward * size;
        Vector3 nz = Vector3.back * size;

        HashSet<Vector3> blockPositions = new HashSet<Vector3>(blocks.Count);

        foreach (var t in blocks) blockPositions.Add(t.pos);

        foreach (var t in blocks)
        {
            Vector3 p = t.pos;

            var px1 = blockPositions.Contains(p + px);
            var px2 = blockPositions.Contains(p + nx);
            var px3 = blockPositions.Contains(p + py);
            var px4 = blockPositions.Contains(p + ny);
            var px5 = blockPositions.Contains(p + pz);
            var px6 = blockPositions.Contains(p + nz);

            bool covered = px1 && px2 && px3 && px4 && px5 && px6;

            if (!covered && !t.spawned)
            {
                CubePlacer.instance.SpawnCube(t.pos, t.block, t.forceParent, t.optimized, px1, px2, px3, px4, px5, px6);
            }
            else if (covered && t.spawned)
            {
                Destroy(t.spawned);
            }
        }
    }
}
