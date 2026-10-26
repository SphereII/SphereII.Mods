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
        [HarmonyPatch(nameof(global::EntityAlive.updateCurrentBlockPosAndValue))]
        public class SCoreBlock_updateCurrentBlockPosAndValue
        {
            public static void Postfix(global::EntityAlive __instance)
            {
                if (__instance is EntityPlayerLocal) return;
                if (__instance is EntityPlayer) return;
                if (__instance.Buffs.GetCustomVar("notrample") > 0f) return;
                if (__instance.HasAnyTags(FastTags<TagGroup.Global>.Parse("notrample"))) return;

                Vector3i blockPosition = __instance.GetBlockPosition();
                var block = GameManager.Instance.World.GetBlock(blockPosition).Block;
                if (block.FilterTags != null && block.FilterTags.ContainsCaseInsensitive(DestructibleTag))
                    block.DamageBlock(GameManager.Instance.World, new BlockValueRef(blockPosition), block.ToBlockValue(), Block.list[block.ToBlockValue().type].MaxDamage, (__instance != null) ? __instance.entityId : -1, default(ItemActionAttack.AttackHitInfo), false);
            }
        }

        // Let the NPCs pass by traps without being hurt.
        // Targets BlockDamage, not Block: BlockDamage overrides this method without calling the
        // base, so a patch on Block never ran for traps. BlockBarbed and BlockSpikes call
        // BlockDamage's version first and stop when it returns false, so they're covered too.
        // The entity is bound by position (__3) because its parameter name has changed between
        // game versions (_entity on Block, _targetEntity on BlockDamage).
        [HarmonyPatch(typeof(BlockDamage))]
        [HarmonyPatch(nameof(BlockDamage.OnEntityCollidedWithBlock))]
        public class SCoreBlockDamage_OnEntityCollidedWithBlock
        {
            public static bool Prefix(BlockDamage __instance, Entity __3)
            {
                if (__3 == null)
                    return false;

                // For hired entities, take a move penalty, but no damage.
                var entityAlive = __3 as global::EntityAlive;
                if (entityAlive == null) return true;

                if (EntityUtilities.GetLeaderOrOwner(entityAlive.entityId) != null )
                {
                    if (__instance.MovementFactor != 1f)
                    {
                      // https://github.com/SphereII/SphereII.Mods/issues/133
                      // entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, __instance.MovementFactor, entityAlive));entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier));
                      entityAlive.SetMotionMultiplier(EffectManager.GetValue(PassiveEffects.MovementFactorMultiplier, null, __instance.MovementFactor, entityAlive));
                    }
                    return false;
                }
                return true;
            }
        }
    }
}