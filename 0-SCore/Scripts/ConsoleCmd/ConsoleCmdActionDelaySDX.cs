
using System.Collections.Generic;
using UAI;


public class ConsoleCmdActionDelaySDX : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "actiondelay",
            "ad"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        UAIBase.ActionChoiceDelay = float.TryParse(_params[0], out var num) ? num : 0.2f;
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Action Delay is now {UAIBase.ActionChoiceDelay}");
    }

    protected override string getDescription()
    {
        return "SCore: Changes the time before a new action in utilityUAI. Default is 0.2";
    }
}