using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is meant to be a replacement for <see cref="ObjectiveRandomPOIGoto"/>,
/// with these differences:
/// <list type="bullet">
/// <item>You can specify which tags a POI must have, to be a target for the quest</item>
/// <item>You can specify which tags a POI must NOT have, to be a target for the quest</item>
/// <item>You can specify the search distance from the quest giver, as either a max distance or a range</item>
/// </list>
/// </summary>
/// <example>
/// <code>
/// &lt;objective type="RandomTaggedPOIGotoSDX, SCore">
///     &lt;!-- Standard objective properties not listed... -->
///     &lt;property name="include_tags" value="downtown,industrial"/>
///     &lt;property name="exclude_tags" value="rural,wilderness"/>
///     &lt;property name="distance" value="300-1000"/>
/// &lt;/objective>
/// </code>
/// </example>
public class ObjectiveRandomTaggedPOIGotoSDX : ObjectiveRandomPOIGoto
{
    /// <summary>
    /// The name of the property used to include POI tags.
    /// </summary>
    public const string PropIncludeTags = "include_tags";

    /// <summary>
    /// The name of the property used to exclude POI tags.
    /// </summary>
    public const string PropExcludeTags = "exclude_tags";

    /// <summary>
    /// The name of the property that limits the search to POIs with a variant for the quest's
    /// "poi_variant_suffix" (see QuestPOIVariantManager).
    /// </summary>
    public const string PropRequirePoiVariant = "require_poi_variant";

    /// <summary>
    /// The name of the property that lets the quest pick POIs the player already completed for
    /// this trader, instead of waiting out the trader's 7 day cooldown.
    /// </summary>
    public const string PropReuseCompletedPois = "reuse_completed_pois";

    /// <summary>
    /// A prefab must have at least one of these POI tags to be included in the search for POIs.
    /// </summary>
    public FastTags<TagGroup.Poi> IncludeTags { get; internal set; } = FastTags<TagGroup.Poi>.none;

    /// <summary>
    /// If a prefab has any of these POI tags, it is excluded from the search for POIs.
    /// </summary>
    public FastTags<TagGroup.Poi> ExcludeTags { get; internal set; } = FastTags<TagGroup.Poi>.none;

    /// <summary>
    /// The minimum distance to search. POIs inside this distance will not be returned.
    /// </summary>
    public float MinSearchDistance { get; internal set; } = -1;

    /// <summary>
    /// The maximum distance to search. POIs outside this distance will not be returned.
    /// </summary>
    public float MaxSearchDistance { get; internal set; } = -1;

    /// <summary>
    /// If true, only POIs that have a variant prefab for the quest's "poi_variant_suffix" are returned.
    /// </summary>
    public bool RequirePoiVariant { get; internal set; }

    /// <summary>
    /// If true, POIs the player already completed for the trader can be picked again. POIs that are
    /// locked by another quest, or already in the same job list, are still skipped.
    /// </summary>
    public bool ReuseCompletedPois { get; internal set; }

    /// <summary>
    /// Sets up the quest objective.
    /// </summary>
    public override void SetupObjective()
    {
        base.SetupObjective();
        keyword = Localization.Get("ObjectiveRandomTaggedPOIGotoSDX");
    }

    /// <summary>
    /// Parses additional properties from the dynamic properties.
    /// </summary>
    /// <param name="properties"></param>
    public override void ParseProperties(DynamicProperties properties)
    {
        if (properties.Values.ContainsKey(PropIncludeTags))
        {
            IncludeTags = FastTags<TagGroup.Poi>.Parse(properties.Values[PropIncludeTags].Replace(" ", ""));
        }
        if (properties.Values.ContainsKey(PropExcludeTags))
        {
            ExcludeTags = FastTags<TagGroup.Poi>.Parse(properties.Values[PropExcludeTags].Replace(" ", ""));
        }
        if (properties.Values.ContainsKey(PropRequirePoiVariant))
        {
            RequirePoiVariant = StringParsers.ParseBool(properties.Values[PropRequirePoiVariant]);
        }
        if (properties.Values.ContainsKey(PropReuseCompletedPois))
        {
            ReuseCompletedPois = StringParsers.ParseBool(properties.Values[PropReuseCompletedPois]);
        }
        if (properties.Values.ContainsKey(PropDistance))
        {
            string searchDistance = properties.Values[PropDistance].Replace(" ", "");
            if (searchDistance.Contains("-"))
            {
                string[] distances = searchDistance.Split(new char[] { '-' });
                MinSearchDistance = StringParsers.ParseFloat(distances[0]);
                MaxSearchDistance = StringParsers.ParseFloat(distances[1]);
            }
            else
            {
                MaxSearchDistance = StringParsers.ParseFloat(searchDistance);
            }
            // Remove the property value so the base class doesn't parse it
            properties.Values.Remove(PropDistance);
        }
        base.ParseProperties(properties);
    }

