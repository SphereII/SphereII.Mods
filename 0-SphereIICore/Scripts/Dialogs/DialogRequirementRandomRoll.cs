public class DialogRequirementRandomRoll : BaseDialogRequirement
{

    // Value being percent chance to show.
    //  <requirement type="RandomRoll, Mods" requirementtype="Hide" value="1" /> 

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        float.TryParse(Value, out float flValue);

        if (flValue > 10)
            flValue = 1f;
        return GameManager.Instance.World.RandomRange(0, 10) < flValue;

    }
}


