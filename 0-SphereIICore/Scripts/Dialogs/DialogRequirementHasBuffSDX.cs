using UnityEngine;
public class DialogRequirementHasBuffSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player)
    {
        Debug.Log("Checking Buff: " + base.Value);
        return player.Buffs.HasBuff(base.Value);
    }
}


