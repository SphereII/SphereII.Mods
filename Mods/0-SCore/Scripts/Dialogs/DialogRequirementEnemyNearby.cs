using UAI;

public class DialogRequirementEnemyNearby : BaseDialogRequirement
{
    public override bool CheckRequirement(EntityPlayer player, EntityNPC talkingTo)
    {
        var distance = StringParsers.ParseFloat(ID);
        var result = SCoreUtils.IsEnemyNearby(talkingTo, distance);

        return Value.EqualsCaseInsensitive("not") ? !result : result;
    }
}