/*
 * Match at least one of these buffs, by default.
 *      <requirement type="HasBuffSDX, SCore" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" /> 

 * Do not have these buffs
 *      <requirement type="HasBuffSDX, SCore" requirementtype="Hide" value="!buffCursed" " /> 
 */

using System.Linq;

public class DialogRequirementHasBuffSDX : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var matches = -1;

        // If there's more than one buff listed, loop around, recording how many we match.
        string[] array = Value.Split(new char[]
        {
            ','
        });
        if (Value.Contains(","))
        {
            matches = array.Count(t => player.Buffs.HasBuff(t));
            if (matches > 0)
                return true;
            return false;
        }
        else
        {
            // Reverse condition on the buff.
            if (Value.StartsWith("!"))
            {
                var tempBuff = Value.Replace("!", "");
                if (!player.Buffs.HasBuff(tempBuff))
                    return true;
            }
            // If no operator, just check if we have it
            if (player.Buffs.HasBuff(Value))
                return true;

        }

        return false;

    }

}


