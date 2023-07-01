
using System.Collections.Generic;
using UAI;


public class ConsoleCmdActionFireClear : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "fireclear"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        FireManager.Instance.Reset();
    }

    protected override string getDescription()
    {
        return "SCore: Clears the Fire cache.";
    }
}