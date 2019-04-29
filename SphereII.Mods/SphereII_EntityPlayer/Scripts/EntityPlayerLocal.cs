/*
 * Class: EntityPlayerSDXLocal
 * Author:  sphereii 
 * Category: Entity
 * Description:
 *      This mod is an extension of the base PlayerLocal Class. It must be incuded with the EntityPlayerSDX class, as the code explicitly looks for this pair.
 *
 */
using System;
using System.IO;
using UnityEngine;
using XMLData.Item;

class EntityPlayerSDXLocal : EntityPlayerLocal
{
    private bool blOneBlockCrouch = false;
    private bool blSoftHands = false;
    private bool blAttackReleased = false;
    private ParticleSystem myFootStepParticle;

    public EntityClass.ParticleData myFootStepParticleData;
    public override void Init(int _entityClass)
    {
        base.Init(_entityClass);

        // Read the OneBlockCrouch setting if it's set, then adjust the crouch modifier accordingly
        EntityClass entityClass = EntityClass.list[_entityClass];
        if (entityClass.Properties.Values.ContainsKey("OneBlockCrouch"))
            bool.TryParse(entityClass.Properties.Values["OneBlockCrouch"], out this.blOneBlockCrouch);

        // Soft hands hurt when you hit things
        if (entityClass.Properties.Values.ContainsKey("SoftHands"))
            bool.TryParse(entityClass.Properties.Values["SoftHands"], out this.blSoftHands);

        if (blOneBlockCrouch)
        {
            this.vp_FPController.PhysicsCrouchHeightModifier = 0.49f;
            this.vp_FPController.SyncCharacterController();

        }

        if ( entityClass.Properties.Values.ContainsKey("FootStepParticle"))
        {
            Debug.Log("Initializing foot step particle");
            this.myFootStepParticleData.fileName = entityClass.Properties.Values["FootStepParticle"];
            myFootStepParticle = SetParticle(this.myFootStepParticleData);
            if (myFootStepParticle)
                Debug.Log("Particle foot step initialized.");
            else
                Debug.Log("Particle foot step is NOT initialized");
        }
            

    }

    public override bool IsAttackValid()
    {

        if (base.IsAttackValid() && this.inventory.holdingItem.Name == "meleeHandPlayer" && this.blSoftHands)
        {
            WorldRayHitInfo executeActionTarget = this.inventory.holdingItem.Actions[0].GetExecuteActionTarget(this.inventory.holdingItemData.actionData[0]);
            if (executeActionTarget == null)
            {
                return true;
            }

            // If we hit something in our bare hands, get hurt!
            if (executeActionTarget.bHitValid)
            {
                DamageSource dmg = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Bashing);
                DamageEntity(dmg, 1, false, 1f);
            }

        }

     

        return true;
    }

    public override void PlayStepSound()
    {
        base.PlayStepSound();
    }

    public ParticleSystem SetParticle(EntityClass.ParticleData myParticle)
    {
        Debug.Log("Checking " + myParticle.fileName);
        GameObject temp = DataLoader.LoadAsset<GameObject>(myParticle.fileName);
        if ( temp == null )
        {
            Debug.Log("Could not load the particle.");
            return null;
        }
        Debug.Log("Loaded Game Object");
        GameObject temp2 = UnityEngine.Object.Instantiate<GameObject>(temp);
        if ( temp2 == null )
        {
            Debug.Log("Tried to instantiate game object: failed");
            return null;
        }
        Debug.Log("Searching for particle systems");
        foreach (ParticleSystem particleSystem in temp2.GetComponentsInChildren<ParticleSystem>())
        {
            Debug.Log("particle system: " + particleSystem.name);

            particleSystem.transform.parent = this.emodel.GetModelTransform();

            return particleSystem;
            //if (myParticle.shapeMesh != null && myParticle.shapeMesh.Length > 0)
            //{
            //    SkinnedMeshRenderer[] componentsInChildren = base.GetComponentsInChildren<SkinnedMeshRenderer>();
            //    ParticleSystem.ShapeModule shape = particleSystem.shape;
            //    shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            //    string text = myParticle.shapeMesh.ToLower();
            //    if (text.Contains("setshapetomesh"))
            //    {
            //        text = text.Replace("setshapetomesh", string.Empty);
            //        int num = int.Parse(text);
            //        if (num >= 0 && num < componentsInChildren.Length)
            //        {
            //            shape.skinnedMeshRenderer = componentsInChildren[num];
            //            ParticleSystem[] componentsInChildren2 = particleSystem.transform.GetComponentsInChildren<ParticleSystem>();
            //            if (componentsInChildren2 != null)
            //            {
            //                for (int i = 0; i < componentsInChildren2.Length; i++)
            //                {
            //                    shape = componentsInChildren2[i].shape;
            //                    shape.shapeType = ParticleSystemShapeType.SkinnedMeshRenderer;
            //                    shape.skinnedMeshRenderer = componentsInChildren[num];
            //                    return componentsInChildren2[i];
            //                }
            //            }
            //        }
            //    }

                
            //}

        }
        return null;
    }

}
