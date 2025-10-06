using HarmonyLib;

namespace SCore.Harmony.PrefabFeatures
{
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch(nameof(Block.CanPlaceBlockAt))]
    public class BlockCanPlaceBlockAt
    {
        private static FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse("traderPlaceable");

        public static void Postfix( ref bool __result, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bOmitCollideCheck)
        {
            if (__result) return;
            if (_blockPos.y > 253) return;
            if (GameManager.Instance.IsEditMode()) return;
        
            var block = _blockValue.Block;
            var world = (World)_world; // Cast once for repeated use

            var isWithinProtection = false;
            if (!block.isMultiBlock)
            {
                // Single block: check position
                isWithinProtection = world.IsWithinTraderPlacingProtection(_blockPos);
            }
            else
            {
                // Multi-block: check bounds
                var bounds = block.multiBlockPos.CalcBounds(_blockValue.type, (int)_blockValue.rotation);
                bounds.center += _blockPos.ToVector3();
                isWithinProtection = world.IsWithinTraderPlacingProtection(bounds);
            }
        
            // If the location is protected by a trader zone:
            if (!isWithinProtection) return;

            // If it doesn't have the required tag, placement is denied.
            if (!block.HasAnyFastTags(tags)) return;

            __result = true;
        }
    }
}
