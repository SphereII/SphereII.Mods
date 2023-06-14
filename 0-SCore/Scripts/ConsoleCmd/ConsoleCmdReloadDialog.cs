
using System.Collections.Generic;
using UAI;


public class ConsoleCmdReloadDialog : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "dialog",
            "dialogs"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        WorldStaticData.Reset("dialogs");
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Reloading dialogs.");
    }

    protected override string getDescription()
    {
        return "SCore: Reloads the dialog xml";
    }
}