using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;

[HarmonyPatch(typeof(XUiC_CollectedItemList))]
[HarmonyPatch("AddIconNotification")]
public class SphereII_XPIconRemover : IHarmony
{
    public void Start()
    {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    static bool Prefix()
    {
        return false;
    }
}

