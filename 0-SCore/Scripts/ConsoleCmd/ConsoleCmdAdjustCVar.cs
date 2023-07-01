using System.Collections.Generic;

public class ConsoleCmdAdjustCVar : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    protected override string[] getCommands()
    {
        return new string[]
        {
            "setcvar"
        };
    }

    public override void Execute(List<string> @params, CommandSenderInfo senderInfo)
    {
        if (!senderInfo.IsLocalGame)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients.");
            return;
        }
        if (@params.Count == 2)
        {
            var cvarName = @params[0];
            var cvarValue = StringParsers.ParseFloat(@params[1]);
            
            EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
            if (primaryPlayer == null) return;

            if (!primaryPlayer.IsAdmin) return;
            
            if (cvarValue == 0)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Removing CVar {cvarName}");
                primaryPlayer.Buffs.RemoveCustomVar(cvarName);
            }
            else
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Adding CVar {cvarName} Value: {cvarValue}");
                primaryPlayer.Buffs.AddCustomVar(cvarName, cvarValue);
            }
        }
        else
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Syntax:  setcvar <cvarName> <cvarValue>");
        }
    }

    protected override string getDescription()
    {
        return "SCore: Sets a CVar on the primary player";
    }
}