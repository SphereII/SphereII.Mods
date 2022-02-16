using System;
using UnityEngine;

//	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
//	Credits: sphereii.
//	Tweaked: Sirillion, TormentedEmu.

//	Adds a custom controller with bindings lost with the removal of the PlayerStatWindow controller. Additional compatible bindings added from other controllers.
//	Difference: Vanilla removed the PlayerStatWindow controller with A20. As such we lost access to a lot of bindings needed to put things onto the HUD, this restores that.


// Usage:  controller="NPCStatWindow, SCore"
public class XUiC_NPCStatWindow : XUiController
{
    public override void Init()
    {
        base.Init();
    }

    public override bool GetBindingValue(ref string value, string bindingName)
    {
        if (bindingName == null) return base.GetBindingValue(ref value, bindingName);

        
        // These bindings only exist under our special class
        if (this.entityAliveSDX)
        {
            var faction = EntityUtilities.GetFactionRelationship(entityAliveSDX, player);

            // We'll use this later.
            var leader = EntityUtilities.GetLeaderOrOwner(entityAliveSDX.entityClass) as EntityAlive;
            switch (bindingName)
            {
                case "isdislike":
                    value = "false";
                    if (faction <= (int)FactionManager.Relationship.Dislike)
                        value = "true";
                    break;
                case "isneutral":
                    value = "false";
                    if (faction >= (int)FactionManager.Relationship.Neutral)
                        value = "true";
                    break;
                case "islike":
                    value = "false";
                    if (faction >= (int)FactionManager.Relationship.Like)
                        value = "true";
                    break;

                case "npcnametitle":
                    value = Localization.Get(entityAliveSDX.Title);
                    break;
                case "npcfirstname":
                    value = Localization.Get(entityAliveSDX.FirstName);
                    break;
                case "npcfaction":
                    value = Localization.Get(FactionManager.Instance.GetFaction(entityAliveSDX.factionId).Name);
                    break;
                case "npclevel":
                    value = entityAliveSDX.Progression.GetLevel().ToString();
                    break;
                case "npchealth":
                    value = entityAliveSDX.Health.ToString();
                    break;
                case "npcmaxhealth":
                    value = entityAliveSDX.GetMaxHealth().ToString();
                    break;
                case "npcexptonextlevel": // I'm not sure if this stuff is hooked up yet
                    value = entityAliveSDX.Progression.ExpToNextLevel.ToString();
                    break;
                case "npcexpfornextlevel": // same, not sure if its here.
                    value = entityAliveSDX.Progression.GetExpForNextLevel().ToString();
                    break;
                case "npcleader": // if leader is null, just break, otherwise send the name back
                    if (leader == null) break;
                    value = leader.EntityName;  // return sphereii
                    break;
                case "npcleaderid": // returns  171
                    if (leader == null) break;
                    value = leader.entityId.ToString();
                    break;
                case "npccurrentholdingitem":
                    value = entityAliveSDX.inventory.holdingItem.GetLocalizedItemName();
                    break;
                case "npcarmor": // not sure if this is hooked up.
                    value = EffectManager.GetValue(PassiveEffects.PhysicalDamageResist, null, 0f, entityAliveSDX, null, default(FastTags), true, true, true, true, 1, true).ToString();
                    break;
                case "npccurrentorder":
                    value = EntityUtilities.GetCurrentOrder(entityAliveSDX.entityId).ToString();
                    break;
                case "npcxp":
                    if (entityAliveSDX?.Progression.ExpDeficit > 0)
                    {
                        float b = Math.Max(this.lastDeficitValue, 0f) * 1.01f;
                        value = this.bindingXp.Format(b);
                        this.currentValue = (float)this.entityAliveSDX.Progression?.ExpDeficit / (float)this.entityAliveSDX.Progression?.GetExpForNextLevel();
                        if (this.currentValue != this.lastDeficitValue)
                        {
                            this.lastDeficitValue = Mathf.Lerp(this.lastDeficitValue, this.currentValue, Time.deltaTime * this.xpFillSpeed);
                            if (Mathf.Abs(this.currentValue - this.lastDeficitValue) < 0.005f)
                            {
                                this.lastDeficitValue = this.currentValue;
                            }
                        }
                    }
                    else
                    {
                        float v2 = Math.Max(this.lastValue, 0f) * 1.01f;
                        value = this.bindingXp.Format(v2);
                        this.currentValue = entityAliveSDX.Progression.GetLevelProgressPercentage();
                        if (this.currentValue != this.lastValue)
                        {
                            this.lastValue = Mathf.Lerp(this.lastValue, this.currentValue, Time.deltaTime * this.xpFillSpeed);
                            if (Mathf.Abs(this.currentValue - this.lastValue) < 0.005f)
                            {
                                this.lastValue = this.currentValue;
                            }
                        }
                    }

                    break;
                case "npcxpcolor":
                    if (entityAliveSDX.Progression?.ExpDeficit > 0)
                    {
                        value = this.expDeficitColor;
                    }
                    else
                    {
                        value = ((this.currentValue == this.lastValue) ? this.standardXPColor : this.updatingXPColor);
                    }
                    break;
                case "npcbagusedslots":
                    value = this.entityAliveSDX.bag.GetUsedSlotCount().ToString();

                    break;
                case "npccarrycapacity":
                    // Grab the Carry Capacity.
                    var unlocked = this.playerCarryCapacityFormatter.Format((int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, entityAliveSDX, null, default(FastTags), true, true, true, true, 1, true));

                    // Grab the Bag size
                    var totalslot = this.playerBagSizeFormatter.Format((int)EffectManager.GetValue(PassiveEffects.BagSize, null, 0f, entityAliveSDX, null, default(FastTags), true, true, true, true, 1, true));

                    // By default, we'll set then unlocked as the right value to show
                    value = unlocked;

                    // If nothing is in total slot, that means its not initialized yet, so just return, using the unlocked value.
                    if (string.IsNullOrEmpty(totalslot))
                        break;

                    // Convert the strings to a number and see which is bigger.
                    if (StringParsers.ParseSInt32(unlocked) > StringParsers.ParseSInt32(totalslot))
                        value = totalslot;
                    else
                        value = unlocked;
                    break;
                case "npcbagfreeslots":
                    var total = StringParsers.ParseSInt32(this.playerBagSizeFormatter.Format((int)EffectManager.GetValue(PassiveEffects.BagSize, null, 0f, this.entityAliveSDX, null, default(FastTags), true, true, true, true, 1, true)));
                    var used = StringParsers.ParseSInt32(this.entityAliveSDX.bag.GetUsedSlotCount().ToString());
                    var freeslots = total - used;
                    value = freeslots.ToString();
                    break;
                case "npcbagfill":
                    value = Mathf.Clamp01((float)this.entityAliveSDX.bag.GetUsedSlotCount() / (float)this.entityAliveSDX.bag.SlotCount).ToCultureInvariantString();
                    break;
                case "npcbagfillcolor":
                    var usedslots = this.entityAliveSDX.bag.GetUsedSlotCount().ToString();
                    var totalslot2 = this.playerCarryCapacityFormatter.Format((int)EffectManager.GetValue(PassiveEffects.CarryCapacity, null, 0f, this.entityAliveSDX, null, default(FastTags), true, true, true, true, 1, true));

                    value = usedslots;

                    if (string.IsNullOrEmpty(totalslot2))
                        break;

                    if (StringParsers.ParseSInt32(usedslots) > StringParsers.ParseSInt32(totalslot2))
                        value = usedslots;
                    else
                        value = totalslot2;

                    var used2 = StringParsers.ParseFloat(usedslots);
                    var total2 = StringParsers.ParseFloat(totalslot2);

                    var percent = used2 / total2;

                    value = "43,124,18,255"; // Green
                    if (percent > 0.75)
                        value = "255,255,0,255"; // Yellow
                    if (percent > 0.90)
                        value = "255,144,24,255"; // Orange
                    if (percent > 1)
                        value = "175,30,25,255"; // Red
                    break;

                // From sphereii - Custom Code - "Free's" NPC Portrait for use.
                case "npcportrait":
                    if (base.xui.Dialog.Respondent != null)
                    {
                        value = this.NPC.NPCInfo.Portrait;
                    }
                    if (string.IsNullOrWhiteSpace(value))
                        value = "npcPortraitGeneric";
                    break;
                // From XUiC_SkillListWindow
                case "skillpointsavailable":
                    string v = this.pointsAvailable;
                    value = this.skillPointsAvailableFormatter.Format(v, entityAliveSDX.Progression.SkillPoints);
                    break;
                case "hasskillpoint":
                    if (entityAliveSDX.Progression.SkillPoints > 0)
                        value = "true";
                    else
                        value = "false";

                    break;
                default:
                    value = "";
                    return true;

            }

        }

        return true;
    }
   
