using HarmonyLib;
using UnityEngine;

/*
 <block name="blah" >
    <property class ="Particles" >
       <property name="OnSpawn" value="unitybundle,unitybundle2"/>

       <!-- If you want to change the particle based on biome -->
       <property name="OnSpawn_pine_forest" value="unitybundle,unitybundle2"/>
       <property name="OnSpawnProb" value="0.2"/>

       <!-- If you want a particle for when it gets damaged -->
       <property name="DamagedParticle" value="unitybundle,unitybundle2"/>
       <property name="DamagedParticle_snow" value="unitybundle,unitybundle2"/>
       <property name="OnDamagedProb" value="0.2"/>
    </property>
</block>
 */

namespace SCore.Features.ParticlesOnBlocks.Scripts {
    public class ParticlesOnBlocks {
        private static GameRandom _random = GameManager.Instance.World.GetGameRandom();

        private static void CheckForParticleOnDamaged(Block block, Vector3i blockPos) {
            if (!block.Properties.Classes.ContainsKey("Particles")) return;
            var particles = block.Properties.Classes["Particles"];
            var particle = GetParticle(particles, "OnDamagedParticle", blockPos);
            if (string.IsNullOrEmpty(particle)) return;

            // Remove any existing particles on this block. 
            // Do we want to do this, or keep the particle going from the original on spawn particle, even if it's damaged?
            // If it's damaged, and has DamagedParticle defined, then we may just want to null the particle.
            BlockUtilitiesSDX.removeParticles(blockPos);

            if (!CanPlaceParticle(particles, "OnDamagedProb")) return;
            BlockUtilitiesSDX.addParticlesCentered(particle,blockPos);
        }

        private static string GetParticle(DynamicProperties classProperty, string key, Vector3i blockPos) {
            
            // Check to see if we have a biome specific particle to use.
            var biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)blockPos.x, (int)blockPos.z);
            var biomeSpecificParticle = key + "_" + biomeAt.m_sBiomeName;
            
            // If not defined, check the block for an overall property
            var availableParticles = classProperty.GetString(biomeSpecificParticle);
            if (string.IsNullOrEmpty(availableParticles))
                availableParticles = classProperty.GetString(key);

            // No particles at all? Boring.
            if (string.IsNullOrEmpty(availableParticles)) return string.Empty;
            
            // Check if the particles are comma delimited.
            var particleArray = availableParticles.Split(',');
            var randomIndex = _random.RandomRange(0, particleArray.Length);
            var particle = particleArray[randomIndex];
            return particle;
        }
        private static bool CanPlaceParticle(DynamicProperties classProperty, string key) {
            // No probability? Allow it all the time.
            var probString = classProperty.GetString(key);
            if (string.IsNullOrEmpty(probString)) return true;

            StringParsers.TryParseFloat(probString, out var prob);
            var rand = _random.RandomRange(0f, 1f);
            return !(rand > prob);
        }
        private static void CheckForParticle(Block block, Vector3i blockPos) {
            if (!block.Properties.Classes.ContainsKey("Particles")) return;
            var particles = block.Properties.Classes["Particles"];
            var particle = GetParticle(particles, "OnSpawnParticle", blockPos);
            if (string.IsNullOrEmpty(particle)) return;
            if (CanPlaceParticle(particles, "OnSpawnProb"))
            {
                BlockUtilitiesSDX.addParticlesCentered(particle,blockPos);
            }
        }
        
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnNeighborBlockChange")]
        public class BlockOnNeighborBlockChange {
            public static void Postfix(Block __instance, Vector3i _myBlockPos) {
                CheckForParticle(__instance, _myBlockPos);
            }
        }
        
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnBlockUnloaded")]
        public class BlockOnBlockUnloaded {
            public static void Postfix(Block __instance, Vector3i _blockPos) {
                CheckForParticle(__instance, _blockPos);
            }
        }
        
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnBlockDamaged")]
        public class BlockOnOnBlockDamaged {
            public static void Postfix(Block __instance, Vector3i _blockPos) {
                CheckForParticleOnDamaged(__instance, _blockPos);
            }
        }
    
        
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnBlockRemoved")]
        public class BlockOnBlockRemoved {
            public static void Postfix(Vector3i _blockPos) {
                BlockUtilitiesSDX.removeParticles(_blockPos);
            }
        }
    }
}