using System.Collections.Generic;

public class ConsoleCmdWeaponSway : ConsoleCmdAbstract {
    private const string SwayCVar = "$WeaponSway";

    public override bool IsExecuteOnClient {
        get { return true; }
    }

    public override string[] getCommands() {
        return new string[] {
            "weaponsway"
        };
    }

    public override void Execute(List<string> @params, CommandSenderInfo senderInfo) {
        // IsLocalGame is set from (_cInfo == null), so it is false only when the command
        // arrived over the wire - telnet, the web console, or a remote client on a dedi.
        // Typing it into your own console counts as local even while connected to a server.
        if (!senderInfo.IsLocalGame)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output(
                "weaponsway has to be typed into the console of the machine it should affect, since it only changes what that client draws.");
            return;
        }

        if (@params.Count == 1)
        {
            // The parameter says whether to SUPPRESS sway, not whether to have it, so
            // "weaponsway true" is the one that turns the motion off. The cvar written
            // here carries that same sense and SwayUtilities.CanSway reads it that way.
            if (StringParsers.TryParseBool(@params[0], out var suppressSway))
            {
                EntityPlayer primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
                if (primaryPlayer == null)
                {
                    SingletonMonoBehaviour<SdtdConsole>.Instance.Output(
                        "No local player to apply this to. Weapon sway is a client side setting and has nothing to act on here.");
                    return;
                }

                primaryPlayer.Buffs.AddCustomVar(SwayCVar, suppressSway ? 1 : 0);
                SingletonMonoBehaviour<SdtdConsole>.Instance.Output(
                    suppressSway
                        ? "Weapon and camera sway is now OFF."
                        : "Weapon and camera sway is now ON.");
                return;
            }
        }

        SingletonMonoBehaviour<SdtdConsole>.Instance.Output(getHelp());
    }

    public override string getDescription() {
        return "SCore: Turns first person weapon and camera sway off or on for this client";
    }

    public override string getHelp() {
        return "Turns first person weapon and camera sway off or on for this client.\n"
               + "Mind the sense of the parameter: it says whether to SUPPRESS the motion.\n"
               + "\n"
               + "Usage:\n"
               + "  weaponsway true    sway OFF\n"
               + "  weaponsway false   sway ON\n"
               + "\n"
               + "Applies only to the player at this machine and is not sent to anyone else.\n"
               + "Sway ON is stored as a zero valued cvar, and the game does not write those\n"
               + "into the save, so it reverts to the default when you reload.";
    }
}
