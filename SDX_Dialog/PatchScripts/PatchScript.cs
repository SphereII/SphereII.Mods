using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class SDXDialog : IPatcherMod
{
   

    // Inorder to update the GetWalk Type, we'll need to mark the GetWalkType to be virtual, so we can over-ride it.
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("== SDX Dialog Patcher Patcher===");

        var gm = module.Types.First(d => d.Name == "QuestList");
        foreach (var method in gm.Methods)
            SetMethodToPublic(method);


        // Dialog has an EntityNPC value for the respondant, and that's failing on some checks. We'll add a new entity that is more general.
        gm = module.Types.First(d => d.Name == "XUiM_Dialog");
        var myEntity = module.Types.First(d => d.Name == "EntityAlive");
        gm.Fields.Add(new FieldDefinition("otherEntitySDX", FieldAttributes.Public, myEntity));
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
