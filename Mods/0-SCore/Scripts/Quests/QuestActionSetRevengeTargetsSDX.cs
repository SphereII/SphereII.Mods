using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>
/// This quest objective sets the revenge targets of all entities in range.
/// </para>
/// 
/// <para>
/// The revenge targets can be set to either the owner of the quest, or to a random member of the
/// quest owner's party which shared the quest. Party members must be in range of the quest.
/// This is set in the "id" attribute. If omitted, defaults to the quest owner only.
/// </para>
/// 
/// <para>
/// The value attribute is the distance, and can be either a number or the string "location".
/// If a number, the distance is the diameter (not radius!) around the player, in blocks.
/// If the string "location", the distance is size of the quest location (usually the POI).
/// If omitted, or if the value is "location" and it can't get the quest location,
/// it defaults to a value of 15 blocks.
/// </para>
/// 
/// <para>
/// Revenge targets will be set on all living entities in range, except for player entities,
/// entities that the player has hired, or the hires of other players that are allies.
/// Only entities that are awake will have their revenge targets set.
/// </para>
/// 
/// <example>
/// This sets the revenge targets of all entities within a 20 block area,
/// to the quest owner,
/// and does so in phase 4 of the quest.
/// <code>
///     &lt;action type="SetRevengeTargetsSDX" id="owner" value="20" phase="4" />
/// </code>
/// </example>
/// 
/// <example>
/// This sets the revenge targets of all entities within the quest location (POI),
/// randomly to either the quest owner or a member of the owner's party that shared the quest,
/// and does so in phase 2 of the quest.
/// <code>
///     &lt;action type="SetRevengeTargetsSDX" id="party" value="location" phase="2" />
/// </code>
/// </example>
/// 
/// <example>
/// This example uses defaults.
/// This sets the revenge targets of all entities within a 20 block area,
/// to the quest owner,
/// and does so in phase 1 of the quest.
/// <code>
///     &lt;action type="SetRevengeTargetsSDX" phase="1" />
/// </code>
/// </example>
/// </summary>
public class QuestActionSetRevengeTargetsSDX : BaseQuestAction
{
    /// <summary>
    /// The default distance to use if none is provided.
    /// </summary>
    public const float DEFAULT_DISTANCE = 15f;

    /// <summary>
    /// ID value to use when setting the revenge targets to the quest owner (only).
    /// </summary>
    public const string OWNER_ID = "owner";

    /// <summary>
    /// ID value to use when setting the revenge targets to either the quest owner, or a random
    /// member of the quest owner's party who shared the quest.
    /// </summary>
    public const string SHARED_ID = "party";

    /// <summary>
    /// The number of "ticks" that the entities should keep the player as their revenge target.
    /// A "tick" happens each time <see cref="EntityAlive.OnUpdateEntity"/> is called.
    /// If you do not set it after calling <see cref="EntityAlive.SetRevengeTarget(EntityAlive)"/>,
    /// the default value is 500.
    /// </summary>
    public const int REVENGE_TICKS = 5000;

    /// <inheritdoc/>
    public override BaseQuestAction Clone()
    {
        var questAction = new QuestActionSetRevengeTargetsSDX();
        CopyValues(questAction);
        return questAction;
    }

    /// <inheritdoc/>
    public override void PerformAction(Quest ownerQuest)
    {
        var targetEntities = GetTargetEntities(ownerQuest);
        if (targetEntities == null)
            return;

        var entitiesInRange = GetEntitiesInRange(ownerQuest);
        if (entitiesInRange == null)
            return;

        foreach (var entity in entitiesInRange)
        {
            if (entity is EntityPlayer)
                continue;

            // Don't set the revenge targets of entities hired by the quest owner,
            // or by entities hired by members of the quest owner's party
            if (EntityTargetingUtilities.IsAllyOfParty(entity, ownerQuest.OwnerJournal.OwnerPlayer))
                continue;

            var target = GetRandomTarget(targetEntities);
            entity.SetRevengeTarget(target);
            entity.SetRevengeTimer(REVENGE_TICKS);
        }
    }

    private Bounds GetBounds(Quest ownerQuest)
    {
        float distance = DEFAULT_DISTANCE;
        Vector3 center = ownerQuest.OwnerJournal.OwnerPlayer.position;

        if ("location".EqualsCaseInsensitive(Value))
        {
            var size = ownerQuest.GetLocationSize();
            if (size != Vector3.zero)
            {
                // The center should be the center of the POI, not the player position
                var location = ownerQuest.GetLocation();

                center = new Vector3(
                    location.x + size.x / 2f,
                    location.y + size.y / 2f,
                    location.z + size.z / 2f);

                // The bounds constructor shrinks the size vector in half, compensate here
                return new Bounds(center, 2f * size);
            }
        }
        else if (!string.IsNullOrEmpty(Value) && !float.TryParse(Value, out distance))
        {
            return default;
        }

        return new Bounds(center, new Vector3(distance, distance, distance));
    }

    private IEnumerable<EntityAlive> GetEntitiesInRange(Quest ownerQuest)
    {
        var bounds = GetBounds(ownerQuest);
        if (bounds == default)
            return null;

        return GameManager.Instance.World.GetLivingEntitiesInBounds(
            ownerQuest.OwnerJournal.OwnerPlayer,
            bounds);
    }

    private EntityAlive GetRandomTarget(EntityAlive[] targetEntities)
    {
        var index = GameManager.Instance.World.GetGameRandom().RandomRange(targetEntities.Length);

        return targetEntities[index];
    }

    private EntityAlive[] GetTargetEntities(Quest ownerQuest)
    {
        var targetEntities = new List<EntityAlive>() { ownerQuest.OwnerJournal.OwnerPlayer };

        if (ID == null || ID.EqualsCaseInsensitive(OWNER_ID))
            return targetEntities.ToArray();

        if (ID.EqualsCaseInsensitive(SHARED_ID))
        {
            var shared = ownerQuest.GetSharedWithIDList();
            if (shared == null)
                return targetEntities.ToArray();

            var notInRange = ownerQuest.GetSharedWithListNotInRange();

            foreach (var entityid in shared)
            {
                var entity = GameManager.Instance.World.GetEntity(entityid) as EntityPlayer;
                if ( entity == null ) continue;
                if (notInRange != null && notInRange.Contains(entity))
                    continue;

                targetEntities.Add(entity);
            }
            return targetEntities.ToArray();
        }

        // "owner" and "party" are the only allowed ID values
        return null;
    }
    
}
