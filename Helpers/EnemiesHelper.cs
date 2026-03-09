global using static CraftKill.Helpers.EnemiesHelper;

namespace CraftKill.Helpers;

using System.Collections.Generic;

/// <summary> Enemies handler :3 </summary>
public static class EnemiesHelper
{
    /// <summary> Dictionary of every enemy's addressable key </summary>
    public static Dictionary<Spawnable, string> EnemyToAddressableKey = new()
    {
        // husks rawr
        [Spawnable.Filth] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Filth.prefab",
        [Spawnable.Stray] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Stray.prefab",
        [Spawnable.Schism] = "Assets/Prefabs/Enemies/Schism.prefab",
        [Spawnable.Soldier] = "Assets/Prefabs/Enemies/Rewrite/Zombie/Soldier.prefab",
        [Spawnable.Insurrectionist] = "Assets/Prefabs/Enemies/Sisyphus.prefab",
        [Spawnable.MirrorReaper] = "Assets/Prefabs/Enemies/MirrorReaperCyberGrind.prefab",

        // machines uwu
        [Spawnable.SwordsMachine] = "Assets/Prefabs/Enemies/Rewrite/Machine/Swordsmachine.prefab",
        [Spawnable.Drone] = "Assets/Prefabs/Enemies/Drone.prefab",
        [Spawnable.Mindflayer] = "Assets/Particles/Enemies/MindflayerCharge.prefab",
        [Spawnable.StreetCleaner] = "Assets/Prefabs/Enemies/Streetcleaner.prefab",
        [Spawnable.GutterMan] = "Assets/Prefabs/Enemies/Gutterman.prefab",
        [Spawnable.GutterTank] = "Assets/Prefabs/Enemies/Guttertank.prefab",

        // demons >:3
        [Spawnable.Maurice] = "Assets/Prefabs/Enemies/Malicious Face.prefab",
        [Spawnable.Cerberi] = "Assets/Prefabs/Enemies/Rewrite/Statue/Cerberus.prefab",
        [Spawnable.Mass] = "Assets/Prefabs/Enemies/Mass.prefab",
        [Spawnable.Idol] = "Assets/Prefabs/Enemies/Idol.prefab",
        [Spawnable.Mannequin] = "Assets/Prefabs/Enemies/Mannequin.prefab",

        // angels (angles!!! :O)
        [Spawnable.Providence] = "Assets/Prefabs/Enemies/Providence.prefab",
        [Spawnable.Virtue] = "Assets/Prefabs/Enemies/Virtue.prefab",
        [Spawnable.Power] = "Assets/Prefabs/Enemies/PowerWithSpawnEffect.prefab",

        // prime souls meow
        [Spawnable.MinosPrime] = "Assets/Prefabs/Enemies/MinosPrime.prefab",
        [Spawnable.SisyphusPrime] = "Assets/Prefabs/Enemies/SisyphusPrime.prefab",
    };

    /// <summary> Enum of every enemy type to be used later by the mod. </summary>
    public enum Spawnable 
    {
        // husks
        Filth,
        Stray,
        Schism,
        Soldier,
        Insurrectionist,
        MirrorReaper,

        // machines
        SwordsMachine,
        Drone,
        Mindflayer,
        StreetCleaner,
        GutterMan,
        GutterTank,

        // demon
        Maurice,
        Cerberi,
        Mass,
        Idol,
        Mannequin,

        // angels
        Providence,
        Virtue,
        Power,

        // prime souls
        MinosPrime,
        SisyphusPrime
    }
}
