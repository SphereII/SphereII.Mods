using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class EAITaskPatches : IPatcherMod
{
   

    // Inorder to update the GetWalk Type, we'll need to mark the GetWalkType to be virtual, so we can over-ride it.
    public bool Patch(ModuleDefinition module)
    {

        //// Search for all the EAI method sin the C#, and toggle the targetEntity or entityTarget to public.         
        Console.WriteLine("== EntityAlilve Patcher Patcher===");
        foreach (TypeDefinition myClass in module.Types.Where(d => d.Name.StartsWith("EAI")))
        {
            var myField = myClass.Fields.FirstOrDefault(d => d.Name == "targetEntity");
            if (myField == null)
                myField = myClass.Fields.FirstOrDefault(d => d.Name == "entityTarget");

            if (myField != null)
                SetFieldToPublic(myField);

        }
        //// This adds a new Entity Alive SDX reference, that we can use and play around with.
        //var myEntity = module.Types.First(d => d.Name == "EntityAlive");
        //myEntity.Fields.Add(new FieldDefinition("otherEntitySDX", FieldAttributes.Public, myEntity));
        //var field = myEntity.Fields.First(d => d.Name == "entityThatKilledMe");
        //SetFieldToPublic(field);
        //var gm = module.Types.First(d => d.Name == "EAIApproachAndAttackTarget");
        //var method = gm.Methods.First(d => d.Name == "GetTargetXZDistanceSq");
        //SetMethodToPublic(method);

        //method = gm.Methods.First(d => d.Name == "GetMoveToLocation");
        //SetMethodToVirtual(method);
        //SetMethodToPublic(method);


        //gm = module.Types.First(d => d.Name == "EAIApproachSpot");
        //method = gm.Methods.First(d => d.Name == "updatePath");
        //SetMethodToPublic(method);
        //SetMethodToVirtual(method);


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
