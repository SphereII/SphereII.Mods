using HarmonyLib;
using System;

namespace Features.LockPicking
{
    /**
     * SCoreBlocks_OnEntityCollidedWithBlock
     * 
     * This class includes a Harmony patch to allow crop trample, when enables. Any block that has a tag of fcropsDestroy will allow trample.
     * 
     * Also includes a Harmony patch for BlockDamage, which will prevent NPCs.
     * 
     * Usage XML: 
     * Adding to existing blocks:
     * <append xpath="/blocks/block/property[@name='FilterTags' and contains(@value, 'SC_crops')]/@value">,fcropsDestroy</append>
     * Adding to new blocks:
     * <property name="FilterTags" value="foutdoor,fcrops,fcropsDestroy" />
     */
    public class SCoreOnEntityCollidedWithBlock
    {
        private static readonly string DestructibleTag = "fcropsDestroy";


        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("updateCurrentBlockPosAndValue")]
        public class SCoreBlock_updateCurrentBlockPosAndValue
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                if (__instance is EntityPlayerLocal) return;
                if (__instance is EntityPlayer) return;
                if (__instance.Buffs.GetCustomVar("notrample") > 0f) return;
                if (__instance.HasAnyTags(FastTags.Parse("notrample"))) return;

                Vector3i blockPosition = __instance.GetBlockPosition();
                var block = GameManager.Instance.World.GetBlock(0, blockPosition).Block;
                if (block.FilterTags != null && block.FilterTags.ContainsCaseInsensitive(DestructibleTag))
                    block.DamageBlock(GameManager.Instance.World, 0, blockPosition, block.ToBlockValue(), Block.list[block.ToBlockValue().type].MaxDamage, (__instance != null) ? __instance.entityId : -1, null, false) ;
            }
        }

        // Let the NPCs pass by traps without being hurt.
        [HarmonyPatch(typeof(BlockDamage))]
        [HarmonyPatch("OnEntityCollidedWithBlock")]
        public class SCoreBlockDamage_OnEntityCollidedWithBlock
        {
            public static bool Prefix(BlockDamage __instance, Entity _targetEntity)
            {
                if (_targetEntity == null)
                    return false;

                // For hired entities, take a move penalty, but no damage.
                var entityAlive = _targetEntity as global::EntityAlive;
                if (entityAlive == null) return true;

                if (EntityUtilities.GetLeaderOrOwner(entityAlive.entityId) != null )
                {
                    if (__instance.MovementFactor != 1f)
                    {
                        entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, __instance.MovementFactor, entityAlive, null, default(FastTags), true, true, true, true, 1, true));
                    }
                    return false;
                }
                return true;
            }
        }
    }
}