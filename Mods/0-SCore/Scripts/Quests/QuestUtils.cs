using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


public static class QuestUtils
{
    private static readonly string AdvFeatureClass = "AdvancedQuests";
    private static readonly string Feature = "ReusePOILocations";


    public static PrefabInstance FindPrefab(string poiName, Vector3 startPosition, ref List<Vector2> usedPOILocations, BiomeFilterTypes biomeFilterType = BiomeFilterTypes.AnyBiome, string biomeFilter = "")
    {
        //var listOfPrefabs = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetPOIPrefabs().FindAll(instance => instance.name.Contains(poiName));
        var listOfPrefabs = GameManager.Instance.World.ChunkClusters[0].ChunkProvider.GetDynamicPrefabDecorator().GetDynamicPrefabs().FindAll(instance => instance.name.Contains(poiName));
        // Filter the prefab list if there's an exact name
        var filteredPrefabs = listOfPrefabs.FindAll(instance => instance.name == poiName);
        if (filteredPrefabs.Count > 0)
            listOfPrefabs = filteredPrefabs;

        // Find the closes Prefab
        var prefab = FindClosesPrefabs(startPosition, listOfPrefabs, usedPOILocations, biomeFilterType, biomeFilter);
        return prefab;
    }
    public static PrefabInstance FindClosesPrefabs(Vector3 position, List<PrefabInstance> prefabs, List<Vector2> usedPOILocations, BiomeFilterTypes biomeFilterType, string biomeFilter)
    {
        PrefabInstance prefab = null;
        var minDist = Mathf.Infinity;
        string[] array = null;

        foreach (var t in prefabs)
        {
            // Have we already went to this one?
            var vector = new Vector2(t.boundingBoxPosition.x, t.boundingBoxPosition.z);
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
            {
                if (usedPOILocations != null && usedPOILocations.Contains(vector)) continue;    
            }

            

            // Check if there's a biome filter.
            if (biomeFilterType != BiomeFilterTypes.AnyBiome)
            {
                var biomeAt = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt((int)vector.x, (int)vector.y);
                if (biomeFilterType == BiomeFilterTypes.OnlyBiome && biomeAt.m_sBiomeName != biomeFilter)
                {
                    if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                        Log.Out($"GotoPOISDX: Prefab Filtered based on Biome Filter Type: {biomeFilterType}  Biome Filter: {biomeFilter}");

                    continue;
                }
                if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
                {
                    array ??= biomeFilter.Split(new char[] {
                        ','
                    });
                    var flag = false;
                    for (var j = 0; j < array.Length; j++)
                    {
                        if (biomeAt.m_sBiomeName != array[j]) continue;
                        if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                            Log.Out($"GotoPOISDX: Prefab excluded based on Biome Filter Type: {biomeFilterType}  Biome Filter: {biomeFilter}");

                        flag = true;
                        break;
                    }
                    if (flag)
                    {
                        continue;
                    }
                }
            }

            var dist = Vector3.Distance(t.boundingBoxPosition, position);
            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                Log.Out($"GotoPOISDX: Prefab {t.name} Found at {t.boundingBoxPosition} Distance: {dist} {biomeFilterType}  Biome Filter: {biomeFilter}");

            if (!(dist < minDist)) continue;
            if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled))
                if (prefab != null)
                    Log.Out($"GotoPOISDX: Found closer Prefab {t.name} than {prefab.name} Old distance {minDist}");
            prefab = t;
            minDist = dist;
        }

        return prefab;
    }

    /// <summary>
    /// Whether logging is enabled. Defaults to true if the debug menu is enabled.
    /// Can be set to temporarily override in code for testing, e.g. on the server.
    /// </summary>
    public static bool LoggingEnabled
    {
        get => _loggingEnabled ?? GamePrefs.GetBool(EnumGamePrefs.DebugMenuEnabled);
        set => _loggingEnabled = value;
    }
    private static bool? _loggingEnabled;

    /// <summary>
    /// Gets a random POI near a trader. This is meant to be a replacement for
    /// <see cref="DynamicPrefabDecorator.GetRandomPOINearTrader"/>,
    /// except it also handles search distance and POI tags used to include or exclude the prefab.
    /// </summary>
    /// <param name="trader"></param>
    /// <param name="questTag"></param>
    /// <param name="difficulty"></param>
    /// <param name="usedPoiLocations"></param>
    /// <param name="entityIdForQuests"></param>
    /// <param name="biomeFilterType"></param>
    /// <param name="biomeFilter"></param>
    /// <param name="includeTags"></param>
    /// <param name="excludeTags"></param>
    /// <returns></returns>
    public static PrefabInstance GetRandomPOINearTrader(
        EntityTrader trader,
        FastTags<TagGroup.Global> questTag,
        byte difficulty,
        FastTags<TagGroup.Poi> includeTags,
        FastTags<TagGroup.Poi> excludeTags,
        float minSearchDistance,
        float maxSearchDistance,
        List<Vector2> usedPoiLocations = null,
        int entityIdForQuests = -1,
        BiomeFilterTypes biomeFilterType = BiomeFilterTypes.AnyBiome,
        string biomeFilter = "")
    {
        var world = GameManager.Instance.World;

        var minDistanceTier = minSearchDistance < 0 ? 0 : GetTraderPrefabListTier(minSearchDistance);
        var maxDistanceTier = maxSearchDistance < 0 ? 2 : GetTraderPrefabListTier(maxSearchDistance);

        for (var distanceTier = minDistanceTier; distanceTier <= maxDistanceTier; distanceTier++)
        {
           var prefabsForTrader = QuestEventManager.Current.GetPrefabsForTrader(
                trader.traderArea,
                difficulty,
                distanceTier,
                world.GetGameRandom());

           if (prefabsForTrader == null) continue;
           // GetPrefabsForTrader shuffles the prefabs before returning them, so we can just
            // iterate through the list and still send players to "random" POIs
            for (var j = 0; j < prefabsForTrader.Count; j++)
            {
                var prefabInstance = prefabsForTrader[j];
                if (ValidPrefabForQuest(
                        trader,
                        prefabInstance,
                        questTag,
                        includeTags,
                        excludeTags,
                        usedPoiLocations,
                        entityIdForQuests,
                        biomeFilterType,
                        biomeFilter,
                        minSearchDistance,
                        maxSearchDistance))
                {
                    return prefabInstance;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a random POI near an entity's position. This is meant to be a replacement for
    /// <see cref="DynamicPrefabDecorator.GetRandomPOINearWorldPos"/>
    /// except it accepts the quest giver instead of the world position, and it also handles
    /// POI tags used to include or exclude the prefab.
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="questTag"></param>
    /// <param name="difficulty"></param>
    /// <param name="includeTags"></param>
    /// <param name="excludeTags"></param>
    /// <param name="minSearchDistance"></param>
    /// <param name="maxSearchDistance"></param>
    /// <param name="usedPOILocations"></param>
    /// <param name="entityIDforQuests"></param>
    /// <param name="biomeFilterType"></param>
    /// <param name="biomeFilter"></param>
    /// <returns></returns>
    public static PrefabInstance GetRandomPOINearEntityPos(
        Entity entity,
        FastTags<TagGroup.Global> questTag,
        byte difficulty,
        FastTags<TagGroup.Poi> includeTags,
        FastTags<TagGroup.Poi> excludeTags,
        float minSearchDistance,
        float maxSearchDistance,
        List<Vector2> usedPOILocations = null,
        int entityIDforQuests = -1,
        BiomeFilterTypes biomeFilterType = BiomeFilterTypes.AnyBiome,
        string biomeFilter = "")
    {
        List<PrefabInstance> prefabsByDifficultyTier = QuestEventManager.Current.GetPrefabsByDifficultyTier(difficulty);
        if (prefabsByDifficultyTier == null)
        {
            return null;
        }

        World world = GameManager.Instance.World;

        // GetPrefabsByDifficultyTier does NOT shuffle the prefabs before returning them, so try
        // 50 times to get a valid prefab from a random position in the list
        for (int i = 0; i < 50; i++)
        {
            int index = world.GetGameRandom().RandomRange(prefabsByDifficultyTier.Count);
            PrefabInstance prefabInstance = prefabsByDifficultyTier[index];

            if (!ValidPrefabForQuest(
                entity,
                prefabInstance,
                questTag,
                includeTags,
                excludeTags,
                usedPOILocations,
                entityIDforQuests,
                biomeFilterType,
                biomeFilter,
                minSearchDistance,
                maxSearchDistance))
            {
                continue;
            }

            return prefabInstance;
        }
        return null;
    }

    /// <summary>
    /// The trader prefab list has three distance tiers per trader area:
    /// 0 is <= 500, 1 is <= 1500, and 2 is everything else.
    /// </summary>
    /// <param name="distance"></param>
    /// <returns></returns>
    public static int GetTraderPrefabListTier(float distance)
    {
        if (distance <= 500f)
            return 0;

        if (distance <= 1500f)
            return 1;

        return 2;
    }

    /// <summary>
    /// Returns true if the prefab is valid for a quest. Uses all utility validation methods.
    /// </summary>
    /// <param name="questGiver"></param>
    /// <param name="prefab"></param>
    /// <param name="questTag"></param>
    /// <param name="includeTags"></param>
    /// <param name="excludeTags"></param>
    /// <param name="usedPoiLocations"></param>
    /// <param name="entityIdForQuests"></param>
    /// <param name="biomeFilterType"></param>
    /// <param name="biomeFilter"></param>
    /// <param name="minSearchDistance"></param>
    /// <param name="maxSearchDistance"></param>
    /// <returns></returns>
    public static bool ValidPrefabForQuest(
        Entity questGiver,
        PrefabInstance prefab,
        FastTags<TagGroup.Global> questTag,
        FastTags<TagGroup.Poi> includeTags,
        FastTags<TagGroup.Poi> excludeTags,
        List<Vector2> usedPoiLocations = null,
        int entityIdForQuests = -1,
        BiomeFilterTypes biomeFilterType = BiomeFilterTypes.AnyBiome,
        string biomeFilter = "",
        float minSearchDistance = -1,
        float maxSearchDistance = -1)
    {
        if (!prefab.prefab.bSleeperVolumes)
        {
            if (LoggingEnabled)
            {
                Log.Out($"Quest {questTag}: Prefab {prefab.name} has no sleeper volumes");
            }
            return false;
        }

        //if (!prefab.prefab.GetQuestTag(questTag))
        //{
        //    if (LoggingEnabled)
        //    {
        //        Log.Out($"Quest {questTag}: Prefab {prefab.name} does not have quest tag {questTag}");
        //    }
        //    return false;
        //}

        Vector2 poiLocation = new Vector2(prefab.boundingBoxPosition.x, prefab.boundingBoxPosition.z);

        if (usedPoiLocations != null && usedPoiLocations.Contains(poiLocation))
        {
            if (LoggingEnabled)
            {
                Log.Out($"Quest {questTag}: Prefab {prefab.name} has already been used");
            }
            return false;
        }

        QuestEventManager.POILockoutReasonTypes lockoutReason = QuestEventManager.Current.CheckForPOILockouts(entityIdForQuests, poiLocation, out var num);

        if (lockoutReason != QuestEventManager.POILockoutReasonTypes.None)
        {
            if (LoggingEnabled)
            {
                Log.Out($"Quest {questTag}: Prefab {prefab.name} is locked out: {lockoutReason}");
            }
            return false;
        }

        if (!MeetsBiomeRequirements(poiLocation, questGiver, biomeFilterType, biomeFilter))
        {
            if (LoggingEnabled)
            {
                Log.Out($"Quest {questTag}: Prefab {prefab.name} does not meet biome requirements: type={biomeFilterType}, filter={biomeFilter}");
            }
            return false;
        }

        if (!MeetsTagRequirements(prefab, includeTags, excludeTags))
        {
            if (LoggingEnabled)
            {
                var includeTagsText = includeTags.IsEmpty ? string.Empty : $"must include {includeTags}; ";
                var excludeTagsText = excludeTags.IsEmpty ? string.Empty : $"must exclude {excludeTags}; ";
                Log.Out($"Quest {questTag}: Prefab {prefab.name} fails tag requirements: {includeTagsText}{excludeTagsText}prefab tags are{prefab.prefab.Tags}");
            }
            return false;
        }

        if (!MeetsDistanceRequirements(prefab, questGiver, minSearchDistance, maxSearchDistance))
        {
            if (LoggingEnabled)
            {
                var minSearchText = minSearchDistance > 0 ? $" minimum distance {minSearchDistance} and" : string.Empty;
                Log.Out($"Quest {questTag}: Prefab {prefab.name} is not within{minSearchText} maximum distance {maxSearchDistance}");
            }
            return false;
        }

        return true;
    }

    /// <summary>
    /// If min and/or max search distance is provided, returns true if the prefab is within that
    /// search distance from the quest giver.
    /// </summary>
    /// <param name="questGiver"></param>
    /// <param name="prefab"></param>
    /// <param name="minSearchDistance"></param>
    /// <param name="maxSearchDistance"></param>
    /// <returns></returns>
    public static bool MeetsDistanceRequirements(
        PrefabInstance prefab,
        Entity questGiver,
        float minSearchDistance,
        float maxSearchDistance)
    {
        if (minSearchDistance < 0 && maxSearchDistance < 0)
            return true;

        Vector2 worldPos = new Vector2(questGiver.position.x, questGiver.position.z);

        Vector2 prefabCenter = prefab.GetCenterXZ();

        // Work with the square of the distance to avoid taking square roots
        float sqrDistance = (worldPos - prefabCenter).sqrMagnitude;

        if (minSearchDistance >= 0 &&
            sqrDistance < (minSearchDistance * minSearchDistance))
        {
            return false;
        }

        if (maxSearchDistance >= 0 &&
            sqrDistance > (maxSearchDistance * maxSearchDistance))
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Returns true if the POI at the specified location meets the biome filter requirements.
    /// </summary>
    /// <param name="poiLocation"></param>
    /// <param name="questGiver"></param>
    /// <param name="biomeFilterType"></param>
    /// <param name="biomeFilter"></param>
    /// <returns></returns>
    public static bool MeetsBiomeRequirements(
        Vector2 poiLocation,
        Entity questGiver,
        BiomeFilterTypes biomeFilterType,
        string biomeFilter)
    {
        if (biomeFilterType == BiomeFilterTypes.AnyBiome)
        {
            return true;
        }

        IBiomeProvider biomeProvider = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider();

        BiomeDefinition poiBiome = biomeProvider.GetBiomeAt((int)poiLocation.x, (int)poiLocation.y);

        if (biomeFilterType == BiomeFilterTypes.OnlyBiome && poiBiome.m_sBiomeName != biomeFilter)
        {
            return false;
        }
        else if (biomeFilterType == BiomeFilterTypes.ExcludeBiome)
        {
            string[] biomes = biomeFilter.Split(new char[] { ',' });

            bool inBiome = false;

            for (int i = 0; i < biomes.Length; i++)
            {
                if (poiBiome.m_sBiomeName == biomes[i])
                {
                    inBiome = true;
                    break;
                }
            }

            if (inBiome)
            {
                return false;
            }
        }
        else if (biomeFilterType == BiomeFilterTypes.SameBiome && questGiver != null)
        {
            var questGiverBiome = biomeProvider.GetBiomeAt(
                (int)questGiver.position.x,
                (int)questGiver.position.z);

            if (poiBiome != questGiverBiome && biomeFilter != "" && poiBiome.m_sBiomeName != biomeFilter)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Returns true if the POI meets the POI tag requirements.
    /// If include tags are provided, it must have one or more of those tags;
    /// if exclude tags are provided, it cannot have any of those tags.
    /// </summary>
    /// <param name="prefabInstance"></param>
    /// <param name="includeTags"></param>
    /// <param name="excludeTags"></param>
    /// <returns></returns>
    public static bool MeetsTagRequirements(PrefabInstance prefabInstance, FastTags<TagGroup.Poi> includeTags, FastTags<TagGroup.Poi> excludeTags)
    {
        if (!includeTags.IsEmpty &&
            !prefabInstance.prefab.Tags.Test_AnySet(includeTags))
        {
            return false;
        }

        if (!excludeTags.IsEmpty &&
            prefabInstance.prefab.Tags.Test_AnySet(excludeTags))
        {
            return false;
        }

        return true;
    }
}
