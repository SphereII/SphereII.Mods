using System.Xml;
using System.Xml.Linq;

// Authored by Soleil Plein, and InnocuousChaos
// Example: <triggered_effect trigger="onSelfBuffStart" action="ExecuteConsoleCommand, SCore" command="st night"/>
public class MinEventActionExecuteConsoleCommand : MinEventActionBase
{
    private string command;

    public override void Execute(MinEventParams _params)
    {
        if (command == null)
        {
        }
        else
        {
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            else
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command));
        }
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        var xmlAttribute = base.ParseXmlAttribute(_attribute);
        if (xmlAttribute || !(_attribute.Name.LocalName == "command"))
            return xmlAttribute;
        command = _attribute.Value;
        return true;
    }
}