public class DialogActionAddCVarSelf : DialogActionAddBuff
{
    public string strOperator = "add";

    public override void PerformAction(EntityPlayer player)
    {
        if (string.IsNullOrEmpty(Value))
            Value = "1";


        int.TryParse(Value, out var flValue);

        var entityId = -1;
        if (player.Buffs.HasCustomVar("CurrentNPC"))
            entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

        if (entityId == -1)
            return;

        var entityNPC = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
        if (entityNPC == null) return;

        var currentValue = entityNPC.Buffs.GetCustomVar(ID);
        switch (strOperator.ToLower())
        {
            case "add":
                currentValue += flValue;
                break;
            case "sub":
                currentValue -= flValue;
                break;
            // default is set
            default:
                currentValue = flValue;
                break;
        }

        if (currentValue == 0)
            entityNPC.Buffs.RemoveCustomVar(ID);
        else
            entityNPC.Buffs.SetCustomVar(ID, currentValue, true);

        
        return;
    }
}
