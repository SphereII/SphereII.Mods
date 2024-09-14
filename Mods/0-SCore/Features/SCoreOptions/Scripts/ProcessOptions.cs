using UnityEngine;

public class ProcessSCoreOptions {
    public static void ProcessCVars(string cvar) {
        if (GameManager.isDedicated) return;

        if (string.IsNullOrEmpty(cvar)) return;
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        if (player == null) return;
        var cvarValue = player.Buffs.GetCustomVar(cvar);
        switch (cvar)
        {
            case "$SCoreUtils_MemoryBudget":
                if (cvarValue > 0)
                {
                    Log.Out("Setting Streaming Memory Budget to 0");
                    QualitySettings.streamingMipmapsMemoryBudget = 0;
                }
                break;
            case "$SCoreUtils_PPEnable":
                if (cvarValue > 0)
                {
                    Log.Out("Setting PP Enable to 0");
                    var command = new ConsoleCmdGfx();
                    command.SetPostProcessing("enable", 0, 0);
                }
                break;
        }
    }
}