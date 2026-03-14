namespace UltrakillStupid.Patches;

using CraftKill.Classes;
using HarmonyLib;

[HarmonyPatch(typeof(LeaderboardController))]
[HarmonyPatch("SubmitLevelScore")]
internal class LeaderboardControllerPatch
{
    public static bool Prefix(string levelName, int difficulty, float seconds, int kills, int style, int restartCount, bool pRank = false)
    {
        return CubePlacer.instance == null;
    }
}