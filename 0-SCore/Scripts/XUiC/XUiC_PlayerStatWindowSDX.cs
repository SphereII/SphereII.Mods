//using UnityEngine;

//internal class XUiC_PlayerStatWindowSDX : XUiController
//{
//    public override void Init()
//    {
//        base.Init();
//    }

//    public override bool GetBindingValue(ref string value, string bindingName)
//    {
//        string fieldName = bindingName;
//        if (fieldName == "playerfoodfill")
//        {
//            value = ((this.player != null) ? this.playerFoodFillFormatter.Format(XUiM_Player.GetFoodPercent(this.player)) : "");
//            return true;
//        }

//        if (fieldName == "playerlevel")
//        {
//            value = ((this.player != null) ? this.playerLevelFormatter.Format(XUiM_Player.GetLevel(this.player)) : "");
//            return true;
//        }

//        if (fieldName == "playerfoodtitle")
//        {
//            value = Localization.Get("xuiFood");
//            return true;
//        }

//        if (fieldName == "playerdeaths")
//        {
//            value = ((this.player != null) ? this.playerDeathsFormatter.Format(XUiM_Player.GetDeaths(this.player)) : "");
//            return true;
//        }
//        if (fieldName == "playercoretemp")
//        {
//            value = ((this.player != null) ? XUiM_Player.GetCoreTemp(this.player) : "");
//            return true;
//        }
//        if (fieldName == "playerdeathstitle")
//        {
//            value = Localization.Get("xuiDeaths");
//            return true;
//        }

//        if (fieldName == "playerwater")
//        {
//            value = ((this.player != null) ? this.playerWaterFormatter.Format(XUiM_Player.GetWater(this.player)) : "");
//            return true;
//        }
//        if (fieldName == "playerwellnesstitle")
//        {
//            value = Localization.Get("xuiWellness");
//            return true;
//        }
//        if (fieldName == "playertravelledtitle")
//        {
//            value = Localization.Get("xuiKMTravelled");
//            return true;
//        }
//        if (fieldName == "playeritemscraftedtitle")
//        {
//            value = Localization.Get("xuiItemsCrafted");
//            return true;
//        }
//        if (fieldName == "playerlongestlife")
//        {
//            value = ((this.player != null) ? XUiM_Player.GetLongestLife(this.player) : "");
//            return true;
//        }

//        if (fieldName == "playerleveltitle")
//        {
//            value = Localization.Get("xuiLevel");
//            return true;
//        }
//        if (fieldName == "playerfood")
//        {
//            value = ((this.player != null) ? this.playerFoodFormatter.Format(XUiM_Player.GetFood(this.player)) : "");
//            return true;
//        }
//        if (fieldName == "playeritemscrafted")
//        {
//            value = ((this.player != null) ? this.playerItemsCraftedFormatter.Format(XUiM_Player.GetItemsCrafted(this.player)) : "");
//            return true;
//        }
//        if (fieldName == "playerzombiekillstitle")
//        {
//            value = Localization.Get("xuiZombieKills");
//            return true;
//        }


//        if (fieldName == "playerlevelfill")
//        {
//            value = ((this.player != null) ? this.playerLevelFillFormatter.Format(XUiM_Player.GetLevelPercent(this.player)) : "");
//            return true;
//        }

//        if (fieldName == "playerpvpkills")
//        {
//            value = ((this.player != null) ? this.playerPvpKillsFormatter.Format(XUiM_Player.GetPlayerKills(this.player)) : "");
//            return true;
//        }

//        if (fieldName == "playerwatertitle")
//        {
//            value = Localization.Get("xuiWater");
//            return true;
//        }
//        if (fieldName == "playertravelled")
//        {
//            value = ((this.player != null) ? XUiM_Player.GetKMTraveled(this.player) : "");
//            return true;
//        }
//        if (fieldName == "playerpvpkillstitle")
//        {
//            value = Localization.Get("xuiPlayerKills");
//            return true;
//        }
//        if (fieldName == "playerlongestlifetitle")
//        {
//            value = Localization.Get("xuiLongestLife");
//            return true;
//        }
//        if (fieldName == "playerwaterfill")
//        {
//            value = ((this.player != null) ? this.playerWaterFillFormatter.Format(XUiM_Player.GetWaterPercent(this.player)) : "");
//            return true;
//        }
//        if (fieldName == "playerzombiekills")
//        {
//            value = ((this.player != null) ? this.playerZombieKillsFormatter.Format(XUiM_Player.GetZombieKills(this.player)) : "");
//            return true;
//        }

//        if (fieldName == "playercoretemptitle")
//        {
//            value = Localization.Get("xuiFeelsLike");
//            return true;
//        }
//        return false;
//    }

//    public override void Update(float _dt)
//    {
//        if (base.ViewComponent.IsVisible && Time.time > this.updateTime)
//        {
//            this.updateTime = Time.time + 0.25f;
//            base.RefreshBindings(this.IsDirty);
//            if (this.IsDirty)
//            {
//                this.IsDirty = false;
//            }
//        }
//        base.Update(_dt);
//    }

//    public override void OnOpen()
//    {
//        base.OnOpen();
//        this.IsDirty = true;
//        this.player = base.xui.playerUI.entityPlayer;
//    }

//    private EntityPlayer player;

//    private readonly CachedStringFormatter<int> playerDeathsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

//    private readonly CachedStringFormatter<float> playerFoodFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

//    private readonly CachedStringFormatter<float> playerFoodFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

//    private readonly CachedStringFormatter<int> playerItemsCraftedFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

//    private readonly CachedStringFormatter<int> playerLevelFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

//    private readonly CachedStringFormatter<float> playerLevelFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

//    private readonly CachedStringFormatter<int> playerPvpKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

//    private readonly CachedStringFormatter<float> playerWaterFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString("0"));

//    private readonly CachedStringFormatter<float> playerWaterFillFormatter = new CachedStringFormatter<float>((float _i) => _i.ToCultureInvariantString());

//    private readonly CachedStringFormatter<int> playerZombieKillsFormatter = new CachedStringFormatter<int>((int _i) => _i.ToString());

//    private float updateTime;
//}



