using Audio;
using HarmonyLib;

namespace Harmony.Recipes
{
    public class XUiCRecipeStackOutputStackPatches
    {
        [HarmonyPatch(typeof(XUiC_RecipeStack))]
        [HarmonyPatch(nameof(XUiC_RecipeStack.outputStack))]
        public class XUiCRecipeStackOutputStack
        {
            public static bool Prefix(XUiC_RecipeStack __instance)
            {
                if (__instance.recipe == null) return true;
                var entityPlayer = __instance.xui.playerUI.entityPlayer;
                if (entityPlayer == null) return true;
                var childByType = __instance.windowGroup.Controller.GetChildByType<XUiC_WorkstationOutputGrid>();
                if (childByType == null) return true;
                if (__instance.originalItem != null && !__instance.originalItem.Equals(ItemValue.None)) return true;
                var outputItemValue = new ItemValue(__instance.recipe.itemValueType, __instance.outputQuality,
                    __instance.outputQuality);
                var itemClass = outputItemValue.ItemClass;
                var additionalOutput = itemClass.Properties.GetString("additional_output");
                if (string.IsNullOrEmpty(additionalOutput)) return true;

                foreach (var item in additionalOutput.Split(","))
                {
                    var additionalItem = item;
                    var additionalCount = 1;
                    if (additionalOutput.Contains(":"))
                    {
                        additionalItem = item.Split(':')[0];
                        additionalCount = StringParsers.ParseSInt32(item.Split(':')[1]);
                    }

                    var itemValue = ItemClass.GetItem(additionalItem);
                    var itemStack = new ItemStack(itemValue, additionalCount);
                    ItemStack[] slots = childByType.GetSlots();
                    var flag = false;
                    foreach (var t in slots)
                    {
                        if (t.CanStackWith(itemStack, false)) continue;
                        t.count += additionalCount;
                        flag = true;
                        break;
                    }

                    if (!flag)
                    {
                        for (var j = 0; j < slots.Length; j++)
                        {
                            if (!slots[j].IsEmpty()) continue;
                            slots[j] = itemStack;
                            flag = true;
                            break;
                        }
                    }

                    if (flag)
                    {
                        childByType.SetSlots(slots);
                        childByType.UpdateData(slots);
                        childByType.IsDirty = true;
                    }
                    else if (!__instance.AddItemToInventory())
                    {
                        __instance.isInventoryFull = true;
                        var text =
                            "No room in workstation output, crafting has been halted until space is cleared.";
                        if (Localization.Exists("wrnWorkstationOutputFull", false))
                        {
                            text = Localization.Get("wrnWorkstationOutputFull", false);
                        }

                        GameManager.ShowTooltip(entityPlayer, text, false, false, 0f);
                        Manager.PlayInsidePlayerHead("ui_denied", -1, 0f, false, false);
                        return false;
                    }
                }

                return true;
            }
        }
    }
}