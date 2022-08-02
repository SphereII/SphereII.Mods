
using System.Collections.Generic;
using UAI;


public class ConsoleCmdActionFireClear : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    public override string[] GetCommands()
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

    public override string GetDescription()
    {
        return "SCore: Clears the Fire cache.";
    }
}