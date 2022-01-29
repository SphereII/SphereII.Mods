using HarmonyLib;
using UnityEngine;

namespace Harmony.Atmosphere
{
    public class Spook
    {
        private const string AdvFeatureClass = "Theme";
        private const string Feature = "Spook";

        // Makes the world in perpetual dark.
        public class SpookSkyManagerBloodMoon
        {
            // Constant Blood Moon
            [HarmonyPatch(typeof(SkyManager))]
            [HarmonyPatch("IsBloodMoonVisible")]
            public static bool Prefix(ref bool __result)
            {
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

                SkyManager.SetSunIntensity(0.3f);
                __result = true;
                return false;
            }
        }

        // Places Blood on the ground when an entity dies.
        public class SpookEntityAliveOnEntityDeath
        {
            [HarmonyPatch(typeof(global::EntityAlive))]
            [HarmonyPatch("OnEntityDeath")]
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