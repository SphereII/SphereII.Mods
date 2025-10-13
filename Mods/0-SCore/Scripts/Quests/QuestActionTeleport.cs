using UnityEngine;

/*

  <action type="Teleport, SCore">
    <!-- only one of the following 4 options. If all are omitted, the SouthWest corner will be used. -->

    <!-- First instance of this block by name is used. -->
    <property name="block" value="radioHam" />

    <!-- this for be the <property name="Tags" value="whatever" /> on the block. -->
    <property name="block_tag" value="bed_tag" />

    <!-- if this exists, it'll use the activate marker -->
    <property name="questmarker" value="doesntmatter" />

    <!-- if set, this will be the offset added to the SW position -->
    <property name="offset" value="28,2,45" />

   </action>
 */
/// <summary>
/// A quest action that teleports the player to a specified location.
/// The location is determined by a priority system:
/// 1. The position of a specific block by name or tag.
/// 2. A vector offset from the quest's POI position.
/// 3. A quest marker position.
/// 4. The default quest POI position.
/// </summary>
public class QuestActionTeleport : BaseQuestAction
{
    private string block;
    private string blockTag; // Renamed from block_tag for C# naming conventions
    private string offset;
    private string questMarker;

    public override void SetupAction()
    {
    }

    public override void PerformAction(Quest ownerQuest)
    {
        // First, determine the correct position based on the defined properties.
        if (TryGetTeleportPosition(ownerQuest, out Vector3 targetPosition))
        {
            var player = ownerQuest.OwnerJournal.OwnerPlayer;
            player.Teleport(targetPosition);
        }
    }

    /// <summary>
    /// Tries to calculate the target teleport position based on a priority of conditions.
    /// </summary>
    /// <returns>True if a valid position was determined.</returns>
    private bool TryGetTeleportPosition(Quest ownerQuest, out Vector3 position)
    {
        // Start with the default POI position as a fallback.
        ownerQuest.GetPositionData(out position, Quest.PositionDataTypes.POIPosition);

        var prefab = GameManager.Instance.GetDynamicPrefabDecorator().GetPrefabAtPosition(position);
        if (prefab == null)
        {
            Log.Out($"No Quest POI set yet for :{ownerQuest.ID} :: Do you need to set a delay for the teleport?");
            return false;
        }

        if (TryGetBlockPosition(prefab, out Vector3 blockPosition))
        {
            position = blockPosition;
        }

        if (!string.IsNullOrEmpty(questMarker))
        {
            ownerQuest.GetPositionData(out position, Quest.PositionDataTypes.Activate);
        }

        if (!string.IsNullOrEmpty(offset))
        {
            var posOffset = StringParsers.ParseVector3(offset);
            if (posOffset == Vector3.zero)
            {
                GameManager.Instance.World.GetRandomSpawnPositionMinMaxToPosition(position, 1, 2, -1, false, out posOffset);
            }
            position += posOffset;
        }

        return true;
    }

    /// <summary>
    /// Scans a prefab instance to find the world position of a block matching a name or tag.
    /// </summary>
    /// <returns>True if a matching block was found.</returns>
    private bool TryGetBlockPosition(PrefabInstance prefabInstance, out Vector3 position)
    {
        position = Vector3.zero;
        bool hasBlockName = !string.IsNullOrEmpty(block);
        bool hasBlockTag = !string.IsNullOrEmpty(blockTag);

        if (!hasBlockName && !hasBlockTag) return false;

        var prefabSize = prefabInstance.prefab.size;
        var prefabPos = prefabInstance.boundingBoxPosition;
        var world = GameManager.Instance.World;

        // Parse the tag once, outside the expensive triple-loop.
        var tagToFind = hasBlockTag ? FastTags<TagGroup.Global>.Parse(blockTag) : FastTags<TagGroup.Global>.none;

        for (var y = 0; y < prefabSize.y; y++)
        {
            for (var x = 0; x < prefabSize.x; x++)
            {
                for (var z = 0; z < prefabSize.z; z++)
                {
                    var worldX = x + prefabPos.x;
                    var worldY = y + prefabPos.y;
                    var worldZ = z + prefabPos.z;

                    var currentBlockValue = world.GetBlock(worldX, worldY, worldZ);
                    if (currentBlockValue.isair) continue;

                    var currentBlock = currentBlockValue.Block;
                    bool nameMatches = hasBlockName && currentBlock.GetBlockName().EqualsCaseInsensitive(block);
                    bool tagMatches = hasBlockTag && currentBlock.HasAnyFastTags(tagToFind);

                    if (nameMatches || tagMatches)
                    {
                        position = new Vector3(worldX, worldY, worldZ);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    public override BaseQuestAction Clone()
    {
        var questAction = new QuestActionTeleport();
        CopyValues(questAction);
        questAction.blockTag = blockTag;
        questAction.block = block;
        questAction.offset = offset;
        questAction.questMarker = questMarker;
        return questAction;
    }

    public override void ParseProperties(DynamicProperties properties)
    {
        base.ParseProperties(properties);
        properties.ParseString("block", ref block);
        properties.ParseString("offset", ref offset);
        // The property key is "block_tag", but our internal variable is "blockTag".
        properties.ParseString("block_tag", ref blockTag);
        properties.ParseString("questmarker", ref questMarker);
    }
}