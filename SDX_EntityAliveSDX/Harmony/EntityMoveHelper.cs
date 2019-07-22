using Harmony;
using UnityEngine;
public class SphereII__BlockDamage
{
    // Don't let the NPCs blindly break blocks.
    [HarmonyPatch(typeof(BlockDamage))]
    [HarmonyPatch("OnEntityCollidedWithBlock")]
    public class SphereII_Block_OnEntityCollidedWithBlock
    {
        static bool Prefix(Entity _targetEntity)
        {
            // If they have the AI tasks for breaking blocks or destroy area, let them have their fun.
            //if (EntityUtilities.HasTask(_targetEntity.entityId, "BreakBlock") || EntityUtilities.HasTask(_targetEntity.entityId, "DestroyArea"))
            //{
            //   return true;
            //}


            //Debug.Log("OnEntityCollidedWithBlock() false");
            //return false;
            return true;
        }
    }

    // Don't let the NPCs blind collider with things. This causes them to stand on each other.
    [HarmonyPatch(typeof(Entity))]
    [HarmonyPatch("OnCollidedWithEntity")]
    public class SphereII_entity_OnCollidedWithEntity
    {
        static bool Prefix(Entity _entity)
        {
            EntityAliveSDX myEntity = (_entity as EntityAliveSDX);
            if(myEntity)
            {
                  Debug.Log("Colliding with: " + _entity.ToString());
                return false;
            }
            return true;

        }
    }

}
