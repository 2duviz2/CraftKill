namespace CraftKill.Classes;

using System.Collections.Generic;
using UnityEngine;

public class BlockOptimizer : MonoBehaviour
{
    public List<Transform> blocks = [];

    public bool isDirty = false;

    public void Update()
    {
        if (isDirty) FindCulledBlocks();
    }

    public void AddBlock(Transform block)
    {
        blocks.Add(block);
        isDirty = true;
    }

    public void CleanList()
    {
        for (int i = blocks.Count - 1; i >= 0; i--)
            if (!blocks[i]) blocks.RemoveAt(i);
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

        for (int i = blocks.Count - 1; i >= 0; i--)
        {
            var t = blocks[i];
            if (!t)
            {
                blocks.RemoveAt(i);
                continue;
            }

            blockPositions.Add(t.position);
        }

        foreach (var t in blocks)
        {
            Vector3 p = t.position;

            bool covered =
                blockPositions.Contains(p + px) &&
                blockPositions.Contains(p + nx) &&
                blockPositions.Contains(p + py) &&
                blockPositions.Contains(p + ny) &&
                blockPositions.Contains(p + pz) &&
                blockPositions.Contains(p + nz);

            bool shouldBeActive = !covered;

            if (t.gameObject.activeSelf != shouldBeActive)
                t.gameObject.SetActive(shouldBeActive);
        }
    }
}