    /// <summary>
    /// Gets the POI position. Returns Vector3.Zero if none found.
    /// </summary>
    /// <param name="ownerNPC"></param>
    /// <param name="entityPlayer"></param>
    /// <param name="usedPoiLocations"></param>
    /// <param name="entityIdForQuests"></param>
    /// <returns></returns>
    public override Vector3 GetPosition(
        EntityNPC ownerNPC = null,
        EntityPlayer entityPlayer = null,
        List<Vector2> usedPoiLocations = null,
        int entityIdForQuests = -1)
    {
        // This is copied from ObjectiveRandomPOIGoto and modified to use utility methods that accept tags
        if (OwnerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition))
        {
            OwnerQuest.GetPositionData(out Vector3 poiSize, Quest.PositionDataTypes.POISize);

            Vector2 poiCenter = new Vector2(
                position.x + poiSize.x / 2f,
                position.z + poiSize.z / 2f);

            var poiY = position.y;
            position = PrefabCenterToPosition(poiCenter);
            // No height data (e.g. Playtest): (0,0,0) would read as "no POI found", so use the POI's own height.
            if (position.y == 0)
                position.y = poiY;

            OwnerQuest.Position = position;

            SetDistanceOffset(poiSize);

            positionSet = true;

            OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName, -1);

            CurrentValue = 2;

