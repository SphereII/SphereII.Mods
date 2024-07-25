using HarmonyLib;
using System.Reflection;

public class DediPatches
{
    public class DediPatchesInit : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());
            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }



    [HarmonyPatch(typeof(BlockShapeModelEntity))]
    [HarmonyPatch("Init")]
    public class SphereII_BlockShapeModelEntity
    {
        private static bool Prefix(ref Block _block)
        {
            if (!GameManager.IsDedicatedServer)
                return true;

            var model = _block.Properties.Values["Model"];
            if (model == null)
                return true;

            if (model.Contains("modfolder"))
            {
                Log.Out($"Converting {model} to Placeholder.");
                _block.Properties.Values["Model"] = "Entities/Misc/block_missingPrefab";
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(EntityClass))]
    [HarmonyPatch("Init")]
    public class SphereII_EntityClass_Init
    {
        private static bool Prefix(ref EntityClass __instance)
        {

            if (!GameManager.IsDedicatedServer)
                return true;



            // if (__instance.Properties.Values.ContainsKey(EntityClass.PropMaterialSwap0))
            //     __instance.Properties.Values.Remove(EntityClass.PropMaterialSwap0);
            //
            // if (__instance.Properties.Values.ContainsKey(EntityClass.PropMaterialSwap1))
            //     __instance.Properties.Values.Remove(EntityClass.PropMaterialSwap1);
            //
            // if (__instance.Properties.Values.ContainsKey(EntityClass.PropMaterialSwap2))
            //     __instance.Properties.Values.Remove(EntityClass.PropMaterialSwap2);
            //
            // if (__instance.Properties.Values.ContainsKey(EntityClass.PropMaterialSwap3))
            //     __instance.Properties.Values.Remove(EntityClass.PropMaterialSwap3);
            //
            // if (__instance.Properties.Values.ContainsKey(EntityClass.PropMaterialSwap4))
            //     __instance.Properties.Values.Remove(EntityClass.PropMaterialSwap4);

            return true;

        }
    }
}