public class MinEventActionPlayerLevelSDX : MinEventActionRemoveBuff
{
    //  <triggered_effect trigger="onSelfBuffStart" action="PlayerLevelSDX, SCore" target="self"  /> 
    public override void Execute(MinEventParams _params)
    {
        for (var i = 0; i < targets.Count; i++)
        {
            var entity = targets[i] as EntityPlayerLocal;
            if (entity != null)
                if (entity.Progression.Level < Progression.MaxLevel)
                {
                    entity.Progression.Level++;
                    GameManager.ShowTooltip(entity, string.Format(Localization.Get("ttLevelUp"), entity.Progression.Level.ToString(), entity.Progression.SkillPoints), "levelupplayer");
                }
        }
    }
}