public class MinEventActionAddBuffToPrimaryPlayer : MinEventActionAddBuff
{
    public override void Execute(MinEventParams _params)
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            return;
        EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if (primaryPlayer == null) return;

        foreach (var name in this.buffNames)
        {
            primaryPlayer.Buffs.AddBuff(name);
        }
    }
}