using GamePath;
using Platform;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using UAI;
using UnityEngine;

public static class EntityTargetingUtilities
{
    /// <summary>
    /// <para>
    /// Determines whether you can damage the target.
    /// </para>
    /// <para>
    /// This is not necessarily a symmetrical relationship. If you cannot damage the target,
    /// it does not mean the target cannot damage you.
    /// </para>
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool CanDamage(EntityAlive self, Entity target)
    {
        // Don't damage your leader or fellow followers.
        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
        if (IsAllyOfLeader(myLeader, target))
            return CanDamageAlly(self, target);

        // If you are a player, don't damage your followers, or the followers of unkillable players
        // (as defined by the "Player Killing" setting). Everyone else is on the table.
        if (self is EntityPlayer)
        {
            if (IsAlly(target, self))
                return CanDamageAlly(target, self);

            return !IsPlayerFriendlyFire(target, self);
        }

        // You can always damage your revenge target, even if it's a player (since they hit first).
        if (IsCurrentRevengeTarget(self, target))
            return true;

        // Otherwise, if the target is a player, you can only damage them if your player leader can
        // kill them, or if you or your (not necessarily player) leader hate them.
        if (target is EntityPlayer)
        {
            if (IsPlayerFriendlyFire(self, target))
                return false;

            return myLeader == null
                ? IsEnemyByFaction(self, target)
                : IsEnemyByFaction(myLeader, target);
        }

        // You can damage them if they are fighting your leader or allies.
        if (IsFightingFollowers(myLeader, target))
            return true;

        // If you have a leader, check friendly fire using their faction.
        // In all other cases, use the faction relationship between yourself and the target.
        return myLeader == null
            ? !IsFriendlyFireByFaction(self, target)
            : !IsFriendlyFireByFaction(myLeader, target);
    }

    /// <summary>
    /// Determines whether you can damage your ally. You are allies if the target is your
    /// leader, or you and your target have the same leader.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool CanDamageAlly(Entity self, Entity target)
    {
        // For now, just return false.
        // In the future, we may change this determination according to the "Player Killing"
        // settings - for example, if set to "Kill Everyone," we may return true.
        return false;
    }

    /// <summary>
    /// This method checks to see if damage, presumably caused by another entity,
    /// is allowed to actually do damage to the checking entity.
    /// </summary>
    /// <param name="self">The entity that is checking to see if it can be damaged.</param>
    /// <param name="damagingEntity">The entity causing the damage, if any.</param>
    /// <returns></returns>
    public static bool CanTakeDamage(EntityAlive self, Entity damagingEntity)
    {
        // If the damage was not caused by a living entity, take that damage.
        if (!(damagingEntity is EntityAlive livingEntity))
            return true;

        return CanDamage(livingEntity, self);
    }

    /// <summary>
    /// <para>
    /// Determines whether yourself and the target entity are allies.
    /// You are allies if the target is your leader, or you and your target have the same leader.
    /// </para>
    /// 
    /// <para>
    /// This does <em>not</em> assume a reciprocal relationship.
    /// If you are the leader of the target, this will return false.
    /// </para>
    /// </summary>
    /// <param name="self"></param>
    /// <param name="targetEntity"></param>
    /// <returns></returns>
    public static bool IsAlly(Entity self, Entity targetEntity)
    {
        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);

