
using System.Collections.Generic;
using UAI;


public class ConsoleCmdlock : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "lock"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count != 2)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments, requires break time and max give ");
            return;
        }
        var breakTime = StringParsers.ParseFloat(_params[0]);
        var maxGive = StringParsers.ParseFloat(_params[1]);

        var player = GameManager.Instance.World.GetPrimaryPlayer();
        if ( player != null )
        {
            player.Buffs.SetCustomVar("BreakTime", breakTime);
            player.Buffs.SetCustomVar("MaxGive", maxGive);
        }
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Lock Testing");
    }

    protected override string getDescription()
    {
        return "SCore: Reloads the Utility AI";
    }
}