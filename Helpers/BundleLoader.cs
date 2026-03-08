namespace Mod.Helpers;

using UnityEngine;
using System.IO;
using System.Reflection;

public static class BundleLoader
{
    public static AssetBundle bundle { get; private set; }

    /// <summary> Loads the requested bundle in BundleLoader.bundle. Can only load one bundle or it will be overwritten </summary>
    /// <param name="path"> Path to the bundle (Folders separated with dots), example: "Assets.MyBundle.bundle" </param>
    public static void LoadBundle(string path)
    {
        var assembly = Assembly.GetExecutingAssembly();

        string resourcePath = $"{path}";

        using var stream = assembly.GetManifestResourceStream(resourcePath);
        if (stream == null) { Plugin.LogError($"Bundle not found: {resourcePath}"); return; }

        UnloadBundle();
        bundle = AssetBundle.LoadFromStream(stream);
    }

    /// <summary> Unloads current bundle, called whenever a new bundle loads </summary>
    public static void UnloadBundle()
    {
        bundle?.Unload(false);
    }
}