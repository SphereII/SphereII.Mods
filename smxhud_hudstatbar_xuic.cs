using HarmonyLib;

//	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
//	Credits: Sirillion.
//	Assists: sphereii, TormentedEmu.

//	Adds an extra binding to separate statmax into its own for current ammo count.
//	Difference: Vanilla has no statmax binding and as such it can only show statcurrentwithmax or statcurrent.

//	Changes the behaviour for YOffset for CollectedItemsList reducing it to 0 as it is not needed with SMX.
//	Difference: Vanilla moves the CollectedItems upwards a bit when holding an ActiveItem in your hand while doing so. Removing it for Vehicles is coming.

public class SMXhud_hudstatbar_xuic
{
	[HarmonyPatch(typeof(XUiC_HUDStatBar))]
	[HarmonyPatch("GetBindingValue")]
	public class SMXhudHudStatBarStatMax
	{
		public static void Postfix(ref bool __result, ref string value, ref string bindingName, ref int ___currentAmmoCount, ref CachedStringFormatter<int> ___statcurrentFormatterInt)
		{
			if (bindingName != null)
			{
				if (bindingName == "statmax")
				{
					value = ___statcurrentFormatterInt.Format(___currentAmmoCount);
					__result = true;
				}
			}
		}
	}
	
	[HarmonyPatch(typeof(XUiC_HUDStatBar))]
	[HarmonyPatch("SetupActiveItemEntry")]
	public class SMXhudHudStatBarYOffsetSetup
	{ 
		public static bool Prefix(ref XUiC_HUDStatBar __instance, ref ItemClass ___itemClass, ref ItemActionAttack ___attackAction, ref ItemValue ___activeAmmoItemValue, ref int ___currentSlotIndex, ref int ___currentAmmoCount, ref string ___lastAmmoName)
		{
			___itemClass = null;
			___attackAction = null;
			___activeAmmoItemValue = ItemValue.None.Clone();
			if (__instance.LocalPlayer != null && __instance.LocalPlayer.inventory.GetItemInSlot(___currentSlotIndex) != null)
			{
				ItemValue itemValue = __instance.LocalPlayer.inventory.GetItem(___currentSlotIndex).itemValue;
				if (itemValue.ItemClass != null)
				{
					ItemActionAttack itemActionAttack = (ItemActionAttack)(itemValue.ItemClass.IsGun() ? __instance.LocalPlayer.inventory.GetItem(___currentSlotIndex).itemValue.ItemClass.Actions[0] : null);
					if (itemActionAttack == null || itemActionAttack is ItemActionMelee || (int)EffectManager.GetValue(PassiveEffects.MagazineSize, __instance.LocalPlayer.inventory.holdingItemItemValue, 0f, __instance.LocalPlayer, null, default(FastTags), true, true, true, true, 1, true) <= 0)
					{
						___currentAmmoCount = 0;
						__instance.xui.CollectedItemList.SetYOffset((__instance.LocalPlayer.AttachedToEntity != null && __instance.LocalPlayer.AttachedToEntity is EntityVehicle) ? 0 : 0);
						return false;
					}
					if (itemActionAttack.MagazineItemNames != null && itemActionAttack.MagazineItemNames.Length != 0)
					{
						___lastAmmoName = itemActionAttack.MagazineItemNames[(int)itemValue.SelectedAmmoTypeIndex];
						___activeAmmoItemValue = ItemClass.GetItem(___lastAmmoName, false);
						___itemClass = ItemClass.GetItemClass(___lastAmmoName, false);
					}
					___attackAction = itemActionAttack;
					__instance.xui.CollectedItemList.SetYOffset(0);
					return false;
				}
				else
				{
					___currentAmmoCount = 0;
					__instance.xui.CollectedItemList.SetYOffset((__instance.LocalPlayer.AttachedToEntity != null && __instance.LocalPlayer.AttachedToEntity is EntityVehicle) ? 0 : 0);
				}
			}
			return false;
		}
	}

