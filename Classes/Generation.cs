namespace CraftKill.Classes;

using HarmonyLib;
using CraftKill;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using CraftKill.Helpers;

public class Generation : MonoBehaviour
{
    public static Generation instance;

    public static int maxHeight = 10;
    public static int chunkSize = 16;
    public static int loadDistance = 4;

    public static int minMobHeight = 3;
    public static float mobChance;
    public static float crateChance = 0.05f / 256f;

    public Dictionary<Vector2Int, GameObject> chunks = [];

    public static List<Dimension> dimensions = [
        new Dimension("Overworld",
        [
            new Biome("Desert", 13, 100, 10, -1,
            [
                Spawnable.Stalker,
                Spawnable.Insurrectionist,
                Spawnable.Virtue,
                Spawnable.Schism,
                Spawnable.Cerberi,
                Spawnable.Maurice,
            ]), 
            new Biome("Plains", 7, 70, 5, 6,
            [
                Spawnable.Filth,
                Spawnable.Soldier,
                Spawnable.Stray,
                Spawnable.Schism,
                Spawnable.Cerberi,
                Spawnable.SwordsMachine,
                Spawnable.Maurice,
                Spawnable.Mass,
                Spawnable.Drone
            ]), 
            new Biome("Hell", 10, 50, 20, 6,
            [
                Spawnable.Cerberi,
                Spawnable.SwordsMachine,
                Spawnable.Mannequin,
                Spawnable.GutterTank,
                Spawnable.GutterMan,
                Spawnable.Providence,
                Spawnable.StreetCleaner,
                Spawnable.Mindflayer
            ]),
            new Biome("Deep Hell", 11, 25, 50, 6,
            [
                Spawnable.MirrorReaper,
                Spawnable.Idol,
                Spawnable.Providence,
                Spawnable.Power,
                Spawnable.Insurrectionist
            ])
        ], 10, "Sky_Overworld"),

        new Dimension("Lust",
        [
            new Biome("Plains", 15, 100, 5, -1,
            [
                Spawnable.Drone,
            ]),
        ], 3, "LustSkybox1", 0.2f),
    ];

    public static int currentDimension = 0;

    public int seed = 0;

    public class Biome(string name, int block, int height, int minItems, int liquidBlock, List<Spawnable> enemies)
    {
        public string name = name;
        public List<string> enemies = [.. enemies.Select(e => EnemyToAddressableKey[e])];
        public int block = block;
        public int height = height;
        public int minItems = minItems;
        public int liquidBlock = liquidBlock;
    }

    public class Dimension(string name, List<Biome> biomes, int maxHeight, string skybox, float perlinScale = 0.04f)
    {
        public string name = name;
        public List<Biome> biomes = biomes;
        public int maxHeight = maxHeight;
        public string skybox = skybox;
        public float perlinScale = perlinScale;
    }

    public static int GetValue(int x, int z, int seed, int maxH, float scale = 0.04f)
    {
        float nx = (x + seed) * scale;
        float nz = (z + seed) * scale;
        float noise = Mathf.PerlinNoise(nx, nz);
        int value = Mathf.FloorToInt(noise * maxH) + 1;

        return Mathf.Clamp(value, 1, maxH);
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

        float blockSize = CubePlacer.blockSize;

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX = chunkX * chunkSize + x;
                int worldZ = chunkZ * chunkSize + z;
                int value = GetValue(worldX, worldZ, seed, dimensions[currentDimension].maxHeight, dimensions[currentDimension].perlinScale);

                var biome = GetBiome(worldX, worldZ, seed);
                var cubePos = GetPos(new Vector3(worldX, value, worldZ), blockSize);

                int cube = GetFloorCube(biome);
                NewCube(cubePos, cube, c);
                CheckForNewMob(worldX, value, worldZ, c.transform, biome);
                CheckForNewCrate(worldX, value, worldZ, c.transform, biome);

                if (value == 1 && biome.liquidBlock > -1)
                {
                    cubePos = GetPos(new Vector3(worldX, value + 1, worldZ), blockSize);
                    NewCube(cubePos, biome.liquidBlock, c);
                }
            }

