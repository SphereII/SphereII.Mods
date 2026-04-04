using Audio;
using UnityEngine;

public partial class EntityAliveSDXV4
{
    // =========================================================================
    // Death / unload
    // =========================================================================

    public override void SetDead()
    {
        for (int i = 0; i < _components.Length; i++)
            _components[i].OnDead();

        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayerLocal;
        if (leader != null)
        {
            leader.Buffs.RemoveCustomVar($"hired_{entityId}");
            EntityUtilities.SetLeaderAndOwner(entityId, 0);
            GameManager.ShowTooltip(leader, $"Oh no! {EntityName} has died. :(");

            if (lootContainer != null && !lootContainer.IsEmpty())
            {
                var bagPos     = new Vector3i(position + transform.up);
                var className  = EntityClass.GetEntityClass("BackpackNPC".GetHashCode()) != null
                                 ? "BackpackNPC" : "Backpack";
                var backpack   = EntityFactory.CreateEntity(className.GetHashCode(), bagPos) as EntityItem;
                var creationData = new EntityCreationData(backpack)
                {
                    entityName    = Localization.Get(EntityName),
                    id            = -1,
                    lootContainer = lootContainer
                };
                GameManager.Instance.RequestToSpawnEntityServer(creationData);
                backpack?.OnEntityUnload();
            }
        }

        // leader is EntityPlayerLocal which extends EntityPlayer — cast is always valid here
        leader?.Companions.Remove(this);

        bWillRespawn = false;
        if (NavObject != null)
        {
            NavObjectManager.Instance.UnRegisterNavObject(NavObject);
            NavObject = null;
        }

        SetupDebugNameHUD(false);
        lootContainer   = null;
        lootListAlive   = null;
        lootListOnDeath = null;
        isCollided      = false;
        nativeCollider  = null;
        physicsCapsuleCollider = null;

        foreach (var col in gameObject.GetComponentsInChildren<Collider>())
        {
            if (col.name.ToLower() == "gameobject")
                col.enabled = false;
        }

        base.SetDead();
    }

    public override void OnEntityUnload()
    {
        for (int i = 0; i < _components.Length; i++)
            _components[i].OnUnload();

        var leader = EntityUtilities.GetLeaderOrOwner(entityId) as EntityPlayer;
        leader?.Companions.Remove(this);
        base.OnEntityUnload();
    }

    public override void MarkToUnload()
    {
        GameManager.Instance.World.ChunkClusters[0].OnChunkVisibleDelegates -= _chunkClusterVisibleDelegate;
        base.MarkToUnload();
    }

    public override bool IsSavedToFile()
    {
        if (Buffs.HasCustomVar("Leader"))  return true;
        if (Buffs.HasCustomVar("Persist")) return true;
        if (GetSpawnerSource() == EnumSpawnerSource.Dynamic) return false;
        if (GetSpawnerSource() == EnumSpawnerSource.Biome)   return false;
        return true;
    }

    // =========================================================================
    // Nav object — duplicate check removed
    // =========================================================================

    public override void HandleNavObject()
    {
        var navObjectName = EntityClass.list[entityClass].NavObject;
        if (string.IsNullOrEmpty(navObjectName)) return;

        if (LocalPlayerIsOwner() && Owner != null)
        {
            NavObject = NavObjectManager.Instance.RegisterNavObject(navObjectName, this, "");
            NavObject.UseOverrideColor = true;
            NavObject.OverrideColor    = Color.cyan;
            NavObject.name             = EntityName;
            return;
        }

        if (NavObject != null)
        {
            NavObjectManager.Instance.UnRegisterNavObject(NavObject);
            NavObject = null;
        }
    }

    public bool LocalPlayerIsOwner()
    {
        var leader = EntityUtilities.GetLeaderOrOwner(entityId);
        return leader != null && GameManager.Instance.World.IsLocalPlayer(leader.entityId);
    }

    // =========================================================================
    // Miscellaneous overrides (behaviour identical to EntityAliveSDX)
    // =========================================================================

    public override void SetModelLayer(int _layerId, bool _force = false, string[] excludeTags = null) { }

