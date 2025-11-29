using HarmonyLib;
using UnityEngine;

namespace Harmony.Dialog
{

  
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