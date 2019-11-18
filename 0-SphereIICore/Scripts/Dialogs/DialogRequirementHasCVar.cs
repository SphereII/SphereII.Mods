using UnityEngine;
public class DialogRequirementHasCVarSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player)
    {
        return player.Buffs.HasCustomVar(base.Value);
    }
}