    public override void UpdateJump()
    {
        if (walkType == 4 && !isSwimming)
        {
            base.FaceJumpTo();
            jumpState = JumpState.Climb;
            if (!emodel.avatarController || !emodel.avatarController.IsAnimationJumpRunning())
                Jumping = false;
            if (jumpTicks == 0 && accumulatedRootMotion.y > 0.005f)
                jumpTicks = 30;
            return;
        }
        base.UpdateJump();
        if (!isSwimming) accumulatedRootMotion.y = 0f;
    }

    public override float getNextStepSoundDistance() => !IsRunning ? 0.5f : 0.25f;

    public override void PlayStepSound(string stepSound, float volume)
    {
        if (IsOnMission()) return;
        if (HasAnyTags(FastTags<TagGroup.Global>.Parse("floating"))) return;
        base.PlayStepSound(stepSound, volume);
    }

    public override void PlayOneShot(string clipName, bool sound_in_head = false, bool netsync = true,
        bool isUnique = false, AnimationEvent _animEvent = null)
    {
        if (IsOnMission()) return;
        base.PlayOneShot(clipName, sound_in_head);
    }

    public override void AwardKill(EntityAlive killer)
    {
        if (killer != null && killer != this)
        {
            var ep = killer as EntityPlayer;
            if (ep != null && !ep.isEntityRemote)
                SCoreQuestEventManager.Instance.EntityAliveKilled(EntityClass.list[entityClass].entityClassName);
        }
        base.AwardKill(killer);
    }

    public override void OnEntityDeath()
    {
        Log.Out($"{entityName} ({entityId}) has died.");
        foreach (var buff in Buffs.ActiveBuffs) Log.Out($" > {buff.BuffName}");
        base.OnEntityDeath();
    }

    public override void dropItemOnDeath()
    {
        if (entityThatKilledMe)
            lootDropProb = EffectManager.GetValue(PassiveEffects.LootDropProb,
                entityThatKilledMe.inventory.holdingItemItemValue, lootDropProb, entityThatKilledMe);
        if (lootDropProb > rand.RandomFloat)
            GameManager.Instance.DropContentOfLootContainerServer(BlockValue.Air, new Vector3i(position), entityId);
    }

    public override void OnDeathUpdate()
    {
        if (deathUpdateTime < timeStayAfterDeath) deathUpdateTime++;

        int deadBodyHP = EntityClass.list[entityClass].DeadBodyHitPoints;
        if (deadBodyHP > 0 && DeathHealth <= -deadBodyHP)
            deathUpdateTime = timeStayAfterDeath;

        if (deathUpdateTime != timeStayAfterDeath) return;

        if (!isEntityRemote && !markedForUnload)
        {
            dropCorpseBlock();
            if (!string.IsNullOrEmpty(_particleOnDestroy))
            {
                float lightBrightness = world.GetLightBrightness(GetBlockPosition());
                world.GetGameManager().SpawnParticleEffectServer(
                    new ParticleEffect(_particleOnDestroy, getHeadPosition(), lightBrightness, Color.white, null, null, false),
                    entityId);
            }
        }
    }

    public override Vector3i dropCorpseBlock()
    {
        if (lootContainer != null && lootContainer.IsUserAccessing()) return Vector3i.zero;
        if (_corpseBlockValue.isair)                                   return Vector3i.zero;
        if (rand.RandomFloat > _corpseBlockChance)                     return Vector3i.zero;

        var pos = World.worldToBlockPos(position);
        while (pos.y < 254 && (float)pos.y - position.y < 3f &&
               !_corpseBlockValue.Block.CanPlaceBlockAt(world, 0, pos, _corpseBlockValue, false))
            pos += Vector3i.up;

        if (pos.y >= 254 || (float)pos.y - position.y >= 2.1f) return Vector3i.zero;

        world.SetBlockRPC(pos, _corpseBlockValue);
        if (pos == Vector3i.zero) return Vector3i.zero;

        if (world.GetTileEntity(0, pos) is not TileEntityLootContainer te) return Vector3i.zero;

        if (lootContainer != null)
            te.CopyLootContainerDataFromOther(lootContainer);
        else
        {
            te.lootListName = lootListOnDeath;
            te.SetContainerSize(LootContainer.GetLootContainer(lootListOnDeath).size, true);
        }
        te.SetModified();
        return pos;
    }
}
