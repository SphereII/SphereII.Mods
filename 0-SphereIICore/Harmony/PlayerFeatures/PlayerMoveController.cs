using HarmonyLib;


/**
 * SphereII_PlayerMoveController_Update
 *
 * This class includes a Harmony patches to the EntityPlayer Local to allow skipping of buffs that contain the name "buffcutscene" by pressing space or escape.
 * 
 * This was used in the Winter Project 2019 to skip the opening cutscene, which was applied through a buff.
 * 
 */
[HarmonyPatch(typeof(PlayerMoveController))]
[HarmonyPatch("Update")]
public class SphereII_PlayerMoveController_Update
{
    // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
    static bool Prefix(PlayerMoveController __instance, EntityPlayerLocal ___entityPlayerLocal)
    {
        if (__instance.playerInput.Jump.IsPressed || __instance.playerInput.Menu.IsPressed)
        {
            foreach (BuffValue buff in ___entityPlayerLocal.Buffs.ActiveBuffs)
            {
                if (buff.BuffName.ToLower().Contains("buffcutscene"))
                    ___entityPlayerLocal.Buffs.RemoveBuff(buff.BuffName);
            }
            
        }
        return true;
    }
}



