using System.Reflection;
using UnityEngine;

namespace LargerParties.PlayerParty {
    public class LargerParties : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());
            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

    }

}