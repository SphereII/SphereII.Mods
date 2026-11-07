using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Lists and removes POI outposts (see POIOutpostManager). Runs on the server.
/// <code>
/// outposts list
/// outposts remove &lt;number from the list&gt;
/// </code>
/// </summary>
public class ConsoleCmdPOIOutposts : ConsoleCmdAbstract
{
    public override string[] getCommands() => new[] { "outposts" };

    public override string getDescription() => "SCore: list or remove POI outposts made by ConvertPOISDX.";

    public override string getHelp() =>
        "Usage:\n" +
        "  outposts list         - list the outposts in this world\n" +
        "  outposts remove <n>   - forget outpost n from the list. Quests can pick its POI again, and the\n" +
        "                          next quest that resets it rebuilds the original. Its settler stays.\n";

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        var console = SingletonMonoBehaviour<SdtdConsole>.Instance;
        var outposts = POIOutpostManager.All.ToList();

        if (_params.Count == 0 || _params[0].EqualsCaseInsensitive("list"))
        {
            if (outposts.Count == 0)
            {
                console.Output("No outposts in this world.");
                return;
            }

            for (var i = 0; i < outposts.Count; i++)
            {
                var o = outposts[i];
                var state = o.Built ? "built" : "waiting for the rebuild";
                var settler = string.IsNullOrEmpty(o.Settler) ? "no settler" : $"settler {o.Settler} ({(o.SettlerSpawned ? $"entity {o.SettlerId}" : "not spawned yet")})";
                console.Output($"{i + 1}. {o.PoiName}{o.Suffix} at {o.Position}: {state}, {settler}, from quest {o.QuestId}");
            }
            return;
        }

        if (_params[0].EqualsCaseInsensitive("remove") && _params.Count > 1
            && int.TryParse(_params[1], out var index) && index >= 1 && index <= outposts.Count)
        {
            var o = outposts[index - 1];
            POIOutpostManager.Remove(o.Position);
            console.Output($"Removed outpost {o.PoiName}{o.Suffix} at {o.Position}.");
            return;
        }

        console.Output(getHelp());
    }
}