    public override void Update(float _dt)
    {
        if (base.ViewComponent.IsVisible && Time.time > this.updateTime)
        {
            this.updateTime = Time.time + 0.25f;
            if ( entityAliveSDX)
                base.RefreshBindings(this.IsDirty);
            if (this.IsDirty)
            {
                this.IsDirty = false;
            }
        }
        base.Update(_dt);
    }
  
    public override void OnOpen()
    {
        this.IsDirty = true;
        this.player = base.xui.playerUI.entityPlayer;
        this.NPC = base.xui.Dialog.Respondent;
        this.entityAliveSDX = this.NPC as EntityAliveSDX;
        if ( this.entityAliveSDX == null )
        {
            OnClose();
            return;
        }
        base.OnOpen();
    }

    private EntityPlayer player;
    public EntityVehicle Vehicle { get; private set; }

    public EntityNPC NPC;
    public EntityAliveSDX entityAliveSDX;

    private readonly CachedStringFormatter<int> playerDeathsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<float> playerFoodFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

    private readonly CachedStringFormatter<float> playerFoodFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

    private readonly CachedStringFormatter<int> playerItemsCraftedFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<float> playerLevelFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

    private readonly CachedStringFormatter<int> playerPvpKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<float> playerWaterFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

    private readonly CachedStringFormatter<float> playerWaterFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

