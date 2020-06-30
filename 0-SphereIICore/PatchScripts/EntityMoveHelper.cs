using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class EntityMoveHelperPatches : IPatcherMod
{
   

    // Inorder to update the GetWalk Type, we'll need to mark the GetWalkType to be virtual, so we can over-ride it.
    public bool Patch(ModuleDefinition module)
    {

        // Search for all the EAI method sin the C#, and toggle the targetEntity or entityTarget to public.         
        Console.WriteLine("== EntityMoveHelper Patcher Patcher===");
    
        var gm = module.Types.First(d => d.Name == "EntityMoveHelper");

        var method = gm.Methods.First(d => d.Name == "CalcMoveDist");
        SetMethodToPublic(method);

        method = gm.Methods.First(d => d.Name == "CalcTempMoveDist");
        SetMethodToPublic(method);

        method = gm.Methods.First(d => d.Name == "ResetStuckCheck");
        SetMethodToPublic(method);

        foreach (var myField in gm.Fields)
        {
            SetFieldToPublic(myField);
        }
      

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
