
using System.Collections.Generic;
using UAI;
using UnityEngine;


public class ConsoleCmdDumpEntities : ConsoleCmdAbstract
{
    public override bool IsExecuteOnClient
    {
        get { return true; }
    }

    public override string[] GetCommands()
    {
        return new string[]
        {
            "DumpEntities"
        };
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo)
    {

        var males = "";
        var females = "";
        SingletonMonoBehaviour<SdtdConsole>.Instance.Output($"Dumping Entities...");
        foreach (var entity in EntityClass.list.Dict)
        {
            var entityClass = entity.Value;
            var name = entityClass.entityClassName;
            if (Localization.Get(name) != name) continue;
            if (entityClass.bHideInSpawnMenu) continue;
            if (entityClass.bIsMale)
                males += $"{entityClass.entityClassName},\n";
            else
                females += $"{entityClass.entityClassName},\n";
            
        }
        
        System.IO.File.WriteAllText(@"C:\Builds\unlocalized-males.txt", males);
        System.IO.File.WriteAllText(@"C:\Builds\unlocalized-females.txt", females);

        
    }

    public override string GetDescription()
    {
        return "SCore: Dumps all unlocalized entities";
    }
}