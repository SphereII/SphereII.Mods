using System.Collections.Generic;

public class ConsoleCmdSetFogPreset : ConsoleCmdAbstract
{
    protected override string[] getCommands()
    {
        return new string[]
        {
            "setfogpreset",
            "setfog"
        };
    }	
    
    protected override string getHelp()
    {
        return "setfog <clear|mist|windymist|lowclouds|seaclouds|groundgfog|frostedground|foggylake|fog|heavyfog|sandstorm1|smoke|toxicswamp|sandstorm2|worldedge> <true/false>";
    }

    protected override string getDescription()
    {
        return "Sets the fog preset";

    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (GameManager.IsDedicatedServer) return;
        var preset = "clear";
        var volume = false;
        switch (_params.Count)
        {
            case 0:
                Log.Out(getHelp());
                return;
            case 1:
                preset = _params[0];
                if (preset.ToLower() == "help")
                {
                    Log.Out(getHelp());
                    return;
                }
                break;
            case 2:
                preset = _params[0];
                volume = true;
                break;
        }

        FogUtils.SetFogPreset(preset, volume);
    }
}