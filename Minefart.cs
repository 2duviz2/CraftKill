namespace CraftKill;

using HarmonyLib;
using Mod;
using System.Collections;
using System.IO;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary> Handles loading and accessing the empty scene. </summary>
public static class Minefart
{
    /// <summary> Whether its already loaded. </summary>
    private static bool _loaded = false;

    public const string MinefartSceneName = "Minefart";

    /// <summary> Load the assetbundle containing the scene. </summary>
    public static void Load()
    {
        // istg why does this crash the game when u dont do this
        Plugin.Ass<GameObject>("FirstRoom");
        Plugin.Ass<GameObject>("FirstRoom Pit");

        // load asset bundle :3 meow rawr
        Stream bundleStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("scene.bundle");

        AssetBundleCreateRequest assetRequest = AssetBundle.LoadFromStreamAsync(bundleStream);
        assetRequest.completed += (_) =>
        {
            Plugin.LogInfo("Loaded Empty Scene bundle.");
            _loaded = true;
        };
    }

    /// <summary> Loads the Empty level. </summary>
    public static void LoadLevel() => Plugin.instance.StartCoroutine(LoadLevelAsync());

    /// <summary> Loads the Empty level asynchronously. </summary>
    public static IEnumerator LoadLevelAsync()
    {
        Plugin.LogInfo("Loading Empty Scene.");
        SceneHelper.Instance.loadingBlocker.SetActive(true);
        SceneHelper.SetLoadingSubtext("Loading world...");
        yield return null;

        if (!_loaded)
        {
            Plugin.LogInfo("Empty Scene Bundle wasn't loaded before trying to enter the scene.");
            Load();

            // wait til its loaded
            while (!_loaded) yield return null;
        }

        if (SceneHelper.CurrentScene != MinefartSceneName)
            SceneHelper.LastScene = SceneHelper.CurrentScene;

        SceneHelper.CurrentScene = MinefartSceneName;
        AsyncOperation sceneload = SceneManager.LoadSceneAsync("Assets/Scenes/PeterGriffin.unity");

        // wait til its loaded 
        while (!sceneload.isDone) yield return null;

        Plugin.LogInfo("Scene loaded!");
        SceneHelper.SetLoadingSubtext("");
        SceneHelper.Instance.loadingBlocker.SetActive(false);

        MusicManager.Instance.battleTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.cleanTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.bossTheme.outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;
        MusicManager.Instance.GetComponent<AudioSource>().outputAudioMixerGroup = AudioMixerController.Instance.musicGroup;

        ChallengeManager.Instance.challengePanel.transform.parent.Find("ChallengeText").GetComponent<TMP_Text>().text = "(NO CHALLENGE)";
    }
}

[HarmonyPatch]
public static class SceneHelperPatch
{
    /// <summary> Reload the empty scene when you restart mission in it. </summary>
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SceneHelper), nameof(SceneHelper.RestartSceneAsync))]
    public static bool RestartMissionPatch(ref Coroutine __result)
    {
        if (SceneHelper.CurrentScene.StartsWith(Minefart.MinefartSceneName))
        {
            __result = Plugin.instance.StartCoroutine(Minefart.LoadLevelAsync());
            return false;
        }

        return true;
    }
}