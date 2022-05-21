﻿using System.Collections.Generic;
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
    /// A prefab must have at least one of these POI tags to be included in the search for POIs.
    /// </summary>
    public POITags? IncludeTags { get; internal set; }

    /// <summary>
    /// If a prefab has any of these POI tags, it is excluded from the search for POIs.
    /// </summary>
    public POITags? ExcludeTags { get; internal set; }

    /// <summary>
    /// The maximum distance to search. POIs outside this distance will not be returned.
    /// </summary>
    public float? MaxSearchDistance { get; internal set; }

    /// <summary>
    /// The minimum distance to search. POIs inside this distance will not be returned.
    /// </summary>
    public float? MinSearchDistance { get; internal set; }

    /// <summary>
    /// Parses additional properties from the dynamic properties.
    /// </summary>
    /// <param name="properties"></param>
    public override void ParseProperties(DynamicProperties properties)
    {
        if (properties.Values.ContainsKey(PropIncludeTags))
        {
            IncludeTags = POITags.Parse(properties.Values[PropIncludeTags].Replace(" ", ""));
        }
        if (properties.Values.ContainsKey(PropExcludeTags))
        {
            ExcludeTags = POITags.Parse(properties.Values[PropExcludeTags].Replace(" ", ""));
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
    protected override Vector3 GetPosition(
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

            position = PrefabCenterToPosition(poiCenter);

            OwnerQuest.Position = position;

            SetDistanceOffset(poiSize);

            positionSet = true;

            OwnerQuest.HandleMapObject(Quest.PositionDataTypes.POIPosition, NavObjectName, -1);

            CurrentValue = 2;

            return position;
        }

        EntityAlive entityAlive = ownerNPC ?? (EntityAlive)OwnerQuest.OwnerJournal.OwnerPlayer;

        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            PrefabInstance prefabInstance;

            // Modified so it won't call the method if the "trader" has no TraderArea.
            // Core NPCs descend from traders, and this will allow them to give POI quests.
            if (entityAlive is EntityTrader trader && trader.traderArea != null)
            {
                prefabInstance = QuestUtils.GetRandomPOINearTrader(
                    trader,
                    MinSearchDistance,
                    MaxSearchDistance,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestClass.DifficultyTier,
                    usedPoiLocations,
                    entityIdForQuests,
                    biomeFilterType,
                    biomeFilter,
                    IncludeTags,
                    ExcludeTags);
            }
            else
            {
                prefabInstance = QuestUtils.GetRandomPOINearEntityPos(
                    entityAlive,
                    // The values used in vanilla are the square of the distance; adjusted here
                    MinSearchDistance ?? 50,
                    MaxSearchDistance ?? 2000,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestClass.DifficultyTier,
                    usedPoiLocations,
                    entityIdForQuests,
                    biomeFilterType,
                    biomeFilter,
                    IncludeTags,
                    ExcludeTags);
            }

            if (prefabInstance == null)
            {
                return Vector3.zero;
            }

            Vector2 prefabCenter = new Vector2(
                prefabInstance.boundingBoxPosition.x + prefabInstance.boundingBoxSize.x / 2f,
                prefabInstance.boundingBoxPosition.z + prefabInstance.boundingBoxSize.z / 2f);

            if (prefabCenter.x == -0.1f && prefabCenter.y == -0.1f)
            {
                Log.Error("ObjectiveRandomGoto: No POI found.");
                return Vector3.zero;
            }

            position = PrefabCenterToPosition(prefabCenter);

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

                OwnerQuest.DataVariables.Add("POIName", OwnerQuest.QuestPrefab.location.Name);

                if (usedPoiLocations != null)
                {
                    usedPoiLocations.Add(new Vector2(
                        prefabInstance.boundingBoxPosition.x,
                        prefabInstance.boundingBoxPosition.z));
                }

                return position;
            }
        }
        else
        {
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(
                NetPackageManager.GetPackage<NetPackageQuestGotoPoint>().Setup(
                    entityAlive.entityId,
                    OwnerQuest.QuestTags,
                    OwnerQuest.QuestCode,
                    NetPackageQuestGotoPoint.QuestGotoTypes.RandomPOI,
                    OwnerQuest.QuestClass.DifficultyTier,
                    0,
                    -1,
                    0f,
                    0f,
                    0f,
                    -1f,
                    biomeFilterType,
                    biomeFilter),
                false);

            CurrentValue = 1;
        }
        return Vector3.zero;
    }

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
        var objectiveRandomPOIGoto = new ObjectiveRandomTaggedPOIGotoSDX();
        CopyValues(objectiveRandomPOIGoto);
        return objectiveRandomPOIGoto;
    }

    /// <summary>
    /// Copies the values from another Objective.
    /// </summary>
    /// <param name="objective"></param>
    protected override void CopyValues(BaseObjective objective)
    {
        base.CopyValues(objective);
        ObjectiveRandomTaggedPOIGotoSDX obj = (ObjectiveRandomTaggedPOIGotoSDX)objective;
        obj.IncludeTags = IncludeTags;
        obj.ExcludeTags = ExcludeTags;
        obj.MaxSearchDistance = MaxSearchDistance;
        obj.MinSearchDistance = MinSearchDistance;
    }

    /// <summary>
    /// Copied verbatim from ObjectiveRandomPOIGoto, made protected so any future subclasses won't
    /// have to do the same thing.
    /// </summary>
    /// <param name="poiSize"></param>
    protected void SetDistanceOffset(Vector3 poiSize)
    {
        if (poiSize.x > poiSize.z)
        {
            distanceOffset = poiSize.x;
            return;
        }
        distanceOffset = poiSize.z;
    }
}