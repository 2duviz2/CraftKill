namespace CraftKill.Classes;

using HarmonyLib;
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

    public static int minMobHeight = 3;
    public static float mobChance;
    public static float crateChance = 0.05f / 256f;

    public Dictionary<Vector2Int, GameObject> chunks = [];

    public int seed = 0;

    public static int GetValue(int x, int z, int seed)
    {
        float scale = 0.04f;

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
        c.AddComponent<GoreZone>();
        chunks[new Vector2Int(chunkX, chunkZ)] = c;

        float blockSize = CubePlacer.instance.blockSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = chunkX * chunkSize + x;
                int worldZ = chunkZ * chunkSize + z;
                int value = GetValue(worldX, worldZ, seed);

                var cubePos = GetPos(new Vector3(worldX, value, worldZ), blockSize);

                NewCube(cubePos, 7, c);
                CheckForNewMob(worldX, value, worldZ, c.transform);
                CheckForNewCrate(worldX, value, worldZ, c.transform);

                if (value == 1)
                {
                    cubePos = GetPos(new Vector3(worldX, value + 1, worldZ), blockSize);
                    NewCube(cubePos, 6, c);
                }
            }

            if (x % 2 == 0) yield return null;
        }
    }

    public static void CheckForNewMob(int worldX, int value, int worldZ, Transform c)
    {
        if (Random.value <= mobChance)
        {
            NewMob(GetPos(new Vector3(worldX, value + 1, worldZ), CubePlacer.instance.blockSize), c);
        }
    }

    public static void CheckForNewCrate(int worldX, int value, int worldZ, Transform c)
    {
        if (Random.value <= crateChance)
        {
            NewCrate(GetPos(new Vector3(worldX, value + 1, worldZ), CubePlacer.instance.blockSize), c);
        }
    }

    public static List<string> enemies = [
        "Assets/Prefabs/Enemies/Rewrite/Zombie/Filth.prefab",
        "Assets/Prefabs/Enemies/Rewrite/Zombie/Soldier.prefab",
        "Assets/Prefabs/Enemies/Rewrite/Zombie/Stray.prefab",
        "Assets/Prefabs/Enemies/Schism.prefab",
        "Assets/Prefabs/Enemies/Rewrite/Statue/Cerberus.prefab",
        "Assets/Prefabs/Enemies/Rewrite/Machine/Swordsmachine.prefab",
        //"Assets/Prefabs/Enemies/Deathcatcher.prefab",
        "Assets/Prefabs/Enemies/Idol.prefab",
        "Assets/Prefabs/Enemies/Malicious Face.prefab",
        "Assets/Prefabs/Enemies/Mass.prefab",
        "Assets/Prefabs/Enemies/Mannequin.prefab",
        "Assets/Prefabs/Enemies/Guttertank.prefab",
        "Assets/Prefabs/Enemies/Gutterman.prefab",
        "Assets/Prefabs/Enemies/Providence.prefab",
        "Assets/Prefabs/Enemies/PowerWithSpawnEffect.prefab",
        "Assets/Prefabs/Enemies/Drone.prefab",
        "Assets/Prefabs/Enemies/Virtue.prefab",
        "Assets/Prefabs/Enemies/Streetcleaner.prefab",
        "Assets/Prefabs/Enemies/Schism.prefab",
        ];

    public static void NewMob(Vector3 pos, Transform c)
    {
        if (!NewMovement.Instance.activated) return;

        GameObject Enemy = Instantiate(Plugin.Ass<GameObject>(enemies[Random.Range(0, enemies.Count)]), pos - Vector3.up * (CubePlacer.instance.blockSize / 2f), Quaternion.identity);
        if (Enemy.name.StartsWith("Providence")) Enemy.transform.Translate(0, 5, 0);
        Enemy.transform.SetParent(c);
    }

    public static void NewCube(Vector3 cubePos, int block, GameObject c)
    {
        CubePlacer.instance.PlaceCube(
            cubePos,
            CubePlacer.instance.blocks[block],
            c.transform,
            true
        );
    }

    public static void NewCrate(Vector3 pos, Transform c, int min = 30, int max = 50)
    {
        GameObject crate = Instantiate(Plugin.Ass<GameObject>("Assets/Prefabs/Levels/Interactive/Crate.prefab"), pos, Quaternion.identity);
        crate.transform.localScale = Vector3.one * (CubePlacer.instance.blockSize / 2f);
        crate.transform.position -= Vector3.right * (CubePlacer.instance.blockSize / 2f);
        crate.transform.position += Vector3.forward * (CubePlacer.instance.blockSize / 2f);
        crate.transform.position += Vector3.down * (CubePlacer.instance.blockSize / 2f);
        crate.GetComponent<Breakable>().destroyEvent.onActivate.AddListener(() =>
        {
            SpawnThingies(pos, min, max);
        });
        crate.GetComponent<Breakable>().crate = false;
        if (c) crate.transform.SetParent(c);
    }

    public static void SpawnThingies(Vector3 pos, int min, int max)
    {
        List<Block> blocks = [];

        foreach (var b in CubePlacer.instance.blocks)
        {
            if (b.chance == 0) continue;

            for (int i = 0; i < b.chance; i++)
                blocks.Add(b);
        }

        for (int i = 0; i < Random.Range(min, max); i++)
        {
            if (blocks.Count == 0) break;

            CubePlacer.instance.CreatePickup(pos + new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * (CubePlacer.instance.blockSize / 2f), blocks[Random.Range(0, blocks.Count)]);
        }
    }

    public static Vector3 GetPos(Vector3 pos, float blockSize)
    {
        return new Vector3(
            pos.x * blockSize,
            pos.y * blockSize - 100,
            pos.z * blockSize) + Vector3.one * blockSize / 2f;
    }

    public void Start()
    {
        Generate(Random.Range(0, 100000));
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

        SetDefaultChances();

        GenerateChunk(0, 0, seed);
        NewCrate(new Vector3(-5, 3, 5), null, 8, 9);
    }

    public static void SetDefaultChances()
    {
        mobChance = 0.04f / 256f;
        mobChance *= PrefsManager.Instance.GetInt("difficulty");
    }
}

[HarmonyPatch]
public class EnemyDeathRewardPatch
{
    [HarmonyPatch(typeof(EnemyIdentifier), nameof(EnemyIdentifier.ProcessDeath))]
    public static void Prefix(EnemyIdentifier __instance)
    {
        if (SceneHelper.CurrentScene != Minefart.MinefartSceneName) return;
        int rank = EnemyTracker.Instance.GetEnemyRank(__instance);
        if (rank <= 0) return;
        Generation.SpawnThingies(__instance.transform.position + Vector3.up * 5, rank * 2, rank * 3);
    }
}