            return position;
        }

        EntityAlive questOwner = ownerNPC ?? (EntityAlive)OwnerQuest.OwnerJournal.OwnerPlayer;

        string variantSuffix = null;
        if (RequirePoiVariant)
        {
            variantSuffix = QuestPOIVariantManager.GetSuffix(OwnerQuest.QuestClass);
            if (string.IsNullOrEmpty(variantSuffix))
                Log.Warning($"ObjectiveRandomTaggedPOIGotoSDX: quest {OwnerQuest.ID} has {PropRequirePoiVariant} but no {QuestPOIVariantManager.PropVariantSuffix}; ignoring it.");
        }

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            PrefabInstance prefabInstance;

            // Modified so it won't call the method if the "trader" has no TraderArea.
            // Core NPCs descend from traders, and this will allow them to give POI quests.
            if (questOwner is EntityTrader trader && trader.traderArea != null)
            {
                // The trader's list of used POIs holds the player's completed POIs for this trader,
                // plus the POIs already taken by this job list. Drop only the completed ones.
                var searchUsedPoiLocations = usedPoiLocations;
                if (ReuseCompletedPois)
                {
                    searchUsedPoiLocations = QuestUtils.WithoutCompletedPOIs(
                        usedPoiLocations,
                        entityPlayer ?? OwnerQuest.OwnerJournal?.OwnerPlayer,
                        new Vector2(trader.traderArea.Position.x, trader.traderArea.Position.z));
                }

                prefabInstance = QuestUtils.GetRandomPOINearTrader(
                    trader,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestClass.DifficultyTier,
                    IncludeTags,
                    ExcludeTags,
                    MinSearchDistance,
                    MaxSearchDistance,
                    searchUsedPoiLocations,
                    entityIdForQuests,
                    biomeFilterType,
                    biomeFilter,
                    variantSuffix);
            }
            else
            {
                prefabInstance = QuestUtils.GetRandomPOINearEntityPos(
                    questOwner,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestClass.DifficultyTier,
                    IncludeTags,
                    ExcludeTags,
                    // The values used in vanilla are the square of the distance; adjusted here
                    MinSearchDistance >= 0 ? MinSearchDistance : 32,
                    MaxSearchDistance >= 0 ? MaxSearchDistance : 2000,
                    usedPoiLocations,
                    entityIdForQuests,
                    biomeFilterType,
                    biomeFilter,
                    variantSuffix);
            }

            if (prefabInstance == null)
            {
                Log.Warning($"Unable to find POI for quest {OwnerQuest.QuestTags}!");
                return Vector3.zero;
            }

            // This is taken from the vanilla ObjectiveRandomPOIGoto - but when is it ever the case?
            Vector2 prefabCenter = prefabInstance.GetCenterXZ();
            if (prefabCenter.x == -0.1f && prefabCenter.y == -0.1f)
            {
                Log.Error("ObjectiveRandomTaggedPOIGotoSDX: No POI found.");
                return Vector3.zero;
            }

            position = PrefabCenterToPosition(prefabCenter);
            // No height data (e.g. Playtest, where the POI is centered on 0,0): a position of (0,0,0)
            // would read as "no POI found", so use the POI's own height.
            if (position.y == 0)
                position.y = prefabInstance.boundingBoxPosition.y;

            if (GameManager.Instance.World.IsPositionInBounds(position))
            {
                OwnerQuest.Position = position;

                FinalizePoint(
                    new Vector3(
                        prefabInstance.boundingBoxPosition.x,
                        prefabInstance.boundingBoxPosition.y,
                        prefabInstance.boundingBoxPosition.z),
                    new Vector3(
                        prefabInstance.boundingBoxSize.x,
                        prefabInstance.boundingBoxSize.y,
                        prefabInstance.boundingBoxSize.z));

                OwnerQuest.QuestPrefab = prefabInstance;

                OwnerQuest.DataVariables.Add("POIName", Localization.Get(OwnerQuest.QuestPrefab.location.Name));

                if (usedPoiLocations != null)
                {
                    usedPoiLocations.Add(new Vector2(
                        prefabInstance.boundingBoxPosition.x,
                        prefabInstance.boundingBoxPosition.z));
                }

                // FinalizePoint recomputes position with the same height lookup, so apply the fallback again.
                if (position.y == 0)
                    position.y = prefabInstance.boundingBoxPosition.y;
                return position;
            }
        }
        else
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageRandomTaggedPOIGotoSDX>().Setup(
                    questOwner.entityId,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestCode,
                    OwnerQuest.QuestClass.DifficultyTier,
                    IncludeTags,
                    ExcludeTags,
                    MinSearchDistance,
                    MaxSearchDistance,
                    0, -1, // POI boundingBoxPosition
                    0f, 0f, 0f, // POI boundingBoxSize
                    biomeFilterType,
                    biomeFilter,
                    variantSuffix,
                    ReuseCompletedPois),
                false);

            CurrentValue = 1;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Converts a 2D location representing the center of a prefab to a 3D position in the world.
    /// </summary>
    /// <param name="prefabCenter"></param>
    /// <returns></returns>
    public static Vector3 PrefabCenterToPosition(Vector2 prefabCenter)
    {
        // Since the center is a 2D vector, its "y" is actually the z-axis position
        int x = (int)prefabCenter.x;
        int y = (int)GameManager.Instance.World.GetHeightAt(prefabCenter.x, prefabCenter.y);
        int z = (int)prefabCenter.y;
        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Clones this Objective.
    /// </summary>
    /// <returns></returns>
    public override BaseObjective Clone()
    {
        var objective = new ObjectiveRandomTaggedPOIGotoSDX();
        CopyValues(objective);
        return objective;
    }

    /// <summary>
    /// Copies the values from another Objective.
    /// </summary>
    /// <param name="objective"></param>
    public override void CopyValues(BaseObjective objective)
    {
        base.CopyValues(objective);
        ObjectiveRandomTaggedPOIGotoSDX obj = (ObjectiveRandomTaggedPOIGotoSDX)objective;
        obj.IncludeTags = IncludeTags;
        obj.ExcludeTags = ExcludeTags;
        obj.MinSearchDistance = MinSearchDistance;
        obj.MaxSearchDistance = MaxSearchDistance;
        obj.RequirePoiVariant = RequirePoiVariant;
        obj.ReuseCompletedPois = ReuseCompletedPois;
        // These values are copied in ObjectiveRandomPOIGoto.Clone, and this class can't call that
        // base class method because it creates the wrong object type. So, copy the values here.
        obj.poiTier = poiTier;
        obj.distance = distance;
        obj.biomeFilterType = biomeFilterType;
        obj.biomeFilter = biomeFilter;
        obj.locationName = locationName;
    }
}