        return IsAllyOfLeader(myLeader, targetEntity);
    }

    /// <summary>
    /// Returns true if you consider the target to be your enemy.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsEnemy(EntityAlive self, Entity target)
    {
        if (!(target is EntityAlive targetEntity))
            return false;

        if (targetEntity.IsDead())
            return false;

        // If we can't even damage it, no sense considering it an enemy.
        if (!CanDamage(self, target))
            return false;

        // Our current revenge target is always an enemy.
        if (IsCurrentRevengeTarget(self, target))
            return true;

        // If they are fighting my leader or allies, they're an enemy.
        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
        if (IsFightingFollowers(myLeader, target))
            return true;

        // They are an enemy if we hate them, or if our leader hates them.
        return myLeader == null
            ? IsEnemyByFaction(self, target)
            : IsEnemyByFaction(myLeader, target);
    }

    /// <summary>
    /// Returns true if the target is fighting a leader or one of their followers.
    /// </summary>
    /// <param name="leader">Entity representing a leader.</param>
    /// <param name="target">Target entity.</param>
    /// <returns></returns>
    public static bool IsFightingFollowers(Entity leader, Entity target)
    {
        if (leader == null || target == null)
            return false;

        var theirTarget = GetAggressionTarget(target);
        if (theirTarget != null)
        {
            // Are they fighting the leader?
            if (theirTarget.entityId == leader.entityId)
                return true;

            // Are they fighting the leader's followers?
            var theirTargetLeader = EntityUtilities.GetLeaderOrOwner(theirTarget.entityId);
            if (theirTargetLeader != null && theirTargetLeader.entityId == leader.entityId)
                return true;
        }

        // Is the leader fighting them?
        var leaderTarget = GetAggressionTarget(leader);
        return leaderTarget != null && leaderTarget.entityId == target.entityId;
    }

    /// <summary>
    /// Tests to see if the target entity is a friend. A "friend" is defined as yourself,
    /// your leader, allies (those who share a leader), and entities in "loved" factions
    /// (including members of your own faction, if not overridden by your leader).
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsFriend(EntityAlive self, Entity target)
    {
        if (!(target is EntityAlive targetEntity))
            return false;

        if (targetEntity.IsDead())
            return false;

        if (self.entityId == target.entityId)
            return true;

        // Note: We can't use CanDamage here, because depending upon future features,
        // it might be possible to damage your friends too.

        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
        if (IsAllyOfLeader(myLeader, target) || IsAlly(target, self))
            return true;

        if (IsPlayerFriendlyFire(self, target))
            return true;

        // Don't consider revenge targets to be our friends.
        if (IsCurrentRevengeTarget(self, target))
            return false;

        // Targets who are attacking our allies are not friends.
        if (IsFightingFollowers(myLeader, target))
            return false;

        // They are a friend if we love them, or our leader loves them.
        return myLeader == null
            ? IsEnemyByFaction(self, target)
            : IsEnemyByFaction(myLeader, target);
    }

    /// <summary>
    /// <para>
    /// Determines if attacking the target would constitute player friendly fire,
    /// as defined by the "Player Killing" setting in the "Multiplayer" tab.
    /// </para>
    /// <para>
    /// It handles cases where you or the target are players, or have leaders who are players.
    /// If no players are involved, it returns false.
    /// </para>
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsPlayerFriendlyFire(Entity self, Entity target)
    {
        EntityPlayer player = null;
        EntityAlive other = target as EntityAlive;

        if (self is EntityPlayer us)
        {
            player = us;
        }
        else if (EntityUtilities.GetLeaderOrOwner(self.entityId) is EntityPlayer ourLeader)
        {
            player = ourLeader;
        }

        if (EntityUtilities.GetLeaderOrOwner(target.entityId) is EntityPlayer theirLeader)
        {
            other = theirLeader;
        }

        return player != null
            // FriendlyFireCheck returns true if it _fails_ the friendly fire check
            && !player.FriendlyFireCheck(other);
    }

    /// <summary>
    /// Determines whether the target is an enemy, according to faction relationship.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsEnemyByFaction(Entity self, Entity target)
    {
        var relationship = EntityUtilities.GetFactionRelationship(
            self as EntityAlive,
            target as EntityAlive);

        return relationship < (int)FactionManager.Relationship.Dislike;
    }

    /// <summary>
    /// Determines whether you attacking the target would constitute friendly fire, according
    /// to faction relationship.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsFriendlyFireByFaction(Entity self, Entity target)
    {
        // For now, we are just returning whether the two entities do not "Love" each other.
        // (Members of the same faction love each other.)
        // In the future, we may change this determination according to the "Player Killing"
        // settings - for example, if set to "Kill Everyone," we may just return false.

        var relationship = EntityUtilities.GetFactionRelationship(
            self as EntityAlive,
            target as EntityAlive);

        return relationship >= (int)FactionManager.Relationship.Love;
    }

    /// <summary>
    /// Returns true if the target is your current revenge target, and you do not forgive it.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsCurrentRevengeTarget(EntityAlive self, Entity target)
    {
        if (target == null || self == null)
            return false;

        var revengeTarget = self.GetRevengeTarget();
        if (revengeTarget == null)
            return false;

        return revengeTarget.entityId == target.entityId
            && !ShouldForgiveDamage(self, target);
    }

    /// <summary>
    /// Determines whether you should <em>immediately</em> forgive any damage that is taken from
    /// the target entity, so they are never considered a revenge target.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool ShouldForgiveDamage(EntityAlive self, Entity target)
    {
        // For now, only forgive damage from your leader or other followers.
        // In the future, we may do something different according to the "Player Killing" setting
        // in the "Multiplayer" tab.
        return IsAlly(self, target);
    }

    /// <summary>
    /// Private helper method to check if a target is an ally of a leader.
    /// This is here mainly to prevent repeated calls to GetLeaderOrOwner.
    /// </summary>
    /// <param name="leader"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    private static bool IsAllyOfLeader(Entity leader, Entity target)
    {
        if (leader == null || target == null)
            return false;

        if (target.entityId == leader.entityId)
            return true;

        var targetLeader = EntityUtilities.GetLeaderOrOwner(target.entityId);
        if (targetLeader == null)
            return false;

        return targetLeader.entityId == leader.entityId;
    }

    /// <summary>
    /// <para>
    /// Private helper method to get the target of aggression, depending upon whether the targeting
    /// entity is a player or an NPC.
    /// </para>
    /// 
    /// <para>
    /// This is necessary because we shouldn't use the "attack target" of NPCs. There are many
    /// situations where this is automatically set to the player ("attack" sleeper volumes, quest
    /// spawns, etc.) even if they aren't aggressive to the player. Also, some entities (like the
    /// drone) use the "attack" target for non-aggressive tasks (like healing). So for NPCs, we
    /// should only use their revenge target, which is always the entity that damaged them.
    /// </para>
    /// 
    /// <para>
    /// On the other hand, if the entity is a player, they only have an attack target if the player
    /// voluntarily initiated an attack. They may also have a revenge target. We can use either,
    /// but the revenge target takes priority.
    /// </para>
    /// </summary>
    /// <param name="attacker"></param>
    /// <returns></returns>
    private static Entity GetAggressionTarget(Entity attacker)
    {
        if (attacker is EntityPlayer player)
            return player.GetRevengeTarget() ?? player.GetAttackTarget();

        if (attacker is EntityAlive entityAlive)
            return entityAlive.GetRevengeTarget();

        return null;
    }
}
