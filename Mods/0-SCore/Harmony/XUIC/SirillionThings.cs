//using Audio;
//using Platform;
//using UnityEngine;
//using HarmonyLib;
//using System.Reflection;
//using System.Runtime.CompilerServices;

////	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
////	Credits: Sirillion.
////	Assists: sphereii, TormentedEmu.

////	Adds an extra binding to separate statmax into its own for current ammo count.
////	Difference: Vanilla has no statmax binding and as such it can only show statcurrentwithmax or statcurrent.

//public class SMXui_itemactionentry_xuic
//{
//    [HarmonyPatch(typeof(XUiC_HUDStatBar))]
//    [HarmonyPatch("SetupActiveItemEntry")]
//    public class SMXuiSetUpActiveItemEntry
//    {
//        public static bool Prefix(XUiC_HUDStatBar __instance, int ___currentSlotIndex, ref ItemClass ___itemClass, ref ItemActionAttack ___attackAction)
//        {
//            if (__instance.LocalPlayer != null && __instance.LocalPlayer.inventory.GetItemInSlot(___currentSlotIndex) != null)
//            {
//                ItemValue itemValue = __instance.LocalPlayer.inventory.GetItem(___currentSlotIndex).itemValue;
//                if (itemValue != null && itemValue.ItemClass != null)
//                {
//                    if (itemValue.ItemClass.Actions[0] is ItemActionDynamicMelee)
//                    {
//                        ___attackAction = itemValue.ItemClass.Actions[0] as ItemActionAttack;
//                        ___itemClass = itemValue.ItemClass;
//                        return false;
//                    }
//                }

//            }
//            return true;
//        }
//    }
//}

////		[HarmonyPatch(typeof(XUiC_ItemActionEntry))]
////	[HarmonyPatch("OnHover")]
////	public class SMXuiItemActionEntryOnHover
////	{
////		public static bool Prefix(ref XUiController _sender, ref BaseItemActionEntry ___itemActionEntry,
////							ref bool ___isOver, ref bool _isOver, ref Color32 ___defaultBackgroundColor, ref Color32 ___disabledFontColor, ref Color32 ___defaultFontColor)
////		{
////			XUiV_Sprite xuiV_Sprite = (XUiV_Sprite)_sender.ViewComponent;
////			___isOver = _isOver;
////			if (___itemActionEntry == null)
////			{
////				xuiV_Sprite.Color = ___itemActionEntry != null && ___itemActionEntry.Enabled ? ___defaultFontColor : ___disabledFontColor;
////				//xuiV_Sprite.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////				return false;
////			}
////			if (xuiV_Sprite != null)
////			{
////				if (_isOver)
////				{
////					xuiV_Sprite.Color = ___itemActionEntry != null && ___itemActionEntry.Enabled ? ___defaultBackgroundColor : ___disabledFontColor;
////					//xuiV_Sprite.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////					return false;
////				}
////				xuiV_Sprite.Color = ___itemActionEntry != null && ___itemActionEntry.Enabled ? ___defaultFontColor : ___disabledFontColor;
////				//xuiV_Sprite.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////			}
////			return false;
////		}
////	}


////	[HarmonyPatch(typeof(XUiC_ItemActionEntry))]
////	[HarmonyPatch("OnPressAction")]
////	public class SMXuiItemActionEntryOnPressAction
////	{
////		public static bool Prefix(ref XUiC_ItemActionEntry __instance, ref BaseItemActionEntry ___itemActionEntry,
////							ref XUiV_Sprite ___background, ref bool ___wasPressed,
////							ref Color32 ___defaultBackgroundColor)
////		{
////			EntityPlayerLocal entityPlayer = __instance.xui.playerUI.entityPlayer;
////			if (___itemActionEntry != null)
////			{
////				if (___itemActionEntry.Enabled)
////				{
////					Manager.PlayInsidePlayerHead(___itemActionEntry.SoundName, -1, 0f, false);
////					___itemActionEntry.OnActivated();
////				}
////				else
////				{
////					Manager.PlayInsidePlayerHead(___itemActionEntry.DisabledSound, -1, 0f, false);
////					___itemActionEntry.OnDisabledActivate();
////				}
////				___background.Color = ___defaultBackgroundColor;
////				//___background.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////				___wasPressed = true;
////			}
////			return false;
////		}
////	}


////	[HarmonyPatch(typeof(XUiC_ItemActionEntry))]
////	[HarmonyPatch("Update")]
////	public class SMXuiItemActionEntryUpdate
////	{
////		// We need a reference to the private method. Since its static once, it only has to do this once.
////		private static MethodInfo UpdateBindingsVisibility = AccessTools.Method(typeof(XUiC_ItemActionEntry), "UpdateBindingsVisibility");

////		// To Patch a base method, we need to fake a patch to it.
////		[HarmonyReversePatch]
////		// This is our base class, as XUiC_ItemActionEntry : XUiController (<= that's the base class name )
////		[HarmonyPatch(typeof(XUiController), "Update")]
////		[MethodImpl(MethodImplOptions.NoInlining)]
////		// Method can be called anything, but the first parameter must be the instance reference, along with the parameters.
////		// the { return; } must match the return value of the base method. Note: this isn't used, it's just a reference
////		static void BaseMethodDummy(XUiC_ItemActionEntry instance, float _dt) { return; }


////		public static bool Prefix(ref XUiC_ItemActionEntry __instance, ref BaseItemActionEntry ___itemActionEntry, ref float _dt,
////							ref XUiV_Sprite ___background, ref bool ___isOver, ref bool ___wasPressed, ref bool ___isDirty,
////							ref Color32 ___defaultBackgroundColor, ref PlayerInputManager.InputStyle ___lastInputStyle, ref PlayerInputManager.InputStyle ___curInput)
////		{
////			if (___isOver && UICamera.hoveredObject != ___background.UiTransform.gameObject)
////			{
////				___background.Color = ___defaultBackgroundColor;
////				//___background.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////				___isOver = false;
////			}
////			if (___isOver && ___wasPressed && ___itemActionEntry != null)
////			{
////				___background.Color = Color.white;
////				//___background.SpriteName = ""; // SMX COMMENT: Removed the sprite to allow for custom sprite use.
////				___wasPressed = false;
////			}
////			if (___isDirty)
////			{
////				__instance.RefreshBindings(false);
////				___isDirty = false;
////			}
////			if (___curInput != ___lastInputStyle)
////			{
////				___lastInputStyle = ___curInput;

////				// The Invoke() takes __instance of your patch. The null is parameters. Since the method does not
////				// have parameters, you can just pass in null.
////				UpdateBindingsVisibility.Invoke(__instance, null);
////				//	this.UpdateBindingsVisibility();
////			}
////			//__instance.Update(_dt);
////			BaseMethodDummy(__instance, _dt);
////			return false;
////		}
////	}
////}