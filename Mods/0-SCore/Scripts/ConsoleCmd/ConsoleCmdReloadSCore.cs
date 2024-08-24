
using System.Collections.Generic;
using UAI;


public class ConsoleCmdReloadBuffs : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    public override string[] getCommands()
    {
        return new string[]
        {
            "ReloadSCore"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        if (_params.Count != 1) return;
        WorldStaticData.Reset(_params[0]);
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reloading {_params[0]}");
    }

    public override string getDescription()
    {
        return "SCore: Reloads the passed in xml. This could have catastrophic results.";
    }
}