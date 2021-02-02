/*
 * Class: EntityAliveEnemySDX
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base entityAlive. This is meant to be a base class, where other classes can extend
 *      from, giving them the ability to accept quests and buffs.
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features. 
 *
 *      <property name="Class" value="EntityAliveEnemySDX, Mods" />
 */
using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityAliveEnemySDX : EntityEnemy
{
    public float flEyeHeight = -1f;
    public bool bWentThroughDoor = false;

    // Default name
    String strMyName = "EvilBob";
    String strTitle;

    public System.Random random = new System.Random();

    private readonly bool blDisplayLog = false;
    public void DisplayLog(String strMessage)
    {
        if (blDisplayLog && !IsDead())
            Debug.Log(entityName + ": " + strMessage);
    }

    public override string ToString()
    {
        return EntityUtilities.DisplayEntityStats(entityId);
    }



    public override float GetEyeHeight()
    {
        if (flEyeHeight == -1f)
            return base.GetEyeHeight();

        return flEyeHeight;
    }

    public override void SetModelLayer(int _layerId, bool _force = false)
    {
        //Utils.SetLayerRecursively(this.emodel.GetModelTransform().gameObject, _layerId);
    }
    // Over-ride for CopyProperties to allow it to read in StartingQuests.
    public override void CopyPropertiesFromEntityClass()
    {
        base.CopyPropertiesFromEntityClass();
        EntityClass entityClass = EntityClass.list[this.entityClass];

        flEyeHeight = EntityUtilities.GetFloatValue(entityId, "EyeHeight");

        // Read in a list of names then pick one at random.
        if (entityClass.Properties.Values.ContainsKey("Names"))
        {
            string text = entityClass.Properties.Values["Names"];
            string[] Names = text.Split(',');

            int index = UnityEngine.Random.Range(0, Names.Length);
            strMyName = Names[index];
        }

        if (entityClass.Properties.Values.ContainsKey("Titles"))
        {
            string text = entityClass.Properties.Values["Titles"];
            string[] Names = text.Split(',');
            int index = UnityEngine.Random.Range(0, Names.Length);
            strTitle = Names[index];
        }


        if (entityClass.Properties.Classes.ContainsKey("Boundary"))
        {
            DisplayLog(" Found Bandary Settings");
            String strBoundaryBox = "0,0,0";
            String strCenter = "0,0,0";
            DynamicProperties dynamicProperties3 = entityClass.Properties.Classes["Boundary"];
            foreach (KeyValuePair<string, object> keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                DisplayLog("Key: " + keyValuePair.Key);
                if (keyValuePair.Key == "BoundaryBox")
                {
                    DisplayLog(" Found a Boundary Box");
                    strBoundaryBox = dynamicProperties3.Values[keyValuePair.Key];
                    continue;
                }

                if (keyValuePair.Key == "Center")
                {
                    DisplayLog(" Found a Center");
                    strCenter = dynamicProperties3.Values[keyValuePair.Key];
                    continue;
                }
            }

            Vector3 Box = StringParsers.ParseVector3(strBoundaryBox, 0, -1);
            Vector3 Center = StringParsers.ParseVector3(strCenter, 0, -1);
            ConfigureBounaryBox(Box, Center);
        }
    }

    protected override float getNextStepSoundDistance()
    {
        if (!IsRunning)
        {
            return 0.5f;
        }
        return 0.25f;
    }


    public void ConfigureBounaryBox(Vector3 newSize, Vector3 center)
    {
        BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        if (component)
        {
            DisplayLog(" Box Collider: " + component.size.ToCultureInvariantString());
            DisplayLog(" Current Boundary Box: " + boundingBox.ToCultureInvariantString());
            // Re-adjusting the box collider     
            component.size = newSize;

            scaledExtent = new Vector3(component.size.x / 2f * base.transform.localScale.x, component.size.y / 2f * base.transform.localScale.y, component.size.z / 2f * base.transform.localScale.z);
            Vector3 vector = new Vector3(component.center.x * base.transform.localScale.x, component.center.y * base.transform.localScale.y, component.center.z * base.transform.localScale.z);
            boundingBox = global::BoundsUtils.BoundsForMinMax(-scaledExtent.x, -scaledExtent.y, -scaledExtent.z, scaledExtent.x, scaledExtent.y, scaledExtent.z);

            boundingBox.center = boundingBox.center + vector;

            if (center != Vector3.zero)
                boundingBox.center = center;

            DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());
        }

    }

    public override string EntityName
    {
        get
        {
            if (strMyName == "Bob")
                return entityName;

            if (String.IsNullOrEmpty(strTitle))
                return strMyName + " the " + base.EntityName;
            else
                return strMyName + " the " + strTitle;
        }
        set
        {
            if (!value.Equals(entityName))
            {
                entityName = value;
                bPlayerStatsChanged |= !isEntityRemote;
            }
        }
    }

    public override void PostInit()
    {
        base.PostInit();
        SetupStartingItems();
        inventory.SetHoldingItemIdx(0);
    }

    protected virtual void SetupStartingItems()
    {
        for (int i = 0; i < itemsOnEnterGame.Count; i++)
        {
            ItemStack itemStack = itemsOnEnterGame[i];
            ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
            if (forId.HasQuality)
            {
                itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
            }
            else
            {
                itemStack.count = forId.Stacknumber.Value;
            }
            inventory.SetItem(i, itemStack);
        }
    }
    public override void OnUpdateLive()
    {
        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
        {
            //If blocked, check to see if its a door.
            if (moveHelper.IsBlocked)
            {
                Vector3i blockPos = moveHelper.HitInfo.hit.blockPos;
                BlockValue block = world.GetBlock(blockPos);
                if (Block.list[block.type].HasTag(BlockTags.Door) && !BlockDoor.IsDoorOpen(block.meta))
                {
                    bool canOpenDoor = true;
                    TileEntitySecureDoor tileEntitySecureDoor = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntitySecureDoor;
                    if (tileEntitySecureDoor != null)
                    {
                        if (tileEntitySecureDoor.IsLocked() && tileEntitySecureDoor.GetOwner() == "")
                            canOpenDoor = false;
                    }
                    //TileEntityPowered poweredDoor = GameManager.Instance.World.GetTileEntity(0, blockPos) as TileEntityPowered;
                    //if (poweredDoor != null)
                    //{
                    //    if (poweredDoor.IsLocked() && poweredDoor.GetOwner() == "")
                    //        canOpenDoor = false;

                    //}
                    if (canOpenDoor)
                    {
                        DisplayLog("I am blocked by a door. Trying to open...");
                        SphereCache.AddDoor(entityId, blockPos);
                        EntityUtilities.OpenDoor(entityId, blockPos);
                        //  We were blocked, so let's clear it.
                        moveHelper.ClearBlocked();

                    }
                }
            }
        }

        // Check to see if we've opened a door, and close it behind you.
        Vector3i doorPos = SphereCache.GetDoor(entityId);
        if (doorPos != Vector3i.zero)
        {
            DisplayLog("I've opened a door recently. I'll see if I can close it.");
            BlockValue block = world.GetBlock(doorPos);
            if (Block.list[block.type].HasTag(BlockTags.Door) && BlockDoor.IsDoorOpen(block.meta))
            {
                float CloseDistance = 3;
                // If it's a multidim, increase tha radius a bit
                if (Block.list[block.type].isMultiBlock)
                {
                    Vector3i vector3i = StringParsers.ParseVector3i(Block.list[block.type].Properties.Values["MultiBlockDim"], 0, -1, false);
                    if (CloseDistance > vector3i.x)
                        CloseDistance = vector3i.x + 1;

                }
                if ((GetDistanceSq(doorPos.ToVector3()) > CloseDistance))
                {
                    DisplayLog("I am going to close the door now.");
                    EntityUtilities.CloseDoor(entityId, doorPos);
                    SphereCache.RemoveDoor(entityId, doorPos);
                }
            }
        }

        // Makes the NPC always face its attack or revent target.
        EntityAlive target = EntityUtilities.GetAttackOrReventTarget(entityId) as EntityAlive;
        if (target != null)
        {
            // makes the npc look at its attack target
            if (emodel != null && emodel.avatarController != null)
                emodel.SetLookAt(target.getHeadPosition());

            SetLookPosition(target.getHeadPosition());
            RotateTo(target, 45, 45);
        }
        Buffs.RemoveBuff("buffnewbiecoat", false);
        Stats.Health.MaxModifier = Stats.Health.Max;

        // Set CanFall and IsOnGround
        if (emodel != null && emodel.avatarController != null)
        {
            emodel.avatarController.SetBool("CanFall", !emodel.IsRagdollActive && bodyDamage.CurrentStun == EnumEntityStunType.None && !isSwimming);
            emodel.avatarController.SetBool("IsOnGround", onGround || isSwimming);
        }


        // Non-player entities don't fire all the buffs or stats, so we'll manually fire the water tick,
        Stats.Water.Tick(0.5f, 0, false);

        // then fire the updatestats over time, which is protected from a IsPlayer check in the base onUpdateLive().
        Stats.UpdateStatsOverTime(0.5f);


        base.OnUpdateLive();

    }

}
