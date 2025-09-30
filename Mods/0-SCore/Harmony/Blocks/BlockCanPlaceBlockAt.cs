using HarmonyLib;
using UnityEngine;

namespace SCore.Harmony.Blocks
{
    public class BlockCanPlaceBlockAt
    {
        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch(nameof(Block.CanPlaceBlockAt))]
        public class BlockCanPlaceBlockAtPatch
        {
            private static FastTags<TagGroup.Poi> blockTags =
                FastTags<TagGroup.Poi>.Parse(Configuration.GetPropertyValue("AdvancedPrefabFeatures",
                    "PrefabTag_NoBuilding"));

            private static string PrefabNames =
                Configuration.GetPropertyValue("AdvancedPrefabFeatures", "PrefabName_NoBuilding");

            private static DynamicPrefabDecorator dynamicPrefabDecorator;

            public static bool Postfix(bool __result, WorldBase _world, int _clrIdx, Vector3i _blockPos)
            {
                if (__result == false) return false;

                if (GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Empty"
                    || GamePrefs.GetString(EnumGamePrefs.GameWorld) == "Playtesting"
                    || GamePrefs.GetString(EnumGamePrefs.GameMode) == "GameModeEditWorld")
                {
                    return __result;
                } 

                
                dynamicPrefabDecorator ??= _world.ChunkCache.ChunkProvider.GetDynamicPrefabDecorator();
                var prefabInstance = dynamicPrefabDecorator.GetPrefabAtPosition(_blockPos);
                if (prefabInstance == null) return true;
                var prefabName = prefabInstance.prefab.LocalizedName;
                if (!string.IsNullOrEmpty(PrefabNames))
                {
                    if (PrefabNames.Contains(prefabName) || PrefabNames.Contains(prefabInstance.prefab.PrefabName))
                    {
                        DisplayBlockedMessage(prefabName);
                        return false;
                    }
                }

                if (!prefabInstance.prefab.Tags.Test_AnySet(blockTags)) return true;
                DisplayBlockedMessage(prefabName);
                return false;
            }

            private static void DisplayBlockedMessage(string prefabName)
            {
                var player = GameManager.Instance.World.GetPrimaryPlayer();
                GameManager.ShowTooltip(player, $"{prefabName} :: {Localization.Get("poi_no_building")}");
            }
        }
    }
}