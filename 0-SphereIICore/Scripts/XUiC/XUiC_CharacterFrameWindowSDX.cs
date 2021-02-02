
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
        lblLevel = (XUiV_Label)base.GetChildById("levelNumber").ViewComponent;
        lblName = (XUiV_Label)base.GetChildById("characterName").ViewComponent;
        isDirty = true;
        levelLabel = Localization.Get("lblLevel");

    }
    public override void Update(float _dt)
    {
        if (GameManager.Instance == null || GameManager.Instance.World == null)
        {
            return;
        }
        if (isDirty)
        {
            lblLevel.Text = string.Format(levelLabel, entity.Progression.GetLevel());
            lblName.Text = entity.EntityName.ToUpper();
            isDirty = false;
            base.RefreshBindings(false);
        }
        base.Update(_dt);

    }

    public override void OnOpen()
    {
        base.OnOpen();
        player = base.xui.playerUI.entityPlayer;
        int entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        UnityEngine.Debug.Log("Entity ID: " + entityID);
        entity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (!entity)
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

        int EntityID = entity.entityId;
        string fieldName = binding.FieldName;
        switch (fieldName)
        {
            case "npchealth":
                value = entity.Health.ToString();
                return true;
            case "npcmaxhealth":
                value = entity.GetMaxHealth().ToString();
                return true;
            case "npchealthtitle":
                value = "Health";
                return true;
            case "":
                value = entity.Stats.Water.Value.ToString();
                return true;
            case "playerfood":
                value = entity.Stats.Stamina.Value.ToString();
                return true;
        }

        return base.GetBindingValue(ref value, binding);
    }

}

