using HarmonyLib;
using System;

/**
 * SphereII_Blocks_OnEntityCollidedWithBlock
 * 
 * This class includes a Harmony patch to allow crop trample, when enables. Any block that has a tag of fcropsDestroy will allow trample.
 * 
 * Also includes a Harmony patch for BlockDamage, which will prevent NPCs.
 * 
 * Usage XML: 
 *  Adding to existing blocks:
 *    <append xpath="/blocks/block/property[@name='FilterTags' and contains(@value, 'fcrops')]/@value">,fcropsDestroy</append>
 *
 * Adding to new blocks:
 *    <property name="FilterTags" value="foutdoor,fcrops,fcropsDestroy"/> 
 */
public class SphereII_Blocks_OnEntityCollidedWithBlock
{
    private static readonly string DestructableTag = "fcropsDestroy";

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("Init")]
    public class SphereII_Block_Init_second
    {
        public static void Postfix(ref Block __instance)
        {
            if (__instance == null)
                return;
            // Check if the destructable tag is on the block, which triggers the ONEntityCollidedWithBlock
            if (__instance.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructableTag))
                __instance.IsCheckCollideWithEntity = true;

            return;
        }
    }

    //[HarmonyPatch(typeof(Block))]
    //[HarmonyPatch("OnEntityCollidedWithBlock")]
    //public class SphereII_Block_OnEntityCollidedWithBlock
    //{
    //    public static bool Prefix(Block __instance, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
    //    {
    //        if (_entity == null)
    //            return false;

    //        // Don't process if its a player.
    //        if (_entity is EntityPlayerLocal)
    //            return false;

    //        try
    //        {
    //            if (__instance.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructableTag))
    //                __instance.DamageBlock(_world, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, (_entity != null) ? _entity.entityId : -1, false, false);
    //        }
    //        catch(Exception ex)
    //        {
    //            return false;
    //        }
    //        return false;


    //    }
    //}

    //// Let the NPCs pass by traps without being hurt.
    //[HarmonyPatch(typeof(BlockDamage))]
    //[HarmonyPatch("OnEntityCollidedWithBlock")]
    //public class SphereII_BlockDamage_OnEntityCollidedWithBlock
    //{
    //    public static bool Prefix(Entity _targetEntity)
    //    {
    //        if (_targetEntity == null)
    //            return false;

    //        if (_targetEntity is EntityNPC)
    //            return false;
    //        return true;
    //    }
    //}

}