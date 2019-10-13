using DMT;
using Harmony;
using System;
using System.IO;
using UnityEngine;
public class SphereII_Core
{
    public class SphereII_Core_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll();
        }
    }
}