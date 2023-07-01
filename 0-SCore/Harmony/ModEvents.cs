
using HarmonyLib;
using System;
using System.Collections.Generic;
using UniLinq;
using UnityEngine;

public class SCoreModEvents
{
    public static List<Transform> ExternalParticles = new List<Transform>();

    public static void Init()
    {
        ModEvents.GameStartDone.RegisterHandler(CheckExternalParticles);
        // When player starts a game
        // ModEvents.GameShutdown.RegisterHandler(new Action(FireManager.Instance.CleanUp));
        //  ModEvents.PlayerSpawnedInWorld.RegisterHandler(new Action<ClientInfo, RespawnType, Vector3i>(FireManager.Instance.Init));

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