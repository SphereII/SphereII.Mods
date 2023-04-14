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
    /// If the entity has a cvar of this name, get a relationship value from the cvar, and use it
    /// as the faction relationship value for damage. The faction relationship must be
    /// <em>strictly below</em> this value, or the enity cannot damage it.
    /// </summary>
    public static readonly string DamageRelationshipCVarName = "DamageRelationship";

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
        if (target.IsDead()) return false;

        // Enemy animals can not follow these rules. We also can't use CanDamageEntity here,
        // because that has a Harmony patch that calls this method again. Instead, reproduce the
        // logic in EntityEnemyAnimal.CanDamageEntity.
        if (IsEnemyInAnimalsFaction(self))
            return target != null && target.entityClass != self.entityClass;

        // Don't damage vehicles if they are immune.
        if (IsDamageImmuneVehicle(self, target))
            return false;

        // Don't damage your leader or fellow followers.
        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
        if (IsAllyOfLeader(myLeader, target))
            return CanDamageAlly(self, target);

        // If two players are involved (directly or as leaders), determine whether they or their
        // followers can damage each other from the "Player Killing" setting.
        var selfPlayer = GetPlayerLeader(self, myLeader);
        var targetPlayer = GetPlayerLeader(target);
        if (selfPlayer != null && targetPlayer != null)
        {
            // FriendlyFireCheck returns true if the players can damage each other
            return selfPlayer.FriendlyFireCheck(targetPlayer);
        }

        // If you are a player, don't damage your followers. Otherwise, use factions.
        if (self is EntityPlayer)
        {
            if (IsAlly(target, self))
                return CanDamageAlly(target, self);

            return !IsFriendlyFireByFaction(self, target);
        }

        // You can always damage your revenge target, even if it's a player (since they hit first).
        if (IsCurrentRevengeTarget(self, target))
            return true;

        // Otherwise, if the target is a player, you can only damage them if you or your
        // (not necessarily player) leader hate them.
        if (target is EntityPlayer)
        {
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
        if (self == null) return true;

        // If it's dead, offer no such protection.
        if (self.IsDead()) return true;

        // If the damage was not caused by a living entity, take that damage.
        if (!(damagingEntity is EntityAlive livingEntity))
            return true;

        return CanDamage(livingEntity, self);
    }

    /// <summary>
    /// Gets the default faction relationship that represents friendly fire (without taking the
    /// cvar into account).
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static float GetFriendlyFireRelationship(Entity entity)
    {
        // Don't damage neutral NPCs. This is to avoid cases where the player or another NPC steps
        // in to help neutral human NPCs fighting an enemy - we don't want them to hurt that NPC.
        if (entity is EntityNPC)
            return (float)FactionManager.Relationship.Neutral;

        // For all other NPCs (animals, etc.) attackers can damage them if they don't love them.
        return (float)FactionManager.Relationship.Love;
    }

    /// <summary>
    /// Returns true if the target is a vehicle that is immune to damage from the checking entity.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsDamageImmuneVehicle(Entity self, Entity target)
    {
        if (!(target is EntityVehicle))
            return false;

        if (self is EntityNPC)
        {
            // For now, just return true.
            // In the future we may have different rules, depending upon your entity class
            // (EntityBandit, EntityDrone); if the vehicle is owned by your leader; if it's
            // owned by a rival player depending upon the "Player Killing" setting; etc.
            return true;
        }

        return false;
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
        if (self == null || targetEntity == null) return false;

        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);

        return IsAllyOfLeader(myLeader, targetEntity);
    }

    /// <summary>
    /// Determines whether yourself and the target entity are in allied parties.
    /// You are in allied parties if:
    /// <list type="bullet">
    /// <item>the target is your leader</item>
    /// <item>you are the target's leader</item>
    /// <item>
    /// you and your target both have player leaders, and those players can't damage each other
    /// according to the "Player Killing" setting
    /// </item>
    /// </list>
    /// Factions are not considered.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="targetEntity"></param>
    /// <returns></returns>
    public static bool IsAllyOfParty(Entity self, Entity targetEntity)
    {
        if (self == null || targetEntity == null)
            return false;

        // Don't assume either entity's leader is a player for this check
        var selfLeader = EntityUtilities.GetLeaderOrOwner(self.entityId) ?? self;
        var targetLeader = EntityUtilities.GetLeaderOrOwner(targetEntity.entityId) ?? targetEntity;

        if (selfLeader.entityId == targetLeader.entityId)
            return true;

        // FriendlyFireCheck returns true if friendly fire is allowed
        if (selfLeader is EntityPlayer selfPlayer && targetLeader is EntityPlayer targetPlayer)
            return !selfPlayer.FriendlyFireCheck(targetPlayer);

        return false;
    }

    /// <summary>
    /// Returns true if you consider the target to be your enemy.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsEnemy(EntityAlive self, Entity target)
    {
        if (self == null) return true;

        if (!(target is EntityAlive targetEntity))
            return false;

        if (targetEntity.IsDead())
            return false;

        // Don't start fights with vehicles.
        if (IsDamageImmuneVehicle(self, target))
            return false;

        // Don't make enemies out of your followers, your leader, or fellow followers.
        var myLeader = EntityUtilities.GetLeaderOrOwner(self.entityId);
        if (IsAllyOfLeader(myLeader ?? self, target))
            return false;

        // If two players are involved (directly or as leaders), determine whether they or their
        // followers can damage each other from the "Player Killing" setting.
        // This is to make sure damage-immune entities are NOT enemies - the reverse is not true.
        // Just because you can damage them does not make them enemies.
        var selfPlayer = GetPlayerLeader(self, myLeader);
        var targetPlayer = GetPlayerLeader(target);
        // FriendlyFireCheck returns true if the players can damage each other
        if (selfPlayer != null && targetPlayer != null && !selfPlayer.FriendlyFireCheck(targetPlayer))
            return false;

        // Our current revenge target is always an enemy.
        if (IsCurrentRevengeTarget(self, target))
            return true;

        // If they are fighting my leader or allies, they're an enemy.
        if (IsFightingFollowers(myLeader, target))
            return true;

        // If it's an enemy animal, we can't use faction targeting. Instead, test to see if it's
        // the attack target. (We considered the revenge target, above.)
        if (IsEnemyInAnimalsFaction(self))
        {
            var attackTarget = self.GetAttackTarget();
            return attackTarget != null && attackTarget.entityId == target.entityId;
        }

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
    /// your leader, allies (those who share a leader), entities in "loved" factions
    /// (including members of your own faction, if not overridden by your leader),
    /// players who are immune to friendly fire from you or your leader, and the followers
    /// of those players.
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

        // If two players are involved (directly or as leaders), determine whether they and their
        // followers are freinds from the "Player Killing" setting.
        var selfPlayer = GetPlayerLeader(self, myLeader);
        var targetPlayer = GetPlayerLeader(target);
        if (selfPlayer != null && targetPlayer != null)
        {
            // FriendlyFireCheck returns true if the players can damage each other
            return !selfPlayer.FriendlyFireCheck(targetPlayer);
        }

        // Don't consider revenge targets to be our friends.
        if (IsCurrentRevengeTarget(self, target))
            return false;

        // Targets who are attacking our allies are not friends.
        if (IsFightingFollowers(myLeader, target))
            return false;

        // They are a friend if we love them, or our leader loves them.
        return myLeader == null
            ? IsFriendlyFireByFaction(self, target)
            : IsFriendlyFireByFaction(myLeader, target);
    }

    /// <summary>
    /// Determines whether the target is an enemy, according to faction relationship.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="target"></param>
    /// <returns></returns>
    public static bool IsEnemyByFaction(Entity self, Entity target)
    {
        if (!(self is EntityAlive livingSelf && target is EntityAlive livingTarget))
            return false;

        var relationship = EntityUtilities.GetFactionRelationship(livingSelf, livingTarget);

        return relationship < (int)FactionManager.Relationship.Dislike;
    }

    /// <summary>
    /// Returns true if the entity is an enemy type that is using the vanilla "animals" faction.
    /// That faction is neutral to all, so cannot target enemies based on faction standing.
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static bool IsEnemyInAnimalsFaction(Entity entity)
    {
        if (!(entity is EntityEnemy enemy))
            return false;

        var faction = FactionManager.Instance.GetFaction(enemy.factionId);
        return faction != null && faction.Name == "animals";
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
        if (!(self is EntityAlive livingSelf && target is EntityAlive livingTarget))
            return false;

        // If we have the "damage relationship" cvar, use that value. Otherwise, use the default.
        float damageRelationship = GetFriendlyFireRelationship(target);

        if (livingSelf.Buffs.HasCustomVar(DamageRelationshipCVarName))
        {
            var buffValue = livingSelf.Buffs.GetCustomVar(DamageRelationshipCVarName);
            // GetCustomVar also sets the custom var to zero if it doesn't exist; check that.
            if (buffValue > 0)
                damageRelationship = buffValue;
        }

        var relationship = EntityUtilities.GetFactionRelationship(livingSelf, livingTarget);
        return relationship >= damageRelationship;
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
    /// Private helper method to get a player who is either yourself or your leader.
    /// Will return null if neither yourself nor your leader (if any) is a player.
    /// </summary>
    /// <param name="self"></param>
    /// <returns></returns>
    private static EntityPlayer GetPlayerLeader(Entity self)
    {
        return GetPlayerLeader(self, EntityUtilities.GetLeaderOrOwner(self.entityId));
    }

    /// <summary>
    /// Private helper method to get a player who is either yourself or your leader.
    /// Will return null if neither yourself nor your leader is a player.
    /// This method should be used when you already have an object representing your leader,
    /// as it avoids a call to GetLeaderOrOwner.
    /// </summary>
    /// <param name="self"></param>
    /// <param name="leader"></param>
    /// <returns></returns>
    private static EntityPlayer GetPlayerLeader(Entity self, Entity leader)
    {
        if (self is EntityPlayer playerSelf)
            return playerSelf;

        return leader as EntityPlayer;
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
