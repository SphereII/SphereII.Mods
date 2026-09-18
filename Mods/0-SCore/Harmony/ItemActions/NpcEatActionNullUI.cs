using HarmonyLib;

namespace Harmony.ItemActions
{
    // ItemClass.ExecuteAction's Eat branch looks up the holder's LocalPlayerUI and then sets
    // xui.IsUsingItemActionEntryPromptComplete outside of the UsePrompt check. For an NPC there is
    // no UI, so xui is null and the press throws. That kills Inventory.SimulateActionExecution
    // before its callback runs: the item is never consumed and the held item is never restored.
    //
    // This prefix only steps in when the original would throw (an Eat press, not yet executed,
    // holder has no local player UI). It then runs the original's press sequence without the
    // prompt handling. Everything else goes to the original untouched.
    public class NpcEatActionNullUI
    {
        [HarmonyPatch(typeof(ItemClass))]
        [HarmonyPatch("ExecuteAction")]
        public class ItemClassExecuteAction
        {
            public static bool Prefix(ItemClass __instance, int _actionIdx, ItemInventoryData _data, bool _bReleased)
            {
                if (_bReleased) return true;

                var holder = _data?.holdingEntity;
                if (holder == null) return true;

                if (__instance.Actions == null || _actionIdx < 0 || _actionIdx >= __instance.Actions.Length)
                    return true;
                if (!(__instance.Actions[_actionIdx] is ItemActionEat curAction)) return true;

                if (_data.actionData == null || _actionIdx >= _data.actionData.Count) return true;
                var actionData = _data.actionData[_actionIdx];
                if (actionData == null || actionData.HasExecuted) return true;

                var ui = LocalPlayerUI.GetUIForPlayer(holder as EntityPlayerLocal);
                if (ui != null && ui.xui != null) return true;

                if (curAction.IsActionRunning(actionData)) return false;
                if (!curAction.CanExecute(actionData)) return false;

                if (holder.emodel != null && holder.emodel.avatarController != null)
                    holder.emodel.avatarController.UpdateInt(AvatarController.itemActionIndexHash, _actionIdx);

                holder.MinEventContext.ItemValue = _data.itemValue;
                holder.FireEvent(MinEvent.Start[_actionIdx]);

                holder.MinEventContext.ItemValue = holder.inventory.holdingItemItemValue;
                actionData.HasExecuted = true;
                curAction.ExecuteAction(actionData, _bReleased);
                return false;
            }
        }
    }
}
