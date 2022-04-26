using HarmonyLib;
using System;

namespace Harmony.Blocks
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
    public class OnEntityCollidedWithBlock
    {
        private static readonly string DestructibleTag = "fCropsDestroy";

        [HarmonyPatch(typeof(BlockPlantGrowing))]
        [HarmonyPatch("LateInit")]
        public class Init
        {
            public static void Postfix(ref BlockPlantGrowing __instance)
            {
                // Check if the destructible tag is on the block, which triggers the ONEntityCollidedWithBlock
                if (__instance?.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructibleTag))
                    __instance.IsCheckCollideWithEntity = true;
            }
        }

        [HarmonyPatch(typeof(Block))]
        [HarmonyPatch("OnEntityCollidedWithBlock")]
        public class SCoreBlock_OnEntityCollidedWithBlock
        {
            public static bool Prefix(Block __instance, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
            {
                // If it's not a plant, do what you do best.
                if (_blockValue.Block is BlockPlantGrowing )
                {
                    // Don't process if its a player.
                    if (_entity is EntityPlayerLocal)
                      return true;

                    if (__instance == null) return true;
                    if (__instance.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructibleTag))
                        __instance.DamageBlock(_world, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, (_entity != null) ? _entity.entityId : -1, false, false);

                }
                return true;
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