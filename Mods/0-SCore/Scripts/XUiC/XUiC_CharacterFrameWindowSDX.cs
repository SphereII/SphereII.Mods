using System.Collections.Generic;

public class XUiC_CharacterFrameWindowSDX : XUiController
{
    private EntityAliveSDX entity;
    private bool isDirty;
    private XUiV_Label lbldescriptionText;
    private EntityPlayerLocal player;

    private Dictionary<int, ProgressionValue> skills = new Dictionary<int, ProgressionValue>();
    private Dictionary<int, ProgressionValue> currentSkills = new Dictionary<int, ProgressionValue>();

    public override void OnClose()
    {
        entity = null;
        base.OnClose();
    }
    public override void Init()
    {
        base.Init();
        lbldescriptionText = (XUiV_Label)GetChildById("descriptionText").ViewComponent;
        isDirty = true;
    }

    public string GetDescription()
    {
        string description = "";
        description += $"{entity.EntityName} ({entity.entityId})\n";
        description += $"--------------------\n";
        description += $"{entity.Health.ToString()}/{entity.GetMaxHealth()} HP\n";
        description += $"Level: {entity.Progression.GetLevel()}\n";
        description += $"Exp: {entity.Progression.GetExpForNextLevel()} to next level.\n";

        description += $"Skills:\n";
        skills.Clear();
        currentSkills.Clear();
        skills = entity.Progression.GetDict();
        //entity.Progression.ProgressionValues.Dict.CopyValuesTo(skills);
        foreach (var progressionValue in this.skills)
        {
            if (progressionValue.Value.Level > 1)
                this.currentSkills.Add(progressionValue.Key, progressionValue.Value);

            if (player.IsGodMode != true) continue;

            var temp = $"{Localization.Get(progressionValue.Value.Name)} ";
            if (progressionValue.Value.ProgressionClass.IsPerk)
                temp += " (Perk): ";
            if (progressionValue.Value.ProgressionClass.IsAttribute)
                temp += " (Attribute): ";
            if (progressionValue.Value.ProgressionClass.IsBook)
                temp += " (Book): ";

          //  Log.Out($" {temp}: {progressionValue.Level}");
        }

       // currentSkills.Sort(ProgressionClass.ListSortOrderComparer.Instance);
        foreach (var skill in currentSkills)
        {
            var temp = $"{Localization.Get(skill.Value.Name)} ";
            if (skill.Value.ProgressionClass.IsPerk)
                temp += " (Perk): ";
            if (skill.Value.ProgressionClass.IsAttribute)
                temp += " (Attribute): ";
            if (skill.Value.ProgressionClass.IsBook)
                temp += " (Book): ";

            description += $" {temp}: {skill.Value.Level}\n";
        }

        description += "Buffs:\n";
        foreach (var buff in entity.Buffs.ActiveBuffs)
            description += $" {Localization.Get(buff.BuffName)} : {buff.DurationInSeconds}s\n";

        description += "CVars:\n";
        foreach (var cvar in entity.Buffs.CVars)
            description += $" {Localization.Get(cvar.Key)} : {cvar.Value}\n";

        return description;

    }
    public override void Update(float _dt)
    {
        if (GameManager.Instance == null || GameManager.Instance.World == null) return;
        if (entity == null || !(entity is EntityAliveSDX))
        {
            lbldescriptionText.Text = "";
            OnClose();
            return;
        }
        if (isDirty)
        {
            lbldescriptionText.Text = GetDescription();
            isDirty = false;
            RefreshBindings();
        }

        base.Update(_dt);
    }

    public override void OnOpen()
    {
        isDirty = true;
        base.OnOpen();
        player = xui.playerUI.entityPlayer;
        var entityID = 0;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityID = (int)player.Buffs.GetCustomVar("CurrentNPC");

        // Do not display the extra information until they are hired.
        entity = player.world.GetEntity(entityID) as EntityAliveSDX;
        if (entity != null)
        {
            return;   
            //var leader = EntityUtilities.GetLeaderOrOwner(entityID);
            //if (leader && leader.entityId == player.entityId)
            //    return;
        }
        OnClose();
    }


    public override bool GetBindingValue(ref string value, string binding)
    {
        if (!entity)
            return false;

        var EntityID = entity.entityId;
        var fieldName = "";

        fieldName = binding;

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