using HarmonyLib;

namespace Harmony.PlayerFeatures
{
    /**
     * SCoreNerdPoll
     * 
     * This class includes a Harmony patches to the the Block Placement code, so that a player cannot "nerd pole"
     */
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("CanPlaceBlockAt")]
    public class SCoreNerdPoll
    {
        private static readonly string AdvFeatureClass = "AdvancedPlayerFeatures";
        private static readonly string Feature = "AntiNerdPole";

        // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
        private static bool Prefix(ref bool __result)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;


            var player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null) return true;
            if (player.IsGodMode == true) return true;
            if (player.IsFlyMode == true) return true;
            if (player.IsInElevator()) return true;
            if (player.IsInWater()) return true;
            if (player.onGround) return true;
            
            // If we are still here, don't let us place the block
            __result = false;
            return false;
            
        }
    }
}