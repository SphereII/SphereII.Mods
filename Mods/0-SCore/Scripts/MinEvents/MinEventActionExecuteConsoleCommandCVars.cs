using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using HarmonyLib;

// <triggered_effect trigger = "onSelfBuffStart" action = "ExecuteConsoleCommandCVars, SCore" command = "testCommand {0} {1}" cvars = "cvar1,cvar2" />

public class MinEventActionExecuteConsoleCommandCVars : MinEventActionTargetedBase
{
    private string command = string.Empty;
    private List<string> cvars;
    private List<float> cvarsValues;

    public override void Execute(MinEventParams _params)
    {
        if (string.IsNullOrEmpty(command))
            return;


        foreach (var t in targets)
        {
            if (t == null) continue;
            var entity = t;
            cvarsValues = new List<float>();
            cvarsValues.Clear();
            foreach (var cvar in cvars)
            {
                var  value = entity.Buffs.CVars.GetValueSafe(cvar);
                cvarsValues.Add(value);
            }

            command = AssembleString(command, cvarsValues);
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsClient)
            {
                SingletonMonoBehaviour<SdtdConsole>.Instance.ExecuteSync(command, null);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                    NetPackageManager.GetPackage<NetPackageConsoleCmdServer>().Setup(command), false);
            }
        }
    }

    private string AssembleString(string _command, List<float> _cvars)
    {
        string str = _command;
        for (int index = 0; index < _cvars.Count; index++)
        {
            str = str.Replace("{" + index + "}", _cvars[index].ToString());
        }

        return str;
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            var name = _attribute.Name.LocalName;
            if (name != null)
            {
                if (name == "command")
                {
                    command = _attribute.Value;
                    return true;
                }

                if (name == "cvars")
                {
                    cvars = _attribute.Value.Split(',').ToList();
                    return true;
                }
            }
        }

        return flag;
    }
}