            if (x % 2 == 0) yield return null;
        }
    }

    public int GetFloorCube(Biome biome)
    {
        return biome.block;
    }

    public static Biome GetBiome(int x, int z, int seed)
    {
        int value = GetValue(x, z, seed + 1, 100, 0.005f);

        Biome bb = null;
        int height = 1000;

        foreach (Biome b in dimensions[currentDimension].biomes)
        {
            if (value <= b.height && height > b.height)
            {
                bb = b;
                height = b.height;
            }
        }

        return bb;
    }

    public static void CheckForNewMob(int worldX, int value, int worldZ, Transform c, Biome biome)
    {
        if (Random.value <= mobChance)
        {
            NewMob(GetPos(new Vector3(worldX, value + 1, worldZ), CubePlacer.blockSize), c, GetEnemy(biome));
        }
    }

    public static string GetEnemy(Biome biome)
    {
        var e = biome.enemies;
        return e[Random.Range(0, e.Count)];
    }

    public static void CheckForNewCrate(int worldX, int value, int worldZ, Transform c, Biome biome)
    {
        if (Random.value <= crateChance)
        {
            if (biome.minItems <= 0) return;

            NewCrate(GetPos(new Vector3(worldX, value + 1, worldZ), CubePlacer.blockSize), c, biome.minItems, (int)(biome.minItems * 1.2f));
        }
    }

    public static void NewMob(Vector3 pos, Transform c, string enemy)
    {
        if (!NewMovement.Instance.activated) return;

        GameObject Enemy = Instantiate(Plugin.Ass<GameObject>(enemy), pos - Vector3.up * (CubePlacer.blockSize / 2f), Quaternion.identity);
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
        crate.transform.localScale = Vector3.one * (CubePlacer.blockSize / 2f);
        crate.transform.position -= Vector3.right * (CubePlacer.blockSize / 2f);
        crate.transform.position += Vector3.forward * (CubePlacer.blockSize / 2f);
        crate.transform.position += Vector3.down * (CubePlacer.blockSize / 2f);
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

            CubePlacer.instance.CreatePickup(pos + new Vector3(Random.value - 0.5f, Random.value - 0.5f, Random.value - 0.5f) * (CubePlacer.blockSize / 2f), blocks[Random.Range(0, blocks.Count)]);
        }
    }

    public static Vector3 GetPos(Vector3 pos, float blockSize)
    {
        return new Vector3(
            pos.x * blockSize,
            pos.y * blockSize - 100,
            pos.z * blockSize) + Vector3.one * blockSize / 2f;
    }

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        Generate(Random.Range(0, 100000));
    }

    public void Update()
    {
        var divider = chunkSize * CubePlacer.blockSize;
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
        mobChance = 0.08f / 256f;
        mobChance *= PrefsManager.Instance.GetInt("difficulty");
        currentDimension = 0;
        UpdateSkybox();
    }

    public void ClearChunks()
    {
        foreach (var chunk in chunks.ToList())
        {
            Destroy(chunk.Value);
            chunks.Remove(chunk.Key);
        }
    }

    public void ChangeDimension(int dimension)
    {
        if (NewMovement.Instance.activated)
            NewMovement.Instance.transform.position = Vector3.zero;

        LevelNamePopup.Instance.CustomNameAppear("CRAFTKILL", dimensions[dimension].name.ToUpper());

        ClearChunks();
        currentDimension = dimension;
        UpdateSkybox();
    }

    public static void UpdateSkybox()
    {
        RenderSettings.skybox = BundleLoader.bundle.LoadAsset<Material>(dimensions[currentDimension].skybox);
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
        rank += 1;
        Generation.SpawnThingies(__instance.transform.position + Vector3.up * 5, rank * 2, rank * 3);
    }
}
