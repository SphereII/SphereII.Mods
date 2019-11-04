using System;
using SDX.Compiler;
using Mono.Cecil;
using Mono.Cecil.Cil;
using System.Linq;
using System.Collections.Generic;

public class SDXTileEntityPatch : IPatcherMod
{
    public bool Patch(ModuleDefinition module)
    {

        AddEnumOption(module, "TileEntityType", "TileEntityType", "PoweredWorkstationSDX", 100);
        return true;
    }

    private void AddEnumOption(ModuleDefinition gameModule, string className, string enumName, string enumFieldName, byte enumValue)
    {
        var enumType = gameModule.Types.First(d => d.Name == enumName);
        FieldDefinition literal = new FieldDefinition(enumFieldName, FieldAttributes.Public | FieldAttributes.Static | FieldAttributes.Literal | FieldAttributes.HasDefault, enumType);
        enumType.Fields.Add(literal);
        literal.Constant = enumValue;
    }

    public bool Link(ModuleDefinition gameModule, ModuleDefinition modModule)
    {
        return true;
    }
}
