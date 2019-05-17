using DMT;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SphereII__ItemActionPatcher
{
    //public class SphereII_ItemActionFriendlyFire : IHarmony
    //{
    //    public void Start()
    //    {
    //        Debug.Log(" Loading Patch: " + GetType().ToString());
    //        var harmony = HarmonyInstance.Create(GetType().ToString());
    //        harmony.PatchAll(Assembly.GetExecutingAssembly());
    //    }
    //}
    
    //[HarmonyPatch(typeof(ItemActionAttack))]
    //[HarmonyPatch("Hit")]
    //public class SphereII_ItemAction_Hit
    //{
    //    static bool Prefix(ItemActionAttack __instance, World _world, WorldRayHitInfo hitInfo, int _attackerEntityId)
    //    {
    //        // Check if the attacking entity is an EntityAliveSDX; we don't want it to apply to anyone else
    //        EntityAliveSDX AttackingEntity = _world.GetEntity(_attackerEntityId) as EntityAliveSDX;
    //        if(AttackingEntity == null)
    //            return true;

    //        // no Need to pass it down to the base method.
    //        if(hitInfo == null || hitInfo.tag == null)
    //            return false;

    //        // if we hit something, check to see if it's a player or friendly faction
    //        if(hitInfo.bHitValid)
    //        {
    //            // Let's check if our target is an entity
    //            String strBodyPart;
    //            Entity entity = ItemActionAttack.FindHitEntityNoTagCheck(hitInfo, out strBodyPart);
    //            if(entity == null)
    //                return true;

    //            // Check if the target is an NPC that might be in our part.
    //            EntityAliveSDX myTarget = entity as EntityAliveSDX;
    //            if(myTarget)
    //            {
    //                // Don't deal damage to an NPC that is part of your part.
    //                if(AttackingEntity.IsInParty(myTarget.entityId))
    //                    return false;
    //            }

    //            EntityPlayerLocal player = entity as EntityPlayerLocal;
    //            if(player)
    //            {
    //                if(AttackingEntity.IsInParty(player.entityId))
    //                    return false;

    //            }
    //        }
    //        return true;
    //    }
    //}
}