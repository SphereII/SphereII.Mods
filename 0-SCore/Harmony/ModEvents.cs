
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using UniLinq;
using UnityEngine;

public class SCoreModEvents
{
    public static List<Transform> ExternalParticles = new List<Transform>();

    public static void Init()
    {
        ModletChecks();
        ModEvents.GameStartDone.RegisterHandler(CheckExternalParticles);
        // When player starts a game
        // ModEvents.GameShutdown.RegisterHandler(new Action(FireManager.Instance.CleanUp));
        //  ModEvents.PlayerSpawnedInWorld.RegisterHandler(new Action<ClientInfo, RespawnType, Vector3i>(FireManager.Instance.Init));

    }

    private static void ModletChecks()
    {
        Log.Out("SCore:: Checking Installed Modlets...");
        CheckLoadedFolders();
        GenerateModletHash();
        Log.Out("SCore:: Done Checking Installed Modlets");
    }

    private static void CheckLoadedFolders()
    {
        var userDataFolder = GamePrefs.GetString(EnumGamePrefs.UserDataFolder) + "/Mods";
        var localModsFolder =  (Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../Mods") : (Application.dataPath + "/../Mods");
        var modFolderCount = 0;
        if (Directory.Exists(userDataFolder))
        {
            modFolderCount++;
            Log.Out($"Loaded Mods From {userDataFolder}");
        }
        if (Directory.Exists(localModsFolder))
        {
            modFolderCount++;
            Log.Out($"Loaded Mods From {localModsFolder}");
        }

        if (modFolderCount > 1)
        {
            Log.Out("WARNING: Loaded Mods From two Folders!");
        }
    }
    private static void GenerateModletHash()
    {
        var hash = string.Empty;
        foreach (var mod in ModManager.GetLoadedMods())
        {
            var modletHash = $"{mod.Name} {mod.VersionString}";
            hash += modletHash.GetHashCode();
        }

        if (string.IsNullOrEmpty(hash))
        {
            Log.Out("No Mods to check. Skipping.");
            return;
        }
        Log.Out($"Modlet List Hash: {hash.GetHashCode()}");
    }
    
    // Read's the SCore's ExternalParticles from the ConfigFeatureBlock for external particles
    private static void CheckExternalParticles()
    {
        const string particleClass = "ExternalParticles";
        var configurationFeatureBlock = Block.GetBlockValue("ConfigFeatureBlock");
        if (configurationFeatureBlock.type == 0)
            return;

        if (!configurationFeatureBlock.Block.Properties.Classes.ContainsKey(particleClass)) return;

        // Register all the new particles to the general particle effects.
        Debug.Log($"Reading External Particles in {configurationFeatureBlock.Block.GetBlockName()}/{particleClass}...");
        var dynamicProperties3 = configurationFeatureBlock.Block.Properties.Classes[particleClass];
        foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
        {
            var particlePath = keyValuePair.Value.ToString();
            if (string.IsNullOrEmpty(particlePath)) continue;

            if (ParticleEffect.IsAvailable(particlePath)) continue;

            Debug.Log($"Registering External Particle: Index: {particlePath.GetHashCode()} for {particlePath}");
            if (!ParticleEffect.IsAvailable(particlePath))
                ParticleEffect.LoadAsset(particlePath);
        }
        Debug.Log("Done Reading External Particles.");
    }

}