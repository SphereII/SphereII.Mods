using System.Reflection;

namespace CompoPackTweaks.Harmony {
        public class CompoPackTweaksInit : IModApi
        {
            public void InitMod(Mod _modInstance)
            {
                Log.Out(" Loading Patch: " + GetType());
                var harmony = new HarmonyLib.Harmony(GetType().ToString());
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
        }


}