
using System;
using System.Collections.Generic;
using System.Reflection;
using UAI;


public class ConsoleCmdUnitTestSCore : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    //public override string[] GetCommands()
    protected override string[] getCommands()
    {
        return new string[]
        {
            "unittest"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {
        if (_params.Count != 3)
        {
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments. Command syntax:  unittest <Script> sourceEntityID targetEntityID ");
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments. Command syntax:  unittest CanDamageChecks npcHarleyBat zombieBoe ");
            SingletonMonoBehaviour<SdtdConsole>.Instance.Output("Invalid arguments. Command syntax:  unittest CanDamageChecks,SCore npcHarleyBat zombieBoe ");
            return;
        }

        var className = _params[0];
        if (!className.Contains(","))
            className += ",SCore";

        Type type = Type.GetType(className);
        if (type == null)
        {
            Log.Out($"Unit Test Failed: {className} does not exist.");
            return;
        }

        var instance = Activator.CreateInstance(type);
        if (instance == null)
        {
            Log.Out($"Unit Test Failed: {className} could not be created.");
            return;
        }

        MethodInfo method = type.GetMethod("Start");
        if (method == null)
        {
            Log.Out($"Unit Test Failed: {className} does not have a Start method.");
            return;
        }

        method.Invoke(instance, new object[] { _params[1], _params[2] });
        return;
    }

    protected override string getDescription()
    {
        return "Runs a test script.";
    }
}