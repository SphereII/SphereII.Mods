using UnityEngine;

/*
 * Match at least one of these buffs, by default.
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" /> 
 * Match at least 2
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" match="2" /> 
 * Match at least 1
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" match="1" /> 
 * Do not have these buffs
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" match="not" /> 
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed" match="not" /> 
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" match="none" /> 
 * Match All:
 *      <requirement type="HasBuffSDX, Mods" requirementtype="Hide" value="buffCursed,buffGodMode,buffImagination" match="all" /> 
 */
public class DialogRequirementHasBuffSDX : BaseDialogRequirement
{
    public string strMatch = "1";

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        int Matches = -1;
     
        // If there's more than one buff listed, loop around, recording how many we match.
        if(Value.Contains(","))
        {
            Matches = 0;
            string[] array = Value.Split(new char[]
            {
                ','
            });
            for(int i = 0; i < array.Length; i++)
            {
                if(player.Buffs.HasBuff(array[i]))
                    Matches++;
            }
        }
        else
        {
            // If it's a single buff, check our operator
            if(strMatch.ToLower() == "not")
                if(!player.Buffs.HasBuff(Value))
                    return true;

            // Reverse condition on the buff.
            if (Value.StartsWith("!"))
            {
                string tempBuff = Value.Replace("!", "");
                if (!player.Buffs.HasBuff(tempBuff))
                    return true;
            }
            // If no operator, just check if we have it
            if (player.Buffs.HasBuff(Value))
                return true;

        }

        if (strMatch == "1")
            return false;

        // Check to see if match is a special key word.
        switch(strMatch.ToLower())
        {
            case "not":  // Make sure we don't match any.
                if(Matches == 0)
                    return true;
                break;
            case "none":  // Make sure we don't match any.
                if(Matches == 0)
                    return true;
                break;

            case "all":   // make sure we match them all.
                if(Matches == Value.Split(new char[] { ',' }).Length)
                    return true;
                break;
        }

        // If it's a numeric value, make sure we match at least that many
        int Counter = 0;
        if(int.TryParse(strMatch, out Counter))
        {
            if(Counter >= Matches)
                return true;

        }
        
        return false;

    }

}


