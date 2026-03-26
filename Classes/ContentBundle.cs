namespace CraftKill;

using System.IO;
using System.Reflection;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class ContentBundle
{
    public static string info;

    public static void LoadRawBundle()
    {
        if (info == null)
        {
            string directoryName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            info = directoryName;
            if (info != null)
            {
                string str = "Got bundle ";
                Plugin.LogInfo(str + (info != null ? info.ToString() : null));

                string catalogPath = Path.Combine(directoryName, "catalog.json");

                string settingsPath = Path.Combine(directoryName, "settings.json");

                AsyncOperationHandle<IResourceLocator> cat = Addressables.LoadContentCatalogAsync(catalogPath, true, null);
                cat.Completed += (_) =>
                {
                    foreach (string key in cat.Result.Keys)
                    {
                        if (key.StartsWith("Assets/Mods/Shit/"))
                            Plugin.LogInfo(key);
                    }
                    Plugin.LogInfo("Got Content Catalog!");
                };
            }
            else
            {
                Plugin.LogError("Failed to get bundles!");
            }
        }
        if (info != null)
        {
            Plugin.LogInfo("Got bundle");
        }
    }
}
