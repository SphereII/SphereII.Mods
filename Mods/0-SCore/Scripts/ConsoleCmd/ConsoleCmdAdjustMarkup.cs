
using System.Collections.Generic;
using UAI;


public class ConsoleCmdAdjustMarkup : ConsoleCmdAbstract {
    private float default_markup = 3f;
    private float default_markdown = 0.2f;
    
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    public override string[] getCommands()
    {
        return new string[]
        {
            "adjustmarkup"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var markup = StringParsers.ParseFloat(_params[0]);
        TraderInfo.BuyMarkup = markup;
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Trader Mark Up Set. Default Markup was {default_markup}");
    }

    public override string getDescription()
    {
        return "SCore: Reloads the Utility AI";
    }
}