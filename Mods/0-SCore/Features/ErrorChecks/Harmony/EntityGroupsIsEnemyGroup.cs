using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.ErrorChecks.Harmony
{
    public class EntityGroupsIsEnemyGroup
    {
        [HarmonyPatch(typeof(EntityGroups))]
        [HarmonyPatch(nameof(EntityGroups.IsEnemyGroup))]
        public class EntityGroupsIsEnemyGroupPatch
        {
            public static bool Prefix(ref bool __result, string _sEntityGroupName)
            {

                if (string.IsNullOrEmpty(_sEntityGroupName) || !EntityGroups.list.ContainsKey(_sEntityGroupName))
                {
                    Log.Out($"[0-SCore] IsEnemyGroup Error: Group '{_sEntityGroupName}' not found in EntityGroups.");
                    return false;
                }

                List<SEntityClassAndProb> list = EntityGroups.list[_sEntityGroupName];

                if (list == null || list.Count == 0)
                {
                    Log.Out($"[0-SCore] IsEnemyGroup Error: Group '{_sEntityGroupName}' is empty or null.");
                    return false;
                }

                // 3. Validation: Entity Class Integrity
                // Note: This checks the first entity in the group to determine if the whole group is hostile.
                int entityClassId = list[0].entityClassId;
                if (!EntityClass.list.ContainsKey(entityClassId))
                {
                    Log.Out(
                        $"[0-SCore] IsEnemyGroup Error: Group '{_sEntityGroupName}' references an invalid EntityClassID ({entityClassId}). Check XML for typos.");
                    return false;
                }

                __result = EntityClass.list[entityClassId].bIsEnemyEntity;
                return false;
            }
        }
    }
}