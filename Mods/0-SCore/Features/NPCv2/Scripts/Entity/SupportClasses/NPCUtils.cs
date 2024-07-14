using System;
using System.Collections.Generic;
using UnityEngine;

public class NPCUtils {
    private EntityAlive _entityAlive;
    private Transform _largeEntityBlocker;

    private float fallTime;
    private float fallThresholdTime;

    public NPCUtils(EntityAlive entityAlive) {
        _entityAlive = entityAlive;
        _largeEntityBlocker = GameUtils.FindTagInChilds(_entityAlive.RootTransform, "LargeEntityBlocker");
    }

    public void CheckCollision(float distance = 5f) {
        var isPlayerWithin = IsAnyPlayerWithingDist(distance, _entityAlive);
        ToggleCollisions(!isPlayerWithin);
    }

    public void ToggleCollisions(bool value) {
        ToggleCollisions(value, _entityAlive);
    }

    private void ToggleCollisions(bool value, EntityAlive entity) {
        if (_largeEntityBlocker)
        {
            _largeEntityBlocker.gameObject.SetActive(value);
        }

        entity.PhysicsTransform.gameObject.SetActive(value);
        entity.IsNoCollisionMode.Value = !value;
    }

    private static bool IsAnyPlayerWithingDist(float dist, EntityAlive entity) {
        var persistentPlayerList = GameManager.Instance.GetPersistentPlayerList();
        if (persistentPlayerList?.Players == null) return false;

        foreach (var keyValuePair in persistentPlayerList.Players)
        {
            var entityPlayer = entity.world.GetEntity(keyValuePair.Value.EntityId) as EntityPlayer;

            if (!entityPlayer) continue;

            var magnitude = (entityPlayer.getChestPosition() - entity.position).magnitude;

            if (!entityPlayer || !(magnitude <= dist)) continue;

            FactionManager.Relationship myRelationship =
                FactionManager.Instance.GetRelationshipTier(entity, entityPlayer);

            // Have to keep it at Neutral, love relationships aren't being picked up for whatever reason when defined in the XML
            if (myRelationship == FactionManager.Relationship.Neutral)
            {
                return true;
            }
        }

        return false;
    }

    public void CheckFallAndGround() {
        if (_entityAlive.isEntityRemote) return;
        if (!_entityAlive.emodel) return;

        var entityAliveV2 = _entityAlive as EntityAliveV2;
        var isInElevator = _entityAlive.IsInElevator();

        // Jump state isn't visible, so we made a work around.
        var jumpState = EntityAlive.JumpState.Off;
        if (entityAliveV2 != null)
        {
            jumpState = entityAliveV2.GetJumpState();
        }

        var avatarController = _entityAlive.emodel.avatarController;
        if (!avatarController) return;

        var flag = _entityAlive.onGround || _entityAlive.isSwimming || isInElevator;
        if (flag)
        {
            fallTime = 0f;
            fallThresholdTime = 0f;
            if (isInElevator)
            {
                fallThresholdTime = 0.6f;
            }
        }
        else
        {
            if (fallThresholdTime == 0f)
            {
                fallThresholdTime = 0.1f + _entityAlive.rand.RandomFloat * 0.3f;
            }

            fallTime += 0.05f;
        }

        var canFall = !_entityAlive.emodel.IsRagdollActive &&
                      _entityAlive.bodyDamage.CurrentStun == EnumEntityStunType.None &&
                      !_entityAlive.isSwimming && !isInElevator &&
                      jumpState == EntityAlive.JumpState.Off &&
                      !_entityAlive.IsDead();

        if (fallTime <= fallThresholdTime)
        {
            canFall = false;
        }

        _entityAlive.emodel.avatarController.UpdateBool("CanFall", canFall);
        _entityAlive.emodel.avatarController.UpdateBool("IsOnGround", _entityAlive.onGround || _entityAlive.isSwimming);

        avatarController.SetFallAndGround(canFall, flag);
    }

    public void SetupStartingItems() {
        var gameModeForId = GameMode.GetGameModeForId(GameStats.GetInt(EnumGameStats.GameModeId));
        var entityClass = EntityClass.list[_entityAlive.entityClass];
        List<ItemStack> itemsOnEnterGame = new List<ItemStack>();

        foreach (var text in entityClass.Properties
                     .Values[EntityClass.PropItemsOnEnterGame + "." + gameModeForId.GetTypeName()]
                     .Split(',', StringSplitOptions.None))
        {
            var itemStack = ItemStack.FromString(text.Trim());
            if (itemStack.itemValue.IsEmpty()) continue;
            itemsOnEnterGame.Add(itemStack);
        }

        for (var i = 0; i < itemsOnEnterGame.Count; i++)
        {
            var itemStack = itemsOnEnterGame[i];
            var forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
            {
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
            }
            else
            {
                itemStack.count = forId.Stacknumber.Value;
            }

            _entityAlive.inventory.SetItem(i, itemStack);
        }
    }

    public Vector3 ConfigureBoundaryBox(Vector3 newSize, Vector3 center) {
        var component = _entityAlive.gameObject.GetComponent<BoxCollider>();
        if (!component) return Vector3.zero;

        // Re-adjusting the box collider     
        component.size = newSize;

        var scaledExtent = new Vector3(component.size.x / 2f * _entityAlive.transform.localScale.x,
            component.size.y / 2f * _entityAlive.transform.localScale.y,
            component.size.z / 2f * _entityAlive.transform.localScale.z);
        var vector = new Vector3(component.center.x * _entityAlive.transform.localScale.x,
            component.center.y * _entityAlive.transform.localScale.y,
            component.center.z * _entityAlive.transform.localScale.z);
        _entityAlive.boundingBox = BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z,
            scaledExtent.x,
            scaledExtent.y, scaledExtent.z);

        _entityAlive.boundingBox.center += vector;

        if (center != Vector3.zero)
            _entityAlive.boundingBox.center = center;
        return scaledExtent;
    }
}