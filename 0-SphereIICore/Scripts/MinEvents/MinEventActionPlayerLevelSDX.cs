public class MinEventActionPlayerLevelSDX : MinEventActionRemoveBuff
{

    //  <triggered_effect trigger="onSelfBuffStart" action="PlayerLevelSDX, Mods" target="self"  /> 
    public override void Execute(MinEventParams _params)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            EntityPlayerLocal entity = targets[i] as EntityPlayerLocal;
            if (entity != null)
            {
                if (entity.Progression.Level < Progression.MaxLevel)
                {
                    entity.Progression.Level++;
                    GameManager.ShowTooltipWithAlert(entity, string.Format(Localization.Get("ttLevelUp"), entity.Progression.Level.ToString(), entity.Progression.SkillPoints), "levelupplayer");
                }
            }
        }
    }
}
