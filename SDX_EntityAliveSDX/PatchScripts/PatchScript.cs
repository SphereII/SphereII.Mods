using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class EntityAliveSDXPatcher : IPatcherMod
{


    // Inorder to update the GetWalk Type, we'll need to mark the GetWalkType to be virtual, so we can over-ride it.
    public bool Patch(ModuleDefinition module)
    {

        // Search for all the EAI method sin the C#, and toggle the targetEntity or entityTarget to public.         
        Console.WriteLine("== EntityAlilve Patcher Patcher===");

        var gm = module.Types.First(d => d.Name == "EntityStats");
        var method = gm.Methods.First(d => d.Name == "UpdateWeatherStats");
        SetMethodToPublic(method);

        gm = module.Types.First(d => d.Name == "EntityAlive");
        method = gm.Methods.First(d => d.Name == "Attack");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);

        method = gm.Methods.First(d => d.Name == "SetAttackTarget");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);
        method = gm.Methods.First(d => d.Name == "SetRevengeTarget");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);

        gm = module.Types.First(d => d.Name == "QuestClass");
        method = gm.Methods.First(d => d.Name == "GetQuest");
        SetMethodToPublic(method);

        gm = module.Types.First(d => d.Name == "PrefabLODManager");
        var field = gm.Fields.First(d => d.Name == "prefabsAroundNear");
        SetFieldToPublic(field);


        gm = module.Types.First(d => d.Name == "GameManager");
        method = gm.Methods.First(d => d.Name == "lootContainerOpened");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);

        gm = module.Types.First(d => d.Name == "EntityMoveHelper");
        method = gm.Methods.First(d => d.Name == "CheckEntityBlocked");
        SetMethodToPublic(method);
        SetMethodToVirtual(method);
        return true;

    }


    // Called after the patching process and after scripts are compiled.
    // Used to link references between both assemblies
    // Return true if successful
    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }


    // Helper functions to allow us to access and change variables that are otherwise unavailable.
    private void SetMethodToVirtual(MethodDefinition meth)
    {
        meth.IsVirtual = true;
    }

    private void SetFieldToPublic(FieldDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
    private void SetMethodToPublic(MethodDefinition field)
    {
        field.IsFamily = false;
        field.IsPrivate = false;
        field.IsPublic = true;

    }
}
