using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml.Linq;

// A LogMessage that resolves cvar references in the message text.
//
// Vanilla's LogMessage writes the message verbatim, so a line written as
//     message="Lucky Looter - Attempting Looting XP (+@$lbd_xp_luckylooter_container_base)"
// reaches Player.log with the cvar NAME in it, not its value. That makes a debug
// line unable to answer the one question it is there for: how much XP was that.
//
// This action takes the same message attribute and expands every @cvar token
// against the target's cvars before logging, so the same line reads
//     Lucky Looter - Attempting Looting XP (+3)
//
// Because the token syntax matches what configs already write, swapping
//     action="LogMessage"  ->  action="LogMessageCVars, SCore"
// is enough; no message text has to change.
//
// <triggered_effect trigger="onSelfBuffStart" action="LogMessageCVars, SCore"
//                   message="XP @$myperk_lbd_xp of @$myperk_lbd_xptonext"/>
//
// Attributes:
//   message  the text to log. @$name, @_name, @.name and @name are expanded;
//            @@ emits a literal @.
//   format   optional numeric format for expanded values. Default "0.###",
//            which prints 1 rather than 1.0 and keeps three decimals when they
//            matter. Any standard .NET numeric format string works.
//   target   as any targeted effect. Defaults to self; the message is logged
//            once per target so cvars resolve against the right entity.
//
// A token naming a cvar the entity does not have renders as <unset:name> rather
// than 0, so a mistyped or never-initialised cvar is visible in the log instead
// of silently reading as zero the way the game itself treats it.
public class MinEventActionLogMessageCVars : MinEventActionTargetedBase
{
    // @@ first so it wins over the single-@ form. A cvar name may start with
    // $ (mod cvars), . (entity stats) or _ (built-ins), or be a bare word.
    private static readonly Regex TokenRx =
        new Regex(@"@@|@(?<name>[$._]?[A-Za-z0-9_]+)", RegexOptions.Compiled);

    private string _message = string.Empty;
    private string _format = "0.###";

    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(_message)) return;

        // Targeted, but a log line should still appear when no target resolved -
        // fall back to the acting entity so the message is never silently dropped.
        if (targets == null || targets.Count == 0)
        {
            Log.Out(Expand(_message, _params.Self));
            return;
        }

        for (var i = 0; i < targets.Count; i++)
        {
            Log.Out(Expand(_message, targets[i]));
        }
    }

    private string Expand(string text, EntityAlive entity)
    {
        if (text.IndexOf('@') < 0) return text;

        return TokenRx.Replace(text, match =>
        {
            if (match.Value == "@@") return "@";

            var name = match.Groups["name"].Value;
            if (entity?.Buffs == null) return $"<no entity:{name}>";
            if (!entity.Buffs.HasCustomVar(name)) return $"<unset:{name}>";

            return entity.Buffs.GetCustomVar(name)
                         .ToString(_format, CultureInfo.InvariantCulture);
        });
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var handled = base.ParseXmlAttribute(_attribute);
        if (handled) return true;

        switch (_attribute.Name.LocalName)
        {
            case "message":
                _message = _attribute.Value;
                return true;
            case "format":
                _format = _attribute.Value;
                return true;
        }

        return false;
    }
}
