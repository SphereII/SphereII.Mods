using HarmonyLib;

namespace Harmony.PlayerFeatures
{
    /**
     * SCoreNerdPoll
     * 
     * This class includes a Harmony patches to the the Block Placement code, so that a player cannot "nerd pole"
     */
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("PlaceBlock")]
    public class SCoreNerdPoll
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "AntiNerdPole";

        // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
        private static bool Prefix(global::EntityAlive _ea)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            var player = _ea as EntityPlayerLocal;
            if (player == null)
                return true;

            if (player.IsGodMode == true)
                return true;

            if (player.IsFlyMode == true)
                return true;

            if (player.IsInElevator())
                return true;

            return player.IsInWater() || player.onGround;

            // If you aren't on the ground, don't place the block.
        }
    }
}