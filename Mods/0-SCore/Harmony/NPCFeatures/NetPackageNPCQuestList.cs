using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(NetPackageNPCQuestList))]
[HarmonyPatch("ProcessPackage")]
public class NetPackageNPCQuestList_Patch
{
    public static bool Prefix(NetPackageNPCQuestList __instance, World _world)
    {
        if (_world == null) return true;

        // Get the Entity ID from the packet
        // (Note: npcEntityID is public in most versions, but if your build fails, 
        // use AccessTools.Field(typeof(NetPackageNPCQuestList), "npcEntityID").GetValue(__instance))
        int entityID = __instance.npcEntityID;

        // Try to find the entity
        Entity entity = _world.GetEntity(entityID);

        // 1. Check for Null (Entity Removed/Picked Up)
        if (entity == null)
        {
            // The entity no longer exists on the server.
            // Return FALSE to skip the original method and prevent the NullReferenceException.
            return false;
        }

        // 2. Check for Trader Cast Validity
        // The vanilla code does 'as EntityTrader'. If this fails, it causes a crash.
        if (!(entity is EntityTrader))
        {
            return false;
        }

        return true;
    }
}