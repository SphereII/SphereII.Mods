public class DialogActionAddBuffSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        if (!string.IsNullOrEmpty(Value))
        {
            var entityId = -1;
            if (player.Buffs.HasCustomVar("CurrentNPC"))
                entityId = (int)player.Buffs.GetCustomVar("CurrentNPC");

            if (entityId == -1)
                return;

            var entityNPC = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
            if (entityNPC == null) return;

            entityNPC.Buffs.AddBuff(ID);
            return;
        }
        player.Buffs.AddBuff(ID);
    }
}
