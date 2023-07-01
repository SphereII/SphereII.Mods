using HarmonyLib;

namespace Features.LockPicking
{
    public class TileEntityLootContainerPatches
    {
        private static readonly string AdvFeatureClass = "AdvancedLockpicking";
        private static readonly string Feature = "AdvancedLocks";

        // This resets for the quest activations
        [HarmonyPatch(typeof(TileEntityLootContainer))]
        [HarmonyPatch("Reset")]
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

            public static bool Prefix(TileEntityLootContainer __instance)
            {
                if (__instance.bPlayerStorage || __instance.bPlayerBackpack)
                    return true;

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, "QuestFullReset"))
                    return true;

                switch (__instance)
                {
                    case TileEntitySecure tileEntity when tileEntity.IsLocked():
                        return true;
                    case TileEntitySecure tileEntity:
                    {
                        if (IsSupposedToBeLocked(__instance.ToWorldPos())) // Check the meta of the original blockvalue.
                            tileEntity.SetLocked(true);
                        return true;
                    }
                    case TileEntitySecureLootContainer secureLootContainer when secureLootContainer.IsLocked():
                        return true;
                    case TileEntitySecureLootContainer secureLootContainer
                        : // All non-player secure containers are locked by default.
                    {
                        secureLootContainer.SetLocked(true);
                        return true;
                    }
                    default:
                        return true;
                }
            }
        }
    }
}