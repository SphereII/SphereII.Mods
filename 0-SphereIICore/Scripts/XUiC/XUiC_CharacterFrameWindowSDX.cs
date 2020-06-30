
class XUiC_CharacterFrameWindowSDX : XUiController
{
    EntityAlive entity;
    EntityPlayerLocal player;
    private XUiV_Label lblLevel;
    private XUiV_Label lblName;
    private bool isDirty;
    private string levelLabel;


    public override void Init()
    {
        base.Init();
        this.lblLevel = (XUiV_Label)base.GetChildById("levelNumber").ViewComponent;
        this.lblName = (XUiV_Label)base.GetChildById("characterName").ViewComponent;
        this.isDirty = true;
        this.levelLabel = Localization.Get("lblLevel");

    }
    public override void Update(float _dt)
    {
        if (GameManager.Instance == null || GameManager.Instance.World == null)
        {
            return;
        }
        if (this.isDirty)
        {
            this.lblLevel.Text = string.Format(this.levelLabel, entity.Progression.GetLevel());
            this.lblName.Text = this.entity.EntityName.ToUpper();
            this.isDirty = false;
            base.RefreshBindings(false);
        }
        base.Update(_dt);

    }

    public override void OnOpen()
    {
        base.OnOpen();
        this.player = base.xui.playerUI.entityPlayer;
        int entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        UnityEngine.Debug.Log("Entity ID: " + entityID);
        this.entity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (!this.entity)
        {
            UnityEngine.Debug.Log(" Entity is null ");
            OnClose();
        }
    }
    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        UnityEngine.Debug.Log("GetBindingValue(): " + binding.FieldName);
        if (!entity)
            return false;

        int EntityID = this.entity.entityId;
        string fieldName = binding.FieldName;
        switch (fieldName)
        {
            case "npchealth":
                value = this.entity.Health.ToString();
                return true;
            case "npcmaxhealth":
                value = this.entity.GetMaxHealth().ToString();
                return true;
            case "npchealthtitle":
                value = "Health";
                return true;
            case "":
                value = this.entity.Stats.Water.Value.ToString();
                return true;
            case "playerfood":
                value = this.entity.Stats.Stamina.Value.ToString();
                return true;
        }

        return base.GetBindingValue(ref value, binding);
    }

}

