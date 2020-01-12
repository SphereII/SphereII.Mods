using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class FoodSpoilagePatch : IPatcherMod
{
    public bool Patch(ModuleDefinition module)
    {
        Console.WriteLine("== FoodSpoilagePatch Patcher===");

        var gm = module.Types.First(d => d.Name == "ItemValue");
        var myTypeRef = gm.Fields.First(d => d.Name == "UseTimes");

            gm.Fields.Add(new FieldDefinition("NextSpoilageTick", FieldAttributes.Public, myTypeRef.FieldType));
            gm.Fields.Add(new FieldDefinition("CurrentSpoilage", FieldAttributes.Public, myTypeRef.FieldType));

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
