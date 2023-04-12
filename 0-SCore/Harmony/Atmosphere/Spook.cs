using HarmonyLib;
using UnityEngine;

namespace Harmony.Atmosphere
{
    /// <summary>
    /// This class of patches manipulate the world to generate a more spooky atmosphere.
    /// </summary>
    public class Spook
    {
        private const string AdvFeatureClass = "Theme";
        private const string Feature = "Spook";
        
        // Constant Blood Moon
        /// <summary>
        /// When SCore's Theme, Spook feature is enabled, the blood moon atmosphere events will be enabled. This includes thunder and lightening.
        /// Note: This does not change the Blood Moon to trigger on any particular day. Just the effects.
        /// </summary>
        [HarmonyPatch(typeof(SkyManager))]
        [HarmonyPatch("IsBloodMoonVisible")]
        public class SkyManagerIsBloodMoonVisible
        {
            public static bool Postfix(bool __result)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return __result;

                // Darken the sun a bit.
                SkyManager.SetSunIntensity(0.3f);
                return true;
            }
        }

        
        /// <summary>
        /// When the SCore's Theme, Spook feature is enabled, any entity that dies will leave being a blood decal.
        /// </summary>
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("OnEntityDeath")]
        public class EntityAlive_OnEntityDeath_BloodSplatter
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return;

                // Spawn location
                Vector3i blockPos;
                blockPos.x = (int)__instance.position.x;
                blockPos.y = (int)__instance.position.y - 1;
                blockPos.z = (int)__instance.position.z;

                // skip if the decal isn't there.
                var block = GameManager.Instance.World.GetBlock(blockPos);
                if (block.hasdecal)
                    return;

                // blood decals
                int[] decals = { 1, 4, 8 };
                block.hasdecal = true;
                block.decalface = BlockFace.Top;
                var index = Random.Range(0, decals.Length);
                block.decaltex = (byte)decals[index];

                var chunk = (Chunk)GameManager.Instance.World.GetChunkFromWorldPos(blockPos);
                if (chunk != null)
                    __instance.world.SetBlockRPC(blockPos, block);
            }
        }
    }
}