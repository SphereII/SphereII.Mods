using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;

 // ? Disabled due to Potential Performance issues


namespace Harmony.Progression
{

    // This gives "human" tagged entityalives access to the progression and levelling system.
    public class EntityAliveSDXProgressionHooks
    {
        // DISABLED for game v3.3 - the patch is both broken and redundant.
        //
        // Broken: it injects ___ProgressionValueQuickList as ProgressionValue[] and ___eventList as
        // List<ProgressionClass>. In v3.3 those fields are List<ProgressionValue> and
        // List<ProgressionValue>, so neither injection binds and Harmony.PatchAll aborts for the
        // entire SCore assembly.
        //
        // Redundant: Progression(EntityAlive) now calls SetupData() itself, for any EntityAlive and
        // not just players. SetupData fills ProgressionValues, ProgressionValueQuickList, eventList
        // AND passiveEffects - a superset of what this postfix hand-rolled - so re-running that work
        // here would re-Add keys that already exist in ProgressionValues.
        //
        // Behaviour change to be aware of: the "noprogression" cvar/tag guard below used to stop
        // progression data being built for an NPC. Vanilla now always builds it in the constructor,
        // so that opt-out no longer has anything to prevent at this point.
        //
        // [HarmonyPatch(typeof(global::Progression))]
        // [HarmonyPatch(MethodType.Constructor)]
        // [HarmonyPatch(new[] { typeof(global::EntityAlive) })]
        public class ProgressionAddPerksToNPCs
        {
            private static int getExpForLevel(float _level)
            {
                return (int)Math.Min((float)global::Progression.BaseExpToLevel * Mathf.Pow(global::Progression.ExpMultiplier, _level), 2.1474836E+09f);
            }

            private static void CalcEventList(global::Progression __instance, ref ProgressionValue[] ___ProgressionValueQuickList, ref List<ProgressionClass> ___eventList)
            {
                ___eventList = new List<ProgressionClass>();
                for (int i = 0; i < ___ProgressionValueQuickList.Length; i++)
                {
                    ProgressionClass progressionClass = ___ProgressionValueQuickList[i].ProgressionClass;
                    if (progressionClass.HasEvents())
                    {
                        ___eventList.Add(progressionClass);
                    }
                }
            }

            private static void Postfix(global::Progression __instance, ref DictionaryNameId<ProgressionValue> ___ProgressionValues, global::EntityAlive ___parent, ref global::EntityAlive _parent, ref ProgressionValue[] ___ProgressionValueQuickList, ref List<ProgressionClass> ___eventList)
            {
                if (_parent is not EntityAliveSDX entityAliveSdx)
                    return;

                if (entityAliveSdx.Buffs.GetCustomVar("noprogression") > 0f) return;
                if (entityAliveSdx.HasAnyTags(FastTags<TagGroup.Global>.Parse("noprogression"))) return;

                ___parent = _parent;
                if (__instance.ExpToNextLevel == 0)
                    __instance.ExpToNextLevel = getExpForLevel((float)(__instance.Level + 1));

                ___ProgressionValueQuickList = new ProgressionValue[global::Progression.ProgressionClasses.Count];
                int num = 0;
                foreach (KeyValuePair<string, ProgressionClass> keyValuePair in global::Progression.ProgressionClasses)
                {
                    ProgressionValue progressionValue = new ProgressionValue(keyValuePair.Value.Name)
                    {
                        Level = keyValuePair.Value.MinLevel,
                        CostForNextLevel = keyValuePair.Value.CalculatedCostForLevel(__instance.Level + 1)
                    };
                    ___ProgressionValues.Add(keyValuePair.Key, progressionValue);
                    ___ProgressionValueQuickList[num++] = progressionValue;
                }

                CalcEventList(__instance, ref ___ProgressionValueQuickList, ref ___eventList);
            }
        }
    }
}