	/*
	[HarmonyReversePatch]
	[HarmonyPatch(typeof(XUiC_HUDStatBar), "SetupActiveItemEntry")]
	public static void MySetupActiveItemEntry(object instance)
	{
		// its a stub so it has no initial content
		throw new System.NotImplementedException("It's a stub");
	}


	[HarmonyReversePatch]
	[HarmonyPatch(typeof(XUiC_HUDStatBar), "updateActiveItemAmmo")]
	public static void MyupdateActiveItemAmmo(object instance)
	{
		// its a stub so it has no initial content
		throw new System.NotImplementedException("It's a stub");
	}


	[HarmonyReversePatch]
	[HarmonyPatch(typeof(XUiController), nameof(XUiController.RefreshBindings))]
	[System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
	static void XUiController_RefreshBindings(XUiC_HUDStatBar instance, bool forceAll) { return; }

	[HarmonyPatch(typeof(XUiC_HUDStatBar), nameof(XUiC_HUDStatBar.RefreshBindings))]
	static void Prefix(XUiC_HUDStatBar __instance, bool forceAll)
	{
		XUiController_RefreshBindings(__instance, forceAll);
	}


	[HarmonyPatch(typeof(XUiC_HUDStatBar))]
	[HarmonyPatch("Update")]
	public class SMXhudHudStatBarYOffsetUpdate
	{
		public static bool Prefix(ref XUiC_HUDStatBar __instance, ref float _dt, ref float ___deltaTime, ref EntityVehicle ___Vehicle, ref HUDStatGroups ___statGroup, ref HUDStatTypes ___statType, ref bool ___IsDirty, ref bool ___wasCrouching, ref int ___currentSlotIndex)
		{
			__instance.Update(_dt);
			___deltaTime = _dt;
			if (__instance.LocalPlayer == null && XUi.IsGameRunning())
			{
				// we need to access the internal set property here
				__instance.LocalPlayer = __instance.xui.playerUI.entityPlayer;
			}
			if (___statGroup == HUDStatGroups.Vehicle && __instance.LocalPlayer != null)
			{
				if (__instance.Vehicle == null && __instance.LocalPlayer.AttachedToEntity != null && __instance.LocalPlayer.AttachedToEntity is EntityVehicle)
				{
					___Vehicle = (EntityVehicle)__instance.LocalPlayer.AttachedToEntity;
					___IsDirty = true;
					__instance.xui.CollectedItemList.SetYOffset(0); // SMX COMMENT: Reduced this value from 100 to 0 to "turn off" yOffset
				}
				else if (__instance.Vehicle != null && __instance.LocalPlayer.AttachedToEntity == null)
				{
					___Vehicle = null;
					___IsDirty = true;
				}
			}
			if (___statType == HUDStatTypes.Stealth && __instance.LocalPlayer.IsCrouching != ___wasCrouching)
			{
				___wasCrouching = __instance.LocalPlayer.IsCrouching;
				__instance.RefreshBindings(true);
				___IsDirty = true;
			}
			if (___statType == HUDStatTypes.ActiveItem)
			{
				if (___currentSlotIndex != __instance.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx())
				{
					___currentSlotIndex = __instance.xui.PlayerInventory.Toolbelt.GetFocusedItemIdx();
					___IsDirty = true;
				}
				if (__instance.HasChanged() || ___IsDirty)
				{
					MySetupActiveItemEntry(__instance);
					MyupdateActiveItemAmmo(__instance);
					__instance.RefreshBindings(true);
					___IsDirty = false;
					return false;
				}
			}
			else
			{
				__instance.RefreshFill();
				if (__instance.HasChanged() || ___IsDirty)
				{
					if (___IsDirty)
					{
						___IsDirty = false;
					}
					__instance.RefreshBindings(true);
				}
			}
			return false;
		}
	}*/
}