public class DialogActionRemoveBuffSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        player.Buffs.RemoveBuff(base.ID, true);
    }
}
