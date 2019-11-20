using UnityEngine;
public class DialogRequirementHasBuffSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player)
    {
        if (Value.Contains(","))
        {
            string[] array = Value.Split(new char[]
            {
                ','
            });
            for (int i = 0; i < array.Length; i++)
            {
                if (player.Buffs.HasBuff(array[i]))
                    return true;
            }
            return false;
        }
        if (player.Buffs.HasBuff(Value))
            return true;

        return false;

    }

}


