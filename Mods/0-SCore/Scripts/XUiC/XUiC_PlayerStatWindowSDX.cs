////using System;
////using System.Collections.Generic;
////using UnityEngine;

//using UnityEngine;
//using HarmonyLib;

////	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
////	Credits: Sirillion.
////	Assists: sphereii, TormentedEmu.

////	Changes hardcoded sprites to fit SMX theme.
////	Difference: Vanilla has some of the sprites hardcoded for hover and selection. This "fixes" that.

//public class SMXui_basepartstack_xuic
//{
//    [HarmonyPatch(typeof(XUiC_BasePartStack))]
//    [HarmonyPatch("SelectedChange")]
//    public class SMXhudBasePartStackSpriteChange
//    {
//        public static void Prefix(ref bool isSelected, ref Color32 ___selectColor, ref XUiController ___background)
//        {
//            //this.SetColor(isSelected ? ___selectColor : XUiC_BasePartStack.backgroundColor);
//            ((XUiV_Sprite)___background.ViewComponent).SpriteName = (isSelected ? "smxlib_slot_frame_narrow" : "smxlib_slot_frame_narrow");
//        }
//    }
//}
//////	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
//////	Credits: SphereII.
//////	Tweaked: Sirillion.

//////	Adds a custom controller with bindings lost with the removal of the PlayerStatWindow controller. Additional compatible bindings added from other controllers.
//////	Difference: Vanilla removed the PlayerStatWindow controller with A20. As such we lost access to a lot of bindings needed to put things onto the HUD, this restores that.

////internal class XUiC_PlayerStatWindow : XUiController
////{
////    public override void Init()
////    {
////        base.Init();
////    }

////    public override bool GetBindingValue(ref string value, string bindingName)
////    {
////        string fieldName = bindingName;

////        // Player LEVEL bindings.
////        if (fieldName == "playerleveltitle")
////        {
////            value = Localization.Get("xuiLevel");
////            return true;
////        }

////        if (fieldName == "playerlevel")
////        {
////            value = ((this.player != null) ? this.playerLevelFormatter.Format(XUiM_Player.GetLevel(this.player)) : "");
////            return true;
////        }

////        if (fieldName == "playerlevelfill")
////        {
////            value = ((this.player != null) ? this.playerLevelFillFormatter.Format(XUiM_Player.GetLevelPercent(this.player)) : "");
////            return true;
////        }

////        // Player DEATH bindings.
////        if (fieldName == "playerdeathstitle")
////        {
////            value = Localization.Get("xuiDeaths");
////            return true;
////        }

////        if (fieldName == "playerdeaths")
////        {
////            value = ((this.player != null) ? this.playerDeathsFormatter.Format(XUiM_Player.GetDeaths(this.player)) : "");
////            return true;
////        }

////        // Player ZOMBIE KILLS bindings.
////        if (fieldName == "playerzombiekillstitle")
////        {
////            value = Localization.Get("xuiZombieKills");
////            return true;
////        }

////        if (fieldName == "playerzombiekills")
////        {
////            value = ((this.player != null) ? this.playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(this.player)) : "");
////            return true;
////        }


////        // Player PVP KILLS bindings.
////        if (fieldName == "playerpvpkillstitle")
////        {
////            value = Localization.Get("xuiPlayerKills");
////            return true;
////        }

////        if (fieldName == "playerpvpkills")
////        {
////            value = ((this.player != null) ? this.playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(this.player)) : "");
////            return true;
////        }


////        // Player LONGEST LIFE bindings.
////        if (fieldName == "playerlongestlifetitle")
////        {
////            value = Localization.Get("xuiLongestLife");
////            return true;
////        }

////        if (fieldName == "playerlongestlife")
////        {
////            value = ((this.player != null) ? XUiM_Player.GetLongestLife(this.player) : "");
////            return true;
////        }

////        // Player TRAVELLED bindings.
////        if (fieldName == "playertravelledtitle")
////        {
////            value = Localization.Get("xuiKMTravelled");
////            return true;
////        }

////        if (fieldName == "playertravelled")
////        {
////            value = ((this.player != null) ? XUiM_Player.GetKMTraveled(this.player) : "");
////            return true;
////        }

////        // Player ITEMS CRAFTED bindings.
////        if (fieldName == "playeritemscraftedtitle")
////        {
////            value = Localization.Get("xuiItemsCrafted");
////            return true;
////        }

////        if (fieldName == "playeritemscrafted")
////        {
////            value = ((this.player != null) ? this.playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(this.player)) : "");
////            return true;
////        }


////        // Player WELLNESS bindings.
////        if (fieldName == "playerwellnesstitle")
////        {
////            value = Localization.Get("xuiWellness");
////            return true;
////        }

////        if (fieldName == "playercoretemptitle")
////        {
////            value = Localization.Get("xuiFeelsLike");
////            return true;
////        }

////        if (fieldName == "playercoretemp")
////        {
////            value = ((this.player != null) ? XUiM_Player.GetCoreTemp(this.player) : "");
////            return true;
////        }

////        if (fieldName == "playerfoodtitle")
////        {
////            value = Localization.Get("xuiFood");
////            return true;
////        }

////        if (fieldName == "playerfood")
////        {
////            value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetFood(this.player)) : "");
////            return true;
////        }

////        if (fieldName == "playerfoodfill")
////        {
////            value = ((this.player != null) ? this.playerFoodFillFormatter.Format(XUiM_Player.GetFoodPercent(this.player)) : "");
////            return true;
////        }

////        if (fieldName == "playerwatertitle")
////        {
////            value = Localization.Get("xuiWater");
////            return true;
////        }

////        if (fieldName == "playerwater")
////        {
////            value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetWater(this.player)) : "");
////            return true;
////        }

////        if (fieldName == "playerwaterfill")
////        {
////            value = ((this.player != null) ? this.playerWaterFillFormatter.Format(XUiM_Player.GetWaterPercent(this.player)) : "");
////            return true;
////        }


////        // From CharacterWindow
////        if (bindingName == "playerlootstagetitle")
////        {
////            value = Localization.Get("xuiLootstage");
////            return true;
////        }

////        if (bindingName == "playerlootstage")
////        {
////            value = ((this.player != null) ? this.player.GetHighestPartyLootStage(0f, 0f).ToString() : "");
////            return true;
////        }

////        if (bindingName == "playerwatermax")
////        {
////            value = ((this.player != null) ? this.playerWaterMaxFormatter.Format(XUiM_Player.GetWaterMax(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playerfoodmax")
////        {
////            value = ((this.player != null) ? this.playerFoodMaxFormatter.Format(XUiM_Player.GetFoodMax(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playerhealth")
////        {
////            value = ((this.player != null) ? this.playerHealthFormatter.Format((int)XUiM_Player.GetHealth(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playermaxhealth")
////        {
////            value = ((this.player != null) ? this.playerMaxHealthFormatter.Format((int)XUiM_Player.GetMaxHealth(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playerstamina")
////        {
////            value = ((this.player != null) ? this.playerStaminaFormatter.Format((int)XUiM_Player.GetStamina(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playermaxstamina")
////        {
////            value = ((this.player != null) ? this.playerMaxStaminaFormatter.Format((int)XUiM_Player.GetMaxStamina(this.player)) : "");
////            return true;
////        }

////        if (bindingName == "playerxptonextlevel")
////        {
////            value = ((this.player != null) ? this.playerXpToNextLevelFormatter.Format(XUiM_Player.GetXPToNextLevel(this.player) + this.player.Progression.ExpDeficit) : "");
////            return true;
////        }

////        if (bindingName == "playercarrycapacity")
////        {
////            value = ((this.player != null) ? this.playerCarryCapacityFormatter.Format((int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, this.player, null, default(FastTags), true, true, true, true, 1, true)) : "");
////            return true;
////        }


////        // From PassiveEffects
////        if (bindingName == "playerbagsize")
////        {
////            value = ((this.player != null) ? this.playerBagSizeFormatter.Format((int)EffectManager.GetValue(PassiveEffects.BagSize, null, 0f, this.player, null, default(FastTags), true, true, true, true, 1, true)) : "");
////            return true;
////        }


////        // From MapWindow
////        if (bindingName == "day")
////        {
////            value = "";
////            if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
////            {
////                value = this.dayFormatter.Format(GameManager.Instance.World.worldTime);
////            }
////            return true;
////        }

////        if (bindingName == "time")
////        {
////            value = "";
////            if (XUi.IsGameRunning() && base.xui.playerUI.entityPlayer != null)
////            {
////                value = this.timeFormatter.Format(GameManager.Instance.World.worldTime);
////            }
////            return true;
////        }


////        // From QuestTurnInDetails
////        if (bindingName == "npcportrait")
////        {
////            if (base.xui.Dialog.Respondent != null)
////            {
////                if (this.NPC is EntityDrone)
////                {
////                    var UISprite = base.xui.GetAtlasByName("ItemIconAtlas", "gunBotT3JunkDrone");

////                }
////                else
////                    value = this.NPC.NPCInfo.Portrait;
////            }
////            return true;
////        }

////        return false;
////    }

////    public override void Update(float _dt)
////    {
////        if (base.ViewComponent.IsVisible && Time.time > this.updateTime)
////        {
////            this.updateTime = Time.time + 0.25f;
////            base.RefreshBindings(this.IsDirty);
////            if (this.IsDirty)
////            {
////                this.IsDirty = false;
////            }
////        }
////        base.Update(_dt);
////    }

////    public override void OnOpen()
////    {
////        base.OnOpen();
////        this.IsDirty = true;
////        this.player = base.xui.playerUI.entityPlayer;
////        this.NPC = base.xui.Dialog.Respondent;
////    }

////    private EntityPlayer player;

////    public EntityNPC NPC;

////    private readonly CachedStringFormatter<int> playerDeathsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<float> playerFoodFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

////    private readonly CachedStringFormatter<float> playerFoodFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

////    private readonly CachedStringFormatter<int> playerItemsCraftedFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<float> playerLevelFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

////    private readonly CachedStringFormatter<int> playerPvpKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<float> playerWaterFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

////    private readonly CachedStringFormatter<float> playerWaterFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

////    private readonly CachedStringFormatter<int> playerZombieKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


////    // From CharacterWindow

////    private readonly CachedStringFormatter<int> playerWaterMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerFoodMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerMaxHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerMaxStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerXpToNextLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

////    private readonly CachedStringFormatter<int> playerCarryCapacityFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


////    // From PassiveEffects

////    private readonly CachedStringFormatter<int> playerBagSizeFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


////    // From MapWindow

////    private readonly CachedStringFormatter<ulong> dayFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{0}"));

////    private readonly CachedStringFormatter<ulong> timeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{1:00}:{2:00}"));

////    private float updateTime;
////    private object itemIconSprite;
////}



