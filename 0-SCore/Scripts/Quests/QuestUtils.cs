using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class QuestUtils
    {

    public static PrefabInstance FindPrefab(string poiName, Vector3 startPosition, ref List<Vector2> usedPOILocations, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.AnyBiome, string biomeFilter = "")
    {
        //var listOfPrefabs = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().FindAll(instance => instance.name.Contains(poiName));
        var listOfPrefabs = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetDynamicPrefabs().FindAll(instance => instance.name.Contains(poiName));
        if (listOfPrefabs == null)
        {
            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
            {
                Log.Out($"GotoPOISDX: No Prefabs by this name found: {poiName} Biome Filter Type: {biomeFilterType} Biome Filter: {biomeFilter}");
            }
            return null;
        }
        // Find the closes Prefab
        var prefab = QuestUtils.FindClosesPrefabs(startPosition, listOfPrefabs, usedPOILocations, biomeFilterType, biomeFilter);
        if (prefab == null)
        {
            return null;
        }
        return prefab;
    }
    public static  PrefabInstance FindClosesPrefabs(Vector3 position, List<PrefabInstance> prefabs, List<Vector2> usedPOILocations, BiomeFilterTypes biomeFilterType , string biomeFilter)
    {
        PrefabInstance prefab = null;
        float minDist = Mathf.Infinity;
        string[] array = null;

        foreach (var t in prefabs)
        {
            // Have we already went to this one?
            Vector2 vector = new Vector2((float)t.boundingBoxPosition.x, (float)t.boundingBoxPosition.z);
            if (usedPOILocations != null && usedPOILocations.Contains(vector))
                continue;

            // Check if there's a biome filter.
            if (biomeFilterType != BiomeFilterTypes.AnyBiome)
            {
                BiomeDefinition biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
                if (biomeFilterType == BiomeFilterTypes.OnlyBiome && biomeAt.m_sBiomeName != biomeFilter)
                {
                    if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                        Log.Out($"GotoPOISDX: Prefab Filtered based on Biome Filter Type: {biomeFilterType}  Biome Filter: {biomeFilter}");

                    continue;
                }
                if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
                {
                    if (array == null)
                    {
                        array = biomeFilter.Split(new char[]
                        {
                                    ','
                        });
                    }
                    bool flag = false;
                    for (int j = 0; j < array.Length; j++)
                    {
                        if (biomeAt.m_sBiomeName == array[j])
                        {
                            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                                Log.Out($"GotoPOISDX: Prefab excluded based on Biome Filter Type: {biomeFilterType}  Biome Filter: {biomeFilter}");

                            flag = true;
                            break;
                        }
                    }
                    if (flag)
                    {
                        continue;
                    }
                }
            }

            float dist = Vector3.Distance(t.boundingBoxPosition, position);
            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                Log.Out($"GotoPOISDX: Prefab {t.name} Found at {t.boundingBoxPosition} Distance: {dist} {biomeFilterType}  Biome Filter: {biomeFilter}");

            if (dist < minDist)
            {
                if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                    if ( prefab != null)
                        Log.Out($"GotoPOISDX: Found closer Prefab {t.name} than {prefab.name} Old distance {minDist}");
                prefab = t;
                minDist = dist;
            }
        }

        return prefab;
    }
}

