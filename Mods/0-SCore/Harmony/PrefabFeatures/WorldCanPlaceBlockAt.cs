using HarmonyLib;
using UnityEngine;

[HarmonyPatch(typeof(World))]
[HarmonyPatch(nameof(World.CanPlaceBlockAt))]
public class WorldCanPlaceBlockAt
{
    private static FastTags<TagGroup.Global> tags = FastTags<TagGroup.Global>.Parse("traderPlaceable");

    public static void Postfix( ref bool __result, World __instance, Vector3i blockPos, PersistentPlayerData lpRelative, bool traderAllowed = false)
    {
        if (__result) return;

        var player = __instance.GetEntity(lpRelative.EntityId) as EntityPlayer;
        if (player == null) return;

        var holdingItem = player.inventory.GetHoldingBlock();
        var block = holdingItem?.GetBlock();
        if (block == null) return;
        var isWithinProtection = __instance.IsWithinTraderPlacingProtection(blockPos);
     
        // If the location is protected by a trader zone:
        if (!isWithinProtection) return;

        // If it doesn't have the required tag, placement is denied.
        if (!block.HasAnyFastTags(tags)) return;

        __result = true;
    }
}
