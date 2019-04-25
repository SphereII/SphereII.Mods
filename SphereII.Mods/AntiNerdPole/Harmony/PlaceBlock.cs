using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;

[HarmonyPatch(typeof(Block))]
[HarmonyPatch("PlaceBlock")]
public class SphereII_NerdPoll : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
    static bool Prefix(EntityAlive _ea)
    {
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



