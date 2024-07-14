/*
 *      <requirement type="HasBuffSDX, SCore" requirementtype="Hide" value="buffCursed" />
 *      <requirement type="NotHasBuffSDX, SCore" requirementtype="Hide" value="buffCursed" /> 
 */

public class DialogRequirementHasBuffSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {

        return player.Buffs.HasBuff(Value);
    }
}

public class DialogRequirementNotHasBuffSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {

        return !player.Buffs.HasBuff(Value);
    }
}