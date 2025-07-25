using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using Challenges;
//using SCore.Features.EntityPooling.Scripts;
using UniLinq;
using UnityEngine;

public class SCoreModEvents {
    public static List<Transform> ExternalParticles = new List<Transform>();
    private static Shader _defaultShader;
    public static void Init() {
        ModletChecks();
        ModEvents.GameStartDone.RegisterHandler(CheckExternalParticles);
        ModEvents.GameStartDone.RegisterHandler(EntityPool);

        VersionCheck.LoadConfig();
        ModEvents.GameStartDone.RegisterHandler(VersionCheck.OnGameStartDone);

        ModEvents.PlayerSpawnedInWorld.RegisterHandler(CheckGraphicSettings);      
        ModEvents.PlayerSpawnedInWorld.RegisterHandler(AddTriggeredEvents);

        // When player starts a game
        // ModEvents.GameShutdown.RegisterHandler(new Action(FireManager.Instance.CleanUp));
        //  ModEvents.PlayerSpawnedInWorld.RegisterHandler(new Action<ClientInfo, RespawnType, Vector3i>(FireManager.Instance.Init));
    }
 

    private static void AddTriggeredEvents(ref ModEvents.SPlayerSpawnedInWorldData data)
    {
        AddHooks.Initialize();
    }

    private static void CheckGraphicSettings(ref ModEvents.SPlayerSpawnedInWorldData data) {
        ProcessSCoreOptions.ProcessCVars("$SCoreUtils_MemoryBudget");
        ProcessSCoreOptions.ProcessCVars("$SCoreUtils_PPEnable");
    }

    public static Shader GetStandardShader() {
        if (_defaultShader == null)
        {
            _defaultShader = Shader.Find("Standard");
        }

        return _defaultShader;
    }

    private static void EntityPool(ref ModEvents.SGameStartDoneData data) {
        //     GameManager.Instance.gameObject.GetOrAddComponent<EntityFactoryPool>();
    }

    private static void ModletChecks() {
        Log.Out("SCore:: Checking Installed Modlets...");
        CheckLoadedFolders();
        GenerateModletHash();
        Log.Out("SCore:: Done Checking Installed Modlets");
    }

    private static void CheckLoadedFolders() {
        var userDataFolder = GameIO.GetUserGameDataDir() + "/Mods";
        var localModsFolder = (Application.platform == RuntimePlatform.OSXPlayer)
            ? (Application.dataPath + "/../../Mods")
            : (Application.dataPath + "/../Mods");
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
    }

    private static void GenerateModletHash() {
        var hash = string.Empty;
        foreach (var mod in ModManager.GetLoadedMods())
        {
            var modletHash = $"{mod.Name} {mod.VersionString}";
            hash += GetStringSha256Hash(modletHash);
        }

        if (string.IsNullOrEmpty(hash))
        {
            Log.Out("No Mods to check. Skipping.");
            return;
        }

        var sha256 = GetStringSha256Hash(hash);
        if (sha256.Length > 5 )
            sha256= sha256.Substring(0, 5);
        Log.Out($"Modlet List Hash: {sha256}");
    }
    
    public static string GetStringSha256Hash(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        using var sha = new System.Security.Cryptography.SHA256Managed();
        var textData = System.Text.Encoding.UTF8.GetBytes(text);
        var hash = sha.ComputeHash(textData);
        return BitConverter.ToString(hash).Replace("-", string.Empty);
    }

    // Read's the SCore's ExternalParticles from the ConfigFeatureBlock for external particles
    private static void CheckExternalParticles(ref ModEvents.SGameStartDoneData data) {
        const string particleClass = "ExternalParticles";
        var configurationFeatureBlock = Block.GetBlockValue("ConfigFeatureBlock");
        if (configurationFeatureBlock.type == 0)
            return;

        if (!configurationFeatureBlock.Block.Properties.Classes.ContainsKey(particleClass)) return;

        // Register all the new particles to the general particle effects.
        Debug.Log($"Reading External Particles in {configurationFeatureBlock.Block.GetBlockName()}/{particleClass}...");
        var dynamicProperties3 = configurationFeatureBlock.Block.Properties.Classes[particleClass];
        foreach (var keyValuePair in dynamicProperties3.Values.Dict)
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