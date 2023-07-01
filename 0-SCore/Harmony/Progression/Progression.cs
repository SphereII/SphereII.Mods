using HarmonyLib;
using System.Reflection;


// Disabled due to Potential Performance issues


namespace Harmony.Progression
{
    /**
     * SCoreProgression
     * 
     * This class includes a Harmony patch to disable all XP from all events.
     */
    public class SCoreProgression
    {
        private static readonly string AdvFeatureClass = "AdvancedProgression";
        private static readonly string Feature = "ZeroXP";

        [HarmonyPatch(typeof(global::Progression))]
        [HarmonyPatch("AddLevelExpRecursive")]
        public class ProgressionNoAddLevelExpRecursive
        {
            private static bool Prefix()
            {
                // Check if this feature is enabled.
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }

        [HarmonyPatch(typeof(global::Progression))]
        [HarmonyPatch("AddLevelExp")]
        public class ProgressionNoExpAddLevelExpRecursive
        {
            private static bool Prefix()
            {
                // Check if this feature is enabled.
                return !Configuration.CheckFeatureStatus(AdvFeatureClass, Feature);
            }
        }

        [HarmonyPatch(typeof(global::Progression))]
        [HarmonyPatch("AddLevelExp")]
        public class ProgressionAddLevelExp
        {
            public enum XPTypes
            {
                Kill,
                Harvesting,
                Upgrading,
                Crafting,
                Selling,
                Quest,
                Looting,
                Party,
                Other,
                Repairing,
                Debug,
                Max
            }

            // Grab a reference to the private method.
            static readonly MethodInfo AddLevelExpRecursive = AccessTools.Method(typeof(global::Progression), "AddLevelExpRecursive");

            private static void Postfix(global::Progression __instance, global::EntityAlive ___parent, ref FastTags[] ___xpFastTags, int _exp, string _cvarXPName = "_xpOther", XPTypes _xpType = XPTypes.Other, bool useBonus = true)
            {
                if (___parent as EntityAliveSDX == null) return;

                float num = (float)_exp;
                if (useBonus)
                {
                    if (___xpFastTags == null)
                    {
                        ___xpFastTags = new FastTags[11];
                        for (int i = 0; i < 11; i++)
                        {
                            ___xpFastTags[i] = FastTags.Parse(((XPTypes)i).ToStringCached<XPTypes>());
                        }
                    }
                    num = num * (float)GameStats.GetInt(EnumGameStats.XPMultiplier) / 100f;
                    num = EffectManager.GetValue(PassiveEffects.PlayerExpGain, null, num, ___parent, null, ___xpFastTags[(int)_xpType], true, true, true, true, 1, true);
                }
                if (num > 214748370f)
                {
                    num = 214748370f;
                }

                Log.Out($"Total Experience: {__instance.ExpToNextLevel} {__instance.ExpDeficit} {__instance.Level}");
                AddLevelExpRecursive.Invoke(__instance, new object[] { (int)num, _cvarXPName });
                int level = __instance.Level;
            }
        }
    }
}