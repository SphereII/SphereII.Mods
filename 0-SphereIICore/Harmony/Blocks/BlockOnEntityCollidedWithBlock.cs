using DMT;
using Harmony;
using System;
using UnityEngine;


//  <append xpath="/blocks/block/property[@name='FilterTags' and contains(@value, 'fcrops')]/@value">,fcropsDestroy</append>
//	<property name="FilterTags" value="foutdoor,fcrops,fcropsDestroy"/> 
public class SphereII_Blocks_OnEntityCollidedWithBlock
{
    private static string DestructableTag = "fcropsDestroy";

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("Init")]
    public class SphereII_Block_Init_second
    {
        public static void Postfix(ref Block __instance)
        {
            // Check if the destructable tag is on the block, which triggers the ONEntityCollidedWithBlock
            if (__instance.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructableTag))
                __instance.IsCheckCollideWithEntity = true;

            return;
        }
    }

    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("OnEntityCollidedWithBlock")]
    public class SphereII_Block_OnEntityCollidedWithBlock
    {
        public static bool Prefix(Block __instance, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, Entity _entity)
        {
            // Don't process if its a player.
            if (_entity is EntityPlayerLocal)
                return true;

            if (__instance.FilterTags != null && __instance.FilterTags.ContainsCaseInsensitive(DestructableTag))
                __instance.DamageBlock(_world, 0, _blockPos, _blockValue, Block.list[_blockValue.type].MaxDamage, (_entity != null) ? _entity.entityId : -1, false, false);
            
            return true;


        }
    }
}