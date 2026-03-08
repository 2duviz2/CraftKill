namespace Mod;

using BepInEx;
using BepInEx.Configuration;
using CraftKill;
using HarmonyLib;
using Mod.Helpers;
using Mod.Helpers.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
public class Plugin : BaseUnityPlugin
{
    public static Plugin instance;
    public static ConfigFile config;

    public void Awake()
    {
        instance = this;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        config = Config;

        new Harmony(PluginInfo.GUID).PatchAll();

        SceneManager.sceneLoaded += (_, __) => Invoke(nameof(OnSceneLoad), 0.1f);
    }

    public void OnSceneLoad()
    {
        if (SceneHelper.CurrentScene == "Main Menu")
        {
            foreach (var button in FindObjectsOfType<Button>(true))
            {
                if (button.name == "Sandbox")
                {
                    GameObject copy = Instantiate(button.gameObject, button.transform.parent);
                    copy.GetComponentInChildren<TMP_Text>(true).text = "WORLD";
                    var b = copy.GetComponent<Button>();
                    b.onClick = new();
                    b.onClick.AddListener(() => Minefart.LoadLevel());
                    return;
                }
            }
        }
    }

    public void Start()
    {
        BundleLoader.LoadBundle("assets.bundle");

        foreach (var (type, attr) in AttributeHelper.GetTypesWithAttribute<CreateOnStart>())
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                GameObject obj = new GameObject(type.Name);
                DontDestroyOnLoad(obj);
                obj.AddComponent(type);
            }
        }
    }

    public static T Ass<T>(string path) => Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
    public static void LogInfo(object msg) => instance.Logger.LogInfo(msg);
    public static void LogWarning(object msg) => instance.Logger.LogWarning(msg);
    public static void LogError(object msg) => instance.Logger.LogError(msg);
}

public class PluginInfo
{
    public const string GUID = "duviz.craftkill";
    public const string Name = "CraftKill";
    public const string Version = "1.0.0";
}