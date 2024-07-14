public class DialogRequirementHasPlayerLevelSDX : BaseDialogRequirement
{
    private static readonly string AdvFeatureClass = "AdvancedDialogDebugging";
    public virtual bool CheckValue(float playerValue, float value)
    {
        return playerValue == value;
    }

    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        AdvLogging.DisplayLog(AdvFeatureClass, "Player: " + player.EntityName);
        foreach (var temp in player.Buffs.CVars) AdvLogging.DisplayLog(AdvFeatureClass, "\tLevel: " + temp.Key + " Value: " + temp.Value);

        if (player.Progression.Level <= Progression.MaxLevel)
        {
            // If value is not specified, accepted it.
            if (string.IsNullOrEmpty(Value))
                return true;

            var flValue = 0f;
            float.TryParse(Value, out flValue);
            var flPlayerValue = player.Buffs.GetCustomVar(ID);
            return CheckValue(flPlayerValue, flValue);
        }

        return false;
    }
}

public class DialogRequirementHasPlayerLevelGTSDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return playerValue > value;
        }
    }

    public class DialogRequirementHasPlayerLevelGTESDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return playerValue >= value;
        }
    }

    public class DialogRequirementHasPlayerLevelLESDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return playerValue < value;
        }
    }

    public class DialogRequirementHasPlayerLevelLTESDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return playerValue <= value;
        }
    }

    public class DialogRequirementNotHasPlayerLevelSDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return !base.CheckValue(playerValue, value);
        }
    }

    public class DialogRequirementHasPlayerLevelNotLESDX : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return !base.CheckValue(playerValue, value);
        }
    }

    public class DialogRequirementHasPlayerLevelNotCVarLTE : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return !base.CheckValue(playerValue, value);
        }
    }

    public class DialogRequirementHasPlayerLevelNotCVarGT : DialogRequirementHasPlayerLevelSDX
    {
        public override bool CheckValue(float playerValue, float value)
        {
            return !base.CheckValue(playerValue, value);
        }
    }