using HarmonyLib;
using UnityEngine;

namespace Harmony.Dialog
{

    // Removes the custom IsBusy bool, which pauses custom NPCs in their activities, allowing the player to talk to them.
    [HarmonyPatch(typeof(XUiC_DialogWindowGroup))]
    [HarmonyPatch("OnClose")]
    public class OnClose
    {
        public static bool Prefix(XUiC_DialogWindowGroup __instance)
        {
            if (!__instance.xui.playerUI.entityPlayer.Buffs.HasCustomVar("CurrentNPC")) return true;
            var entityID = (int)__instance.xui.playerUI.entityPlayer.Buffs.GetCustomVar("CurrentNPC");
            var myEntity = __instance.xui.playerUI.entityPlayer.world.GetEntity(entityID) as global::EntityAliveSDX;
            if (myEntity == null) return true;
            myEntity.Buffs.RemoveCustomVar("CurrentPlayer");
            myEntity.emodel.avatarController.UpdateBool("IsBusy", false);
            return true;
        }
    }

    // Removes the custom IsBusy bool, which pauses custom NPCs in their activities, allowing the player to talk to them.
    [HarmonyPatch(typeof(XUiC_DialogWindowGroup))]
    [HarmonyPatch("OnOpen")]
    public class OnOpen
    {
        public static bool Prefix(XUiC_DialogWindowGroup __instance)
        {
            if (!__instance.xui.playerUI.entityPlayer.Buffs.HasCustomVar("CurrentNPC")) return true;
            var entityID = (int)__instance.xui.playerUI.entityPlayer.Buffs.GetCustomVar("CurrentNPC");
            var myEntity = __instance.xui.playerUI.entityPlayer.world.GetEntity(entityID) as global::EntityAliveSDX;
            if (myEntity == null) return true;
            
            myEntity.emodel.avatarController.UpdateBool("IsBusy", true);
            myEntity.RotateTo(__instance.xui.playerUI.entityPlayer, 8f, 8f);
            myEntity.SetLookPosition(__instance.xui.playerUI.entityPlayer.getHeadPosition());
            return true;
        }
    }
    
    [HarmonyPatch(typeof(XUiC_LootWindowGroup))]
    [HarmonyPatch("OnClose")]
    public class XUiC_LootWindowGroupOnClose
    {
        public static bool Prefix(XUiC_DialogWindowGroup __instance)
        {
            if (!__instance.xui.playerUI.entityPlayer.Buffs.HasCustomVar("CurrentNPC")) return true;
            var entityID = (int)__instance.xui.playerUI.entityPlayer.Buffs.GetCustomVar("CurrentNPC");
            var myEntity = __instance.xui.playerUI.entityPlayer.world.GetEntity(entityID) as global::EntityAliveSDX;
            if (myEntity == null) return true;
            myEntity.UpdateWeapon();
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                myEntity.SendSyncData();
            }
            return true;
        }
    }
}