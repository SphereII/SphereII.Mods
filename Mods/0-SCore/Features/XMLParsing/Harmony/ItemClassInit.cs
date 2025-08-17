using System.Collections.Generic;
using HarmonyLib;

namespace SCore.Features.XMLParsing.Harmony
{
    [HarmonyPatch(typeof(ItemClass))]
    [HarmonyPatch(nameof(ItemClass.Init))]
    public class ItemClassInit
    {
        public static bool Prefix(ItemClass __instance)
        {
            if (!__instance.Properties.Classes.ContainsKey("Import")) return true;
            var dynamicProperties3 = __instance.Properties.Classes["Import"];

            HashSet<string> hashSet = new HashSet<string> { Block.PropCreativeMode };
            hashSet.Add("Extends");
            
            foreach (var properties in dynamicProperties3.Values.Dict)
            {
                if (properties.Value != "Block") continue;
                var block = Block.GetBlockByName(properties.Key, true);
                
                // Create a new dynamic properties that uses both the blocks and current Item properties.
                var dynamicProperties = new DynamicProperties();
                // Copy all the properties from the block.
                dynamicProperties.CopyFrom(block.Properties, hashSet);
                
                // copy, and over-ride anything from the current item.
                dynamicProperties.CopyFrom(__instance.Properties, null);
                __instance.Properties = dynamicProperties;
            }

            return true;
        }
    }
}