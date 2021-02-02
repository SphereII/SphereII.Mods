﻿/*
 * Class: EntityAliveFarmingAnimal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base EntityAliveSDX, and allows animal husbandry... breeding, etc
 * 
 * Usage:
 *      Add the following class to entities that are meant to use these features.
 *
 *      <property name="Class" value="EntityAliveFarmingAnimal, Mods" />
 */
using System;
using UnityEngine;

public class EntityAliveFarmingAnimalSDX : EntityAliveSDX
{

    protected override void Awake()
    {
        base.Awake();

        // So they don't step over each other.
        // this.stepHeight = 0f;
        // ConfigureBounaryBox(new Vector3(1f, 1.8f, 1f));

    }

    public void ConfigureBounaryBox(Vector3 newSize)
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

            // component.center = new Vector3(newSize.x, newSize.y / 2, newSize.z);
            nativeCollider = component;
            //this.scaledExtent = component.size;
            //this.boundingBox = BoundsUtils.BoundsForMinMax(newSize.x, newSize.y, newSize.z, newSize.x, newSize.x, newSize.z );
            if (isDetailedHeadBodyColliders())
                component.enabled = false;
            DisplayLog(" After BoundaryBox: " + boundingBox.ToCultureInvariantString());

            CapsuleCollider[] componentsInChildren = base.gameObject.GetComponentsInChildren<CapsuleCollider>();
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                if (!componentsInChildren[i].gameObject.CompareTag("LargeEntityBlocker"))
                {
                    componentsInChildren[i].transform.gameObject.layer = 14;
                }
            }
            BoxCollider[] componentsInChildren2 = base.gameObject.GetComponentsInChildren<BoxCollider>();
            for (int j = 0; j < componentsInChildren2.Length; j++)
            {
                componentsInChildren2[j].transform.gameObject.layer = 14;
            }
        }

    }
    public override void CopyPropertiesFromEntityClass()
    {
        npcID = "animalFarm";

        base.CopyPropertiesFromEntityClass();
    }

    // Cows were being stuck on the fence and trying to attack them. This is, I think, due to the entity move helper which makes
    // it attack blocks that get in its way, ala zombie.
    public new bool Attack(bool _bAttackReleased)
    {
        if (attackTarget == null)
            return false;

        return base.Attack(_bAttackReleased);
    }

    public void CheckAnimalEvent()
    {
        // Test Hooks
        DisplayLog(ToString());
    }

    // read in the cvar for sizeScale and adjust it based on the buff
    public void AdjustSizeForStage()
    {
        //float size = this.Buffs.GetCustomVar("$sizeScale");
        //if (size > 0.0f)
        //{
        //    this.gameObject.transform.localScale = new Vector3(size, size, size);
        //    //ConfigureBounaryBox( this.gameObject.transform.localScale);
        //}
    }


    public override void OnUpdateLive()
    {
        // AdjustSizeForStage();

        if (Buffs.HasCustomVar("Herd"))
        {
            EntityAliveFarmingAnimalSDX temp = world.GetEntity((int)Buffs.GetCustomVar("Herd")) as EntityAliveFarmingAnimalSDX;
            if (temp)
            {

                Buffs.SetCustomVar("CurrentOrder", (float)EntityUtilities.Orders.None, true);
                setHomeArea(temp.GetBlockPosition(), 10);
            }
        }
        base.OnUpdateLive();

    }

    public override string ToString()
    {
        String strOutput = base.ToString();

        String strMilkLevel = "0";
        if (Buffs.HasCustomVar("MilkLevel"))
        {
            strMilkLevel = Buffs.GetCustomVar("MilkLevel").ToString();
            strOutput += "\n Milk Level: " + strMilkLevel;
        }

        if (Buffs.HasCustomVar("$EggValue"))
        {
            String strEggLevel = Buffs.GetCustomVar("$EggValue").ToString();
            strOutput += "\n Egg Level: " + strEggLevel;
        }
        if (Buffs.HasCustomVar("Mother"))
        {
            int MotherID = (int)Buffs.GetCustomVar("Mother");
            EntityAliveSDX MotherEntity = world.GetEntity(MotherID) as EntityAliveSDX;
            if (MotherEntity)
                strOutput += "\n My Mother is: " + MotherEntity.EntityName + " ( " + MotherID + " )";
        }

        return strOutput;
    }



    public override bool CanEntityJump()
    {
        return false;
    }
    public override bool Jumping
    {
        get
        {
            return false;
        }
        set
        {

        }
    }
}