    private readonly CachedStringFormatter<int> playerZombieKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


    // From CharacterWindow

    private readonly CachedStringFormatter<int> playerWaterMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerFoodMaxFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerMaxHealthFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerMaxStaminaFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerXpToNextLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

    private readonly CachedStringFormatter<int> playerCarryCapacityFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


    // From PassiveEffects

    private readonly CachedStringFormatter<int> playerBagSizeFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());


    // From MapWindow

    private readonly CachedStringFormatter<ulong> dayFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{0}"));

    private readonly CachedStringFormatter<ulong> timeFormatter = new CachedStringFormatter<ulong>((ulong _worldTime) => ValueDisplayFormatters.WorldTime(_worldTime, "{1:00}:{2:00}"));

    // From Toolbelt window
    private CachedStringFormatter<float> bindingXp = new CachedStringFormatter<float>((float _f) => _f.ToCultureInvariantString());

    private float updateTime;

    private string pointsAvailable;
    private float lastDeficitValue;
    private float currentValue;
    private float xpFillSpeed;
    private float lastValue;
    private string expDeficitColor;
    private string standardXPColor;
    private string updatingXPColor;
    private readonly CachedStringFormatter<string, int> skillPointsAvailableFormatter = new CachedStringFormatter<string, int>((string _s, int _i) => string.Format("{1}", _s, _i));
}



