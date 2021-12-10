public class DialogRequirementRandomRollSDX : BaseDialogRequirement
{
    // Value being percent chance to show.
    //  <requirement type="RandomRollSDX, SCore" requirementtype="Hide" value="1" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        float.TryParse(Value, out var flValue);

        if (flValue > 10)
            flValue = 1f;
        return GameManager.Instance.World.RandomRange(0, 10) < flValue;
    }
}