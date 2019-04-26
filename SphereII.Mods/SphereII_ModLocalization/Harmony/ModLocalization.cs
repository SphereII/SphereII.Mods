using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System.Collections.Generic;

[HarmonyPatch(typeof(Localization))]
public class SphereII_ModLocalization : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    
    //   var gm = module.Types.First(d => d.Name == "Localization");
    //var field = gm.Fields.First(d => d.Name == "mDictionary");
    //SetFieldToPublic(field);
}