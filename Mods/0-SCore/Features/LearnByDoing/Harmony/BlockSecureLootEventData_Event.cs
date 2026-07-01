using HarmonyLib;

namespace SCore.Features.LearnByDoing.Harmony
{
    // Disabled: BlockCompositeTileEntity no longer has EventData_Event in current game build.
    // TODO: Find the correct class/method to patch for secure loot timer events.
    // [HarmonyPatch(typeof(BlockCompositeTileEntity), "EventData_Event")]
    public class BlockSecureLooteventData_Event
    {
        public static bool Prefix(BlockCompositeTileEntity __instance, TimerEventData timerData)
        {
            var world = GameManager.Instance.World;
            var array = (object[])timerData.Data;
            var vector3i = (Vector3i)array[2];
            var block = world.GetBlock(vector3i);
            if (world.GetTileEntity(vector3i) is not TileEntityComposite) return true;
            OnLootContainerPicked.onLootContainerPicked(block);
            return true;
        }
    }
}