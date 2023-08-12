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
            // distribute the loot contents from the client to the server.
         //   myEntity.SendSyncData(2);

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
            // foreach (var item in myEntity.TraderData.PrimaryInventory)
            // {
            //     myEntity.lootContainer.AddItem(item);
            //     myEntity.TraderData.PrimaryInventory.Remove(item);
            //
            // }
            //myEntity.SendSyncData(2);

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

            // distribute the loot contents from the client to the server.
        //  myEntity.SendSyncData(2);

            // var currentWeapon = myEntity.inventory.holdingItem.GetItemName();
            // if (!string.IsNullOrEmpty(currentWeapon))
            // {
            //     var itemValue = ItemClass.GetItem(currentWeapon);
            //     // var handItem = myEntity.GetDefaultHandItem();
            //     // if (!myEntity.bag.HasItem(handItem))
            //     // {
            //     //     if (!myEntity.lootContainer.HasItem(itemValue) && handItem.type != itemValue.type)
            //     //     {
            //     //         Debug.Log("I no longer have my weapon");
            //             myEntity.UpdateWeapon(itemValue);
            //     //     }
            //     // }
            // }
            return true;
        }
    }
}