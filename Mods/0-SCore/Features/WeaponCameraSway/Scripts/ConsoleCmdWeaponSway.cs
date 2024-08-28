using System.Collections.Generic;

public class ConsoleCmdWeaponSway : ConsoleCmdAbstract {
    public override bool IsExecuteOnClient {
        get { return true; }
    }

    public override string[] getCommands() {
        return new string[] {
            "weaponsway"
        };
    }

    public override void Execute(List<string> @params, CommandSenderInfo senderInfo) {
        if (!senderInfo.IsLocalGame)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Command can only be used on clients.");
            return;
        }

        if (@params.Count == 1)
        {
            if (StringParsers.TryParseBool(@params[0], out var swayOn))
            {
                EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                if (primaryPlayer == null) return;
                primaryPlayer.Buffs.AddCustomVar("$WeaponSway", swayOn ? 1 : 0);
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Weapon Sway is {swayOn}");
                return;
            }
        }

        SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Syntax:  weaponsway <true/false>");
    }

    public override string getDescription() {
        return "SCore: Sets a CVar on the primary player";
    }
}