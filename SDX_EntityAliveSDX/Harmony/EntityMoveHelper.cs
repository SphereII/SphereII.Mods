using Harmony;
using System;
using UnityEngine;

public class SphereII__BlockDamage
{
    [HarmonyPatch(typeof(BlockDamage))]
    [HarmonyPatch("OnEntityCollidedWithBlock")]
    public class SphereII_Block_OnEntityCollidedWithBlock
    {
        static bool Prefix(Entity _targetEntity)
        {
            if(_targetEntity is EntityAliveSDX)
                return false;
            return true;

        }
    }


}
 