// using HarmonyLib;
//
// namespace SCore.Harmony.WorldGen
// {
//     [HarmonyPatch(typeof(PoiMapElement))]
//     [HarmonyPatch(MethodType.Constructor)]
//     [HarmonyPatch(new[] { typeof(uint),typeof(string),typeof(BlockValue),typeof(BlockValue),typeof(int),typeof(int),typeof(int),typeof(int) })]
//     public class PoiMapElementConstructor
//     {
//         public static void Postfix(PoiMapElement __instance, string _name)
//         {
//             // When the name is empty, it's placing blocks into the world. 
//             // If it has a name, it could be coming from the Biomes' POI placement, like gravel roads, etc.
//             if (string.IsNullOrEmpty(_name))
//             {
//                 var block = Block.GetBlockValue("terrBedrock");
//                 __instance.m_BlockBelow = block;
//             }
//         }
//     
//     }
// }
