namespace CraftKill.Classes;

using HarmonyLib;
using Mod;
using Mod.Helpers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using ULTRAKILL.Portal;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class CubePlacer : MonoBehaviour
{
    public static CubePlacer instance;

    public Camera cam;

    public List<Block> blocks = [];
    public List<(Block, int)> inventory = [];

    public int selectedBlock = 0;

    public GameObject ghostCube;
    public GameObject placedCube;

    public GameObject ItemsCanvas;
    public Transform ItemsContainer;

    public Material defaultMaterial;
    
    public Dictionary<string, GameObject> assets = [];

    LayerMask layerMask = LayerMask.GetMask("Outdoors", "OutdoorsBaked", "Environment", "EnvironmentBaked");

    public float blockSize = 5;
    public float reach = 25f;
    public bool inventoryDirty = true;

    public float timeWithoutNavmeshUpdate = 100;
    public float navmeshSyncRate = 2.5f;

    public KeyCode toggleKeycode = KeyCode.X;
    public KeyCode changeBlockKeycode = KeyCode.LeftAlt;

    public NavMeshSurface surface;

    public void Start()
    {
        instance = this;
        cam = GetComponent<CameraController>().cam;
        ghostCube = Instantiate(BundleLoader.bundle.LoadAsset<GameObject>("Ghost"));
        ghostCube.transform.localScale = Vector3.one * blockSize;
        ghostCube.SetActive(false);
        placedCube = BundleLoader.bundle.LoadAsset<GameObject>("Block");

        defaultMaterial = GetSandboxMaterial("Procedural Cube");

        SetupAssets();
        SetupItemsCanvas();
        SetupDefaultBlocks();

        if (SceneHelper.CurrentScene == Minefart.MinefartSceneName)
        {
            gameObject.AddComponent<Generation>();
            surface = FindObjectOfType<NavMeshSurface>();
            surface.layerMask = LayerMask.GetMask("Outdoors");
            surface.size = Vector3.one * Generation.chunkSize * blockSize * 2;
        }
    }

    void SetupAssets()
    {
        assets["explosion"] = Plugin.Ass<GameObject>("Assets/Prefabs/Attacks and Projectiles/Explosions/Explosion.prefab");
        assets["pickup"] = BundleLoader.bundle.LoadAsset<GameObject>("Pickup");
        assets["breakParticle"] = BundleLoader.bundle.LoadAsset<GameObject>("BreakParticle");
    }

    void SetupItemsCanvas()
    {
        ItemsCanvas = Instantiate(BundleLoader.bundle.LoadAsset<GameObject>("ItemCanvas"));
        ItemsContainer = ItemsCanvas.transform.Find("Items");
    }

    void SetupDefaultBlocks()
    {
        NewBlock("Default", defaultMaterial.mainTexture, BlockType.block, 5);
        NewBlock("BlowMe", BundleLoader.bundle.LoadAsset<Texture>("blowme"), BlockType.tnt, 1);
        NewBlock("cat", BundleLoader.bundle.LoadAsset<Texture>("cat"), BlockType.block, 0);
        NewBlock("cat2", BundleLoader.bundle.LoadAsset<Texture>("cat2"), BlockType.block, 0);
        NewBlock("Mrbones", BundleLoader.bundle.LoadAsset<Texture>("Mrbones"), BlockType.block, 0);
        NewBlock("StrongRock", BundleLoader.bundle.LoadAsset<Texture>("BackgroundTile"), BlockType.bedrock, 0);
        NewBlock("Lava", Plugin.Ass<Material>("Assets/Materials/Liquids/Lava.mat").mainTexture, BlockType.lava, 0);
        NewBlock("Grass", Plugin.Ass<Material>("Assets/Materials/Environment/Layer 1/Grass.mat").mainTexture, BlockType.bedrock, 0);
    }

    void ReloadInventory()
    {
        Transform templateItem = ItemsContainer.GetChild(0);
        foreach (Transform c in ItemsContainer)
            if (c != templateItem)
                Destroy(c.gameObject);
        templateItem.gameObject.SetActive(false);

        int index = 0;
        foreach (var block in inventory)
        {
            GameObject item = Instantiate(templateItem.gameObject, ItemsContainer);
            item.SetActive(true);
            item.transform.Find("Icon").GetComponent<RawImage>().texture = block.Item1.texture;

            item.transform.Find("Highlight").gameObject.SetActive(selectedBlock == index);

            string amount = block.Item2 == 1 ? "" : block.Item2.ToString();

            item.transform.Find("Amount").GetComponent<TMP_Text>().text = amount;

            index++;
        }
    }

    void NewBlock(string blockID, Texture texture, BlockType type, int chance)
    {
        var block = new Block(blockID, texture, type, chance);
        blocks.Add(block);
    }

    public void ClearBlock(Block block, int amount)
    {
        int index = inventory.FindIndex(x => x.Item1 == block);
        inventoryDirty = true;

        if (index != -1)
        {
            var b = inventory[index];
            b.Item2 -= amount;

            if (b.Item2 <= 0)
            {
                inventory.RemoveAt(index);
                return;
            }

            inventory[index] = b;
        }
    }

    public void GiveBlock(Block block, int amount)
    {
        int index = inventory.FindIndex(x => x.Item1 == block);
        inventoryDirty = true;

        if (index != -1)
        {
            var b = inventory[index];
            b.Item2 += amount;

            inventory[index] = b;
        }
        else
        {
            inventory.Add((block, amount));
        }
    }

    public void Update()
    {
        if (GunControl.Instance.currentWeapon && !GunControl.Instance.currentWeapon.activeSelf && NewMovement.Instance.activated && !OptionsManager.Instance.paused)
        {
            if (Input.GetKey(changeBlockKeycode))
            {
                GunControl.Instance.scrollCooldown = 5;
                selectedBlock += (int)(Input.mouseScrollDelta.y);
                inventoryDirty = true;
            }

            if (selectedBlock >= inventory.Count)
                selectedBlock = 0;
            if (selectedBlock < 0)
                selectedBlock = inventory.Count - 1;

            if (TryRaycastPos(out Vector3 pos, out Transform hitTransform))
            {
                if (inventory.Count > 0)
                {
                    ghostCube.transform.position = pos;
                    ghostCube.SetActive(true);

                    if (InputManager.Instance.InputSource.Fire2.WasPerformedThisFrame && !CheckCollision(NewMovement.Instance.transform, ghostCube.transform))
                    {
                        PlaceCubeByPlayer(pos);
                    }
                }
                else
                {
                    ghostCube.SetActive(false);
                }
                if (InputManager.Instance.InputSource.Fire1.WasPerformedThisFrame)
                {
                    if (hitTransform.name == "CraftKill_Breakable" && hitTransform.GetComponent<BlockInfo>().block.type != BlockType.bedrock)
                    {
                        CreateParticle(hitTransform.position, hitTransform.GetComponent<BlockInfo>().block);
                        CreatePickup(hitTransform.position, hitTransform.GetComponent<BlockInfo>().block);
                        Destroy(hitTransform.gameObject);
                    }
                }
            }
            else
            {
                ghostCube.SetActive(false);
            }
        }
        else
        {
            ghostCube.SetActive(false);
        }

        if (Input.GetKeyDown(toggleKeycode))
        {
            if (GunControl.Instance.currentWeapon)
                GunControl.Instance.currentWeapon.SetActive(!GunControl.Instance.currentWeapon.activeSelf);
        }

        if (inventoryDirty)
        {
            inventoryDirty = false;
            ReloadInventory();
        }

        timeWithoutNavmeshUpdate += Time.deltaTime;
        if (timeWithoutNavmeshUpdate >= navmeshSyncRate && surface)
        {
            timeWithoutNavmeshUpdate = 0;
            surface.transform.position = NewMovement.Instance.transform.position;
            surface.BuildNavMesh();
        }
    }

    public void PlaceCubeByPlayer(Vector3 pos)
    {
        PlaceCube(pos, inventory[selectedBlock].Item1);
        ClearBlock(inventory[selectedBlock].Item1, 1);
    }

    public void PlaceCube(Vector3 pos, Block block, Transform forceParent = null, bool optimized = false)
    {
        GameObject cube = Instantiate(placedCube, pos, Quaternion.identity);

        if (forceParent)
            cube.transform.SetParent(forceParent);

        cube.name = "CraftKill_Breakable";
        cube.transform.localScale = Vector3.one * blockSize;
        cube.layer = LayerMask.NameToLayer("Outdoors");
        cube.tag = "Floor";

        cube.GetComponent<BoxCollider>().size = Vector3.one * 1.05f;
        if (!optimized) cube.AddComponent<PortalAwareRenderer>();

        cube.GetComponent<Renderer>().material = GetMaterial(block);
        cube.AddComponent<BlockInfo>().block = block;

        if (!optimized)
        {
            var navmeshModifier = cube.AddComponent<NavMeshObstacle>();
            navmeshModifier.carving = true;
        }

        var breakable = cube.AddComponent<Breakable>();
        breakable.destroyEvent = new UltrakillEvent
        {
            onActivate = new(),
            onDisActivate = new(),
            toActivateObjects = [],
            toDisActivateObjects = [],
        };
        breakable.destroyOnBreak = [];
        breakable.activateOnBreak = [];

        if (block.type == BlockType.tnt)
        {
            breakable.destroyEvent.onActivate.AddListener(() =>
            {
                var explosion = Instantiate(assets["explosion"], cube.transform.position, Quaternion.identity).GetComponentInChildren<Explosion>();
                explosion.damage = 100;
                explosion.enemyDamageMultiplier = 2;
                explosion.maxSize = 25;
                explosion.speed = 3;
            });
        }
        else if (block.type == BlockType.bedrock)
        {
            Destroy(breakable);
            // do nothing :P
        }
        else if (block.type == BlockType.lava)
        {
            Destroy(breakable);

            Destroy(cube.GetComponent<BoxCollider>());

            GameObject newCube = new GameObject("Water");
            newCube.transform.SetParent(cube.transform, false);
            newCube.transform.localScale = Vector3.one;

            newCube.AddComponent<BoxCollider>().isTrigger = true;

            newCube.AddComponent<Water>().clr = new Color(1, 0.3387192f, 0);

            var hz = newCube.AddComponent<HurtZone>();

            hz.damageType = EnviroDamageType.Burn;
            hz.trigger = true;
            hz.affected = AffectedSubjects.All;
            hz.ignoreDashingPlayer = false;
            hz.ignoreInvincibility = false;
            hz.bounceForce = 25f;
            hz.hurtCooldown = 1f;
            hz.setDamage = 50f;
            hz.hardDamagePercentage = 0.35f;

            newCube.layer = LayerMask.NameToLayer("Water");

            var st = cube.AddComponent<ScrollMe>();
        }
        else
        {
            breakable.destroyEvent.onActivate.AddListener(() =>
            {
                CreateParticle(pos, block);
            });
        }
    }

    public void CreatePickup(Vector3 pos, Block block)
    {
        GameObject pickup = Instantiate(assets["pickup"], pos, Quaternion.identity);

        pickup.layer = LayerMask.NameToLayer("Outdoors");
        pickup.tag = "Floor";

        pickup.GetComponent<Renderer>().material = GetMaterial(block);
        pickup.AddComponent<BlockInfoPickup>().block = block;
    }

    public void CreateParticle(Vector3 pos, Block block)
    {
        GameObject particle = Instantiate(assets["breakParticle"], pos, Quaternion.identity);

        particle.layer = LayerMask.NameToLayer("Outdoors");

        var main = particle.GetComponent<ParticleSystem>().main;
        main.startSizeMultiplier = 1f;
        var shape = particle.GetComponent<ParticleSystem>().shape;
        shape.radius = blockSize / 2f;
        particle.GetComponent<ParticleSystemRenderer>().material = GetMaterial(block);

        particle.AddComponent<CubeParticle>();
    }

    public bool TryRaycastPos(out Vector3 pos, out Transform hitTransform)
    {
        bool result = PortalPhysicsV2.Raycast(cam.transform.position, cam.transform.forward, reach, layerMask, out var hit, out _, out _, queryTriggerInteraction: QueryTriggerInteraction.UseGlobal);
        pos = V3ToGrid(hit.point - cam.transform.forward * 0.1f);
        hitTransform = hit.transform;
        return result;
    }

    public Material GetMaterial(Block block)
    {
        Material newMat = Instantiate(defaultMaterial);
        newMat.mainTexture = block.texture;
        return newMat;
    }

    public Vector3 V3ToGrid(Vector3 grid)
    {
        return new Vector3(GridPos(grid.x), GridPos(grid.y), GridPos(grid.z));
    }

    public float GridPos(float num)
    {
        return Mathf.Round((num - blockSize / 2f) / blockSize) * blockSize + blockSize / 2f ;
    }

    public bool CheckCollision(Transform t1, Transform t2)
    {
        Vector3 p1 = t1.position;
        Vector3 s1 = t1.localScale * 0.5f;

        Vector3 p2 = t2.position;
        Vector3 s2 = t2.localScale * 0.5f;

        bool x = Mathf.Abs(p1.x - p2.x) <= (s1.x + s2.x);
        bool y = Mathf.Abs(p1.y - p2.y) <= (s1.y + s2.y);
        bool z = Mathf.Abs(p1.z - p2.z) <= (s1.z + s2.z);

        return x && y && z;
    }

    Material GetSandboxMaterial(string path)
    {
        GameObject temporalCube = Instantiate(Plugin.Ass<GameObject>($"Assets/Prefabs/Sandbox/{path}.prefab"));
        Material mat = new(temporalCube.GetComponent<Renderer>().material);

        Destroy(temporalCube);

        return mat;
    }

    Material GetPathMaterial(string path) =>
        new(Plugin.Ass<Material>($"Assets/Materials/{path}.mat"));
}

[HarmonyPatch]
public class CameraPatch
{
    [HarmonyPatch(typeof(CameraController), nameof(CameraController.Start))]
    public static void Prefix(CameraController __instance) => __instance.gameObject.AddComponent<CubePlacer>();
}
