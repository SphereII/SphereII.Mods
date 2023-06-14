
using System.Collections.Generic;
using UAI;


public class ConsoleCmdUtilityAI : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "utilityai",
            "uai"

        };
    }


    protected override string getDescription()
    {
        return "SCore: Reloads the Utility AI";
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        UAIBase.Reload();
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reloading Utility AI");
    }

}