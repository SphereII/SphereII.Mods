using HarmonyLib;

namespace SCore.Features.LearnByDoing.Harmony
{
    [HarmonyPatch(typeof(BlockSecureLoot))]
    [HarmonyPatch(nameof(BlockSecureLoot.EventData_Event))]
    public class BlockSecureLooteventData_Event
    {
        public static bool Prefix(BlockSecureLoot __instance, TimerEventData timerData)
        {
            var world = GameManager.Instance.World;
            var array = (object[])timerData.Data;
            var num = (int)array[0];
            var vector3i = (Vector3i)array[2];
            var block = world.GetBlock(vector3i);
            if (world.GetTileEntity(num, vector3i) is not TileEntitySecureLootContainer tileEntitySecureLootContainer) return true;
            OnLootContainerPicked.onLootContainerPicked(block);
            return true;
        }
    }
}