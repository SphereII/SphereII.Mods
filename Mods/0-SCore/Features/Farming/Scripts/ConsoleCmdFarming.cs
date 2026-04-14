using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Console command for inspecting and validating the SCore farming managers.
///
/// Commands:
///   scf count              — print registered counts for farm plots and crop plants
///   scf validate           — remove stale entries from both managers and report
///   scf listplots [range]  — list all registered farm plot positions (optional: within range of player)
///   scf listcrops [range]  — list all registered crop plant positions (optional: within range of player)
/// </summary>
public class ConsoleCmdFarming : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient => true;

    public override string[] getCommands() => new[] { "scorefarming", "scf" };

    public override string getDescription() => "SCore: Inspect and validate farming plot/crop registries.";

    public override string getHelp() =>
        "Usage:\n" +
        "  scf count              — show registered farm plot and crop counts\n" +
        "  scf validate           — remove stale entries from both managers\n" +
        "  scf listplots [range]  — list all registered farm plot positions\n" +
        "  scf listcrops [range]  — list all registered crop plant positions\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var console = SingletonMonoBehaviour<SdtdConsole>.Instance;

        if (_params.Count == 0)
        {
            Output(console, getHelp());
            return;
        }

        switch (_params[0].ToLower())
        {
            case "count":
                ExecuteCount(console);
                break;

            case "validate":
                ExecuteValidate(console);
                break;

            case "listplots":
                ExecuteListPlots(console, _params);
                break;

            case "listcrops":
                ExecuteListCrops(console, _params);
                break;

            default:
                Output(console, $"Unknown sub-command '{_params[0]}'. Run 'scf' for help.");
                break;
        }
    }

    // -----------------------------------------------------------------------

    private static void ExecuteCount(SdtdConsole console)
    {
        var plots = FarmPlotManager.Instance.GetAll();
        var crops = CropManager.Instance.GetAll();
        Output(console, $"Farm plots registered : {plots.Count}");
        Output(console, $"Crop plants registered: {crops.Count}");
    }

    private static void ExecuteValidate(SdtdConsole console)
    {
        int removedPlots = FarmPlotManager.Instance.Validate();
        int removedCrops = CropManager.Instance.Validate();
        Output(console, $"Validation complete.");
        Output(console, $"  Stale farm plots removed : {removedPlots}");
        Output(console, $"  Stale crop plants removed: {removedCrops}");

        // Print updated counts
        Output(console, $"  Farm plots remaining : {FarmPlotManager.Instance.GetAll().Count}");
        Output(console, $"  Crop plants remaining: {CropManager.Instance.GetAll().Count}");
    }

    private static void ExecuteListPlots(SdtdConsole console, List<string> _params)
    {
        float range = ParseRange(_params, console);
        var playerPos = GetPlayerPos();
        var plots = FarmPlotManager.Instance.GetAll();
        var world = GameManager.Instance.World;

        int shown = 0;
        foreach (var plot in plots)
        {
            var pos = plot.GetBlockPos();
            if (range > 0f && Vector3.Distance(playerPos, pos) > range) continue;

            bool hasWater = plot.HasWater();
            bool hasPlant = plot.HasPlant();
            bool isEmpty  = plot.IsEmpty();
            bool isDead   = plot.IsDeadPlant();
            bool visited  = plot.Visited;

            // Collect crop block names stacked above this plot (up to 4 blocks up).
            var cropBlocks = new System.Text.StringBuilder();
            for (int dy = 1; dy <= 4; dy++)
            {
                var above = world.GetBlock(pos + new Vector3i(0, dy, 0));
                if (above.isair) break;
                if (dy > 1) cropBlocks.Append(", ");
                cropBlocks.Append($"Y+{dy}={above.Block.GetBlockName()}");
            }
            string cropInfo = cropBlocks.Length > 0 ? $"  crops=[{cropBlocks}]" : string.Empty;

            Output(console,
                $"  {pos}  water={hasWater}  plant={hasPlant}  empty={isEmpty}  dead={isDead}  visited={visited}{cropInfo}");
            shown++;
        }

        Output(console, range > 0f
            ? $"Listed {shown} of {plots.Count} farm plots within {range} blocks."
            : $"Listed {shown} farm plots total.");
    }

    private static void ExecuteListCrops(SdtdConsole console, List<string> _params)
    {
        float range = ParseRange(_params, console);
        var playerPos = GetPlayerPos();
        var crops = CropManager.Instance.GetAll();
        var world = GameManager.Instance.World;

        int shown = 0;
        foreach (var pos in crops)
        {
            if (range > 0f && Vector3.Distance(playerPos, pos) > range) continue;

            var block = world.GetBlock(pos);
            string blockName = block.isair ? "(air — stale)" : block.Block.GetBlockName();

            // Find the farm plot block below this crop (search down up to 4 blocks).
            string plotInfo = string.Empty;
            for (int dy = 1; dy <= 4; dy++)
            {
                var below = world.GetBlock(pos - new Vector3i(0, dy, 0));
                if (below.Block is BlockFarmPlotSDX)
                {
                    plotInfo = $"  plot={pos - new Vector3i(0, dy, 0)} (Y-{dy})";
                    break;
                }
                if (!below.isair) break; // hit a non-plot, non-air block — stop searching
            }

            Output(console, $"  {pos}  block={blockName}{plotInfo}");
            shown++;
        }

        Output(console, range > 0f
            ? $"Listed {shown} of {crops.Count} crop plants within {range} blocks."
            : $"Listed {shown} crop plants total.");
    }

    // -----------------------------------------------------------------------

    private static void Output(SdtdConsole console, string msg)
    {
        console.Output(msg);
        Log.Out($"[ConsoleCmdFarming] {msg}");
    }

    private static float ParseRange(List<string> _params, SdtdConsole console)
    {
        if (_params.Count < 2) return 0f;
        if (float.TryParse(_params[1], out float r)) return r;
        Output(console, $"Invalid range '{_params[1]}', showing all.");
        return 0f;
    }

    private static Vector3 GetPlayerPos()
    {
        var player = GameManager.Instance.World?.GetPrimaryPlayer();
        return player != null ? player.position : Vector3.zero;
    }
}
