using System.Reflection;

namespace DisableSway.Harmony {
        public class DisableSwayInit : IModApi
        {
            public void InitMod(Mod _modInstance)
            {
                Log.Out(" Loading Patch: " + GetType());
                var harmony = new HarmonyLib.Harmony(GetType().ToString());
                harmony.PatchAll(Assembly.GetExecutingAssembly());

            }
        }


}