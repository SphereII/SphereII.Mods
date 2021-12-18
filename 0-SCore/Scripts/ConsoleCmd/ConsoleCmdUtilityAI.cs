
using System.Collections.Generic;
using UAI;


public class ConsoleCmdUtilityAI : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    public override string[] GetCommands()
    {
        return new string[]
        {
            "utilityai",
            "uai"
            
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        UAIBase.Reload();
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reloading Utility AI");
    }

    public override string GetDescription()
    {
        return "SCore: Reloads the Utility AI";
    }
}