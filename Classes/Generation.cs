namespace CraftKill.Classes;

using Mod;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Generation : MonoBehaviour
{
    public static int maxHeight = 10;
    public static int chunkSize = 16;
    public static int loadDistance = 4;

    public Dictionary<Vector2Int, GameObject> chunks = [];

    public int seed = 0;

    public static int GetValue(int x, int z, int seed)
    {
        float scale = 0.08f;

        float nx = (x + seed) * scale;
        float nz = (z + seed) * scale;
        float noise = Mathf.PerlinNoise(nx, nz);
        int value = Mathf.FloorToInt(noise * maxHeight) + 1;

        return Mathf.Clamp(value, 1, maxHeight);
    }

    public void GenerateChunk(int chunkX, int chunkZ, int seed)
    {
        StartCoroutine(GenerateChunkIE(chunkX, chunkZ, seed));
    }

    public IEnumerator GenerateChunkIE(int chunkX, int chunkZ, int seed)
    {
        GameObject c = new GameObject("Chunk");
        chunks[new Vector2Int(chunkX, chunkZ)] = c;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                float blockSize = CubePlacer.instance.blockSize;
                int worldX = chunkX * chunkSize + x;
                int worldZ = chunkZ * chunkSize + z;
                int value = GetValue(worldX, worldZ, seed);
                while (value > 0)
                {
                    CubePlacer.instance.PlaceCube((new Vector3(
                        worldX * blockSize,
                        value * blockSize - 100,
                        worldZ * blockSize) + Vector3.one * blockSize / 2f),
                        CubePlacer.instance.blocks[0],
                        c.transform,
                        true
                    );

                    value -= 1;
                }
            }
        }

        yield return null;
    }

    public void Start()
    {
        Generate(Random.Range(0, 1000));
    }

    public void Update()
    {
        var divider = chunkSize * CubePlacer.instance.blockSize;
        var plr = new Vector2(NewMovement.Instance.transform.position.x, NewMovement.Instance.transform.position.z);
        Vector2 plrPos = new Vector2(Mathf.Round(plr.x / divider), Mathf.Round(plr.y / divider));

        for (int x = -loadDistance / 2; x <= loadDistance / 2; x++)
        {
            for (int z = -loadDistance / 2; z <= loadDistance / 2; z++)
            {
                var p = plrPos + new Vector2Int(x, z);
                var p2 = new Vector2Int((int)p.x, (int)p.y);
                if (!chunks.ContainsKey(p2))
                {
                    GenerateChunk(p2.x, p2.y, seed);
                }
            }
        }

        foreach (var chunk in chunks.ToList())
        {
            var p = plrPos;
            var p2 = new Vector2Int((int)p.x, (int)p.y);
            if (Vector2.Distance(new Vector2(chunk.Key.x, chunk.Key.y), p2) > loadDistance)
            {
                Destroy(chunk.Value);
                chunks.Remove(chunk.Key);
            }
        }
    }

    public void Generate(int seed)
    {
        Plugin.LogInfo("Generating...");
        this.seed = seed;
        GenerateChunk(0, 0, seed);
    }
}
