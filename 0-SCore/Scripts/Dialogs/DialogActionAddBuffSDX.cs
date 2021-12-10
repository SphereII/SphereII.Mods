public class DialogActionAddBuffSDX : DialogActionAddBuff
{
    public override void PerformAction(EntityPlayer player)
    {
        player.Buffs.AddBuff(base.ID, -1, true);
    }
}
