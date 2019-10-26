using Harmony;

[HarmonyPatch(typeof(Block))]
[HarmonyPatch("PlaceBlock")]
public class SphereII_NerdPoll 
{
    private static string AdvFeatureClass = "AdvancedPlayerFeatures";
    private static string Feature = "AntiNerdPole";

    // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
    static bool Prefix(EntityAlive _ea)
    {
        // Check if this feature is enabled.
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            return true;

        EntityPlayerLocal player = _ea as EntityPlayerLocal;
        if(player == null)
            return true;

        if(player.IsGodMode == true)
            return true;

        if(player.IsFlyMode == true)
            return true;

        if(player.IsInElevator())
            return true;

        if(player.IsInWater())
            return true;

        // If you aren't on the ground, don't place the block.
        if(!player.onGround)
            return false;

        return true;
    }
}



