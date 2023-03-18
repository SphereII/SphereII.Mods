using Platform;
using System.Threading.Tasks;
using System.Xml;
using SCore.Harmony.PlayerFeatures;
using UnityEngine;
public class MinEventActionRecalculateEncumbrance : MinEventActionRemoveBuff
{

    public override void Execute(MinEventParams _params)
    {
        var player = _params.Self as EntityPlayer;
        if (player == null) return;

        Encumbrance.EntityPlayerLocalInit.CheckEncumbrance();

    }
}