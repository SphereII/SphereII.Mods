using System.Collections.Generic;
using DynamicMusic;
using HarmonyLib;
using MusicUtils.Enums;
using UnityEngine;

namespace WorldGenerationEngineFinal {
    public class PrefabManagerPatch {
        // [HarmonyPatch(typeof(PrefabManager))]
        // [HarmonyPatch("GetPrefabWithDistrict")]
        // public class PrefabManagerGetPrefabWithDistrict {
        //     public static bool Prefix(ref float densityPointsLeft) {
        //         // Set the density points to a high value, so it'll never fail a density check for placing of prefabs.
        //         // This will remove the restrictions of capping out a tile.
        //         densityPointsLeft = 1000f;
        //         return true;
        //     }
        // }
        [HarmonyPatch(typeof(PrefabManager))]
        [HarmonyPatch("GetPrefabWithDistrict")]
        public class PrefabManagerGetPrefabWithDistrict {
            public static bool Prefix(ref PrefabData __result, District _district, FastTags<TagGroup.Poi> _markerTags, Vector2i minSize, Vector2i maxSize, Vector2i center, float densityPointsLeft, float _distanceScale) {
                var flag = !_district.tag.IsEmpty;
                var flag2 = !_markerTags.IsEmpty;
                PrefabData prefabData = null;
                var num = float.MinValue;
                var worldSizeDistDiv = WorldBuilder.Instance.WorldSizeDistDiv;
                for (int i = 0; i < PrefabManager.prefabDataList.Count; i++)
                {
                    PrefabData prefabData2 = PrefabManager.prefabDataList[i];
                    
                    if (prefabData2.DensityScore <= densityPointsLeft && !prefabData2.Tags.Test_AnySet(PrefabManager.PartsAndTilesTags) && (!flag || prefabData2.Tags.Test_AllSet(_district.tag)) && (!flag2 || prefabData2.Tags.Test_AnySet(_markerTags)) && PrefabManager.isSizeValid(prefabData2, minSize, maxSize))
                    {
                        int num2 = prefabData2.ThemeRepeatDistance;
                        if (prefabData2.ThemeTags.Test_AnySet(PrefabManager.TraderTags))
                        {
                            num2 /= worldSizeDistDiv;
                        }

                        if (PrefabManager.isThemeValid(prefabData2, center, PrefabManager.UsedPrefabsWorld, num2) &&
                            (_distanceScale <= 0f || PrefabManager.isNameValid(prefabData2, center,
                                PrefabManager.UsedPrefabsWorld,
                                (int)((float)prefabData2.DuplicateRepeatDistance * _distanceScale))))
                        {
                            float scoreForPrefab = PrefabManager.getScoreForPrefab(prefabData2, center);
                            if (scoreForPrefab > num)
                            {
                                num = scoreForPrefab;
                                prefabData = prefabData2;
                            }

                       
                        }
                    }
                }

                __result = prefabData;
                return false;
            }
        }

    }
}