using HarmonyLib;

namespace Features.LockPicking
{
    public class TileEntityLootContainerPatches
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";

        // This resets for the quest activations
        [HarmonyPatch(typeof(TileEntityComposite))]
        [HarmonyPatch(nameof(TileEntityComposite.Reset))]
        public class TileEntityLootContainerReset
        {
            private static bool IsSupposedToBeLocked(Vector3i position)
            {
                // Detect which prefab we are at.
                var prefabInstance = GameManager.Instance.GetDynamicPrefabDecorator()
                    ?.GetPrefabAtPosition(position.ToVector3());
                if (prefabInstance == null)
                    return false;

                for (var i = 0; i < prefabInstance.prefab.size.x; i++)
                {
                    for (var j = 0; j < prefabInstance.prefab.size.z; j++)
                    {
                        for (var k = 0; k < prefabInstance.prefab.size.y; k++)
                        {
                            // Find the world position of this block then check if it matches our current position.
                            var num = i + prefabInstance.boundingBoxPosition.x;
                            var num2 = j + prefabInstance.boundingBoxPosition.z;
                            var num7 = World.toBlockY(k + prefabInstance.boundingBoxPosition.y);
                            var localPosition = new Vector3i(num, num7, num2);
                            if (localPosition != position)
                                continue;

                            // Grab the blockValue from the original prefab reference, then use its meta value.
                            var blockValue = prefabInstance.prefab.GetBlock(i, k, j);
                            if (blockValue.ischild)
                                continue;

                            return (blockValue.meta & 4) > 0;
                        }
                    }
                }

                return false;
            }

            public static bool Prefix(TileEntityComposite __instance)
            {
                // Skip player-owned containers — quest resets don't apply lock logic to them.
                var storage = __instance.GetFeature<TEFeatureStorage>();
                if (storage?.bPlayerStorage == true)
                    return true;

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "QuestFullReset"))
                    return true;

                var lockPickable = __instance.GetFeature<TEFeatureLockPickable>();
                if (lockPickable == null)
                    return true; // No lock-pick feature, let reset proceed normally.

                // Still locked — let reset proceed; the locked state is preserved by the feature.
                if (lockPickable.NeedsLockpicking())
                    return true;

                // Container was picked open. If the prefab had it locked, suppress the reset
                // so the loot doesn't silently restock while the block is in its unlocked state.
                return !IsSupposedToBeLocked(__instance.ToWorldPos());
            }
        }
    }
}