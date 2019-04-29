using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

//namespace Mods
//{
    class MecanimSDX : AvatarController
    {
        // If set to true, logging will be very verbose for troubleshooting
        private readonly bool blDisplayLog = false;

        // interval between changing the indexes in the LateUpdate
        private float nextCheck = 0.0f;
        public float CheckDelay = 5f;

        // Animator support method to keep our current state
        protected AnimatorStateInfo currentBaseState;
        public float animSyncWaitTime = 0.5f;

        // Our transforms for key elements
        public Transform bipedTransform;
        public Transform modelTransform;
        protected Transform head;

        // This controls the animations if we are holding a weapon.
        protected Animator rightHandAnimator;
        private string RightHand = "RightHand";
        private Transform rightHandItemTransform;
        private Transform rightHand;

        // support variable for timing attacks.
        protected int specialAttackTicks;
        protected float timeSpecialAttackPlaying;
        protected float timeAttackAnimationPlaying;
        protected float idleTime;



        // Indexes used to add more variety to state machines
        private int AttackIndexes;
        private int AttackIdleIndexes;
        private int CrouchIndexes;
        private int DeathIndexes;
        private int EatingIndexes;
        private int ElectrocutionIndexes;
        private int HarvestIndexes;
        private int IdleIndexes;
        private int JumpIndexes;
        private int PainIndexes;
        private int RagingIndexes;
        private int RandomIndexes;
        private int RunIndexes;
        private int SleeperIndexes;
        private int SpecialAttackIndexes;
        private int SpecialSecondIndexes;
        private int StunIndexes;
        private int WalkIndexes;

        // bools to check if we are performing an action already
        private bool isInDeathAnim;
        private bool IsElectrocuting = false;
        private bool IsHarvesting = false;
        private bool isEating = false;

        // Maintenance varaibles
        protected bool m_bVisible = false;
        private bool CriticalError = false;

        // Jumping tags and bools
        protected int jumpState;
        protected int fpvJumpState;
        protected new int jumpTag;
        private bool Jumping = false;

        protected int movementStateOverride = -1;

        private MecanimSDX()
        {
            this.entity = base.transform.gameObject.GetComponent<EntityAlive>();
            EntityClass entityClass = EntityClass.list[this.entity.entityClass];

            // this.AttackHash = this.GenerateLists(entityClass, "AttackAnimations");
            int.TryParse(entityClass.Properties.Values["AttackIndexes"], out this.AttackIndexes);
            int.TryParse(entityClass.Properties.Values["SpecialAttackIndexes"], out this.SpecialAttackIndexes);
            int.TryParse(entityClass.Properties.Values["SpecialSecondIndexes"], out this.SpecialSecondIndexes);
            int.TryParse(entityClass.Properties.Values["RagingIndexes"], out this.RagingIndexes);
            int.TryParse(entityClass.Properties.Values["ElectrocutionIndexes"], out this.ElectrocutionIndexes);
            int.TryParse(entityClass.Properties.Values["CrouchIndexes"], out this.CrouchIndexes);
            int.TryParse(entityClass.Properties.Values["StunIndexes"], out this.StunIndexes);
            int.TryParse(entityClass.Properties.Values["SleeperIndexes"], out this.SleeperIndexes);
            int.TryParse(entityClass.Properties.Values["HarvestIndexes"], out this.HarvestIndexes);
            int.TryParse(entityClass.Properties.Values["PainIndexes"], out this.PainIndexes);
            int.TryParse(entityClass.Properties.Values["DeathIndexes"], out this.DeathIndexes);
            int.TryParse(entityClass.Properties.Values["RunIndexes"], out this.RunIndexes);
            int.TryParse(entityClass.Properties.Values["WalkIndexes"], out this.WalkIndexes);
            int.TryParse(entityClass.Properties.Values["IdleIndexes"], out this.IdleIndexes);
            int.TryParse(entityClass.Properties.Values["JumpIndexes"], out this.JumpIndexes);
            int.TryParse(entityClass.Properties.Values["EatingIndexes"], out this.EatingIndexes);
            int.TryParse(entityClass.Properties.Values["RandomIndexes"], out this.RandomIndexes);
            int.TryParse(entityClass.Properties.Values["AttackIdleIndexes"], out this.AttackIdleIndexes);
            if(entityClass.Properties.Values.ContainsKey("RightHandJointName"))
            {
                this.RightHand = entityClass.Properties.Values["RightHandJointName"];
            }
            this.jumpTag = Animator.StringToHash("Jump");

            if(entityClass.Properties.Values.ContainsKey("RightHandJointName"))
            {
                this.RightHand = entityClass.Properties.Values["RightHandJointName"];

            }
        }


        public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
        {
            Log("Running Switch and Model View");
            this.SetBool("IsDead", this.entity.IsDead());
            this.SetBool("IsAlive", this.entity.IsAlive());

            // dummy assign body parts
            this.assignBodyParts();

            // Check if this entity has a weapon or not
            if(this.rightHandItemTransform != null)
            {
                Log("Setting Right hand position");
                this.rightHandItemTransform.parent = this.rightHandItemTransform;
                Vector3 position = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].position;
                Vector3 rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[this.entity.inventory.holdingItem.HoldType.Value].rotation;
                this.rightHandItemTransform.localPosition = position;
                this.rightHandItemTransform.localEulerAngles = rotation;
                this.SetInRightHand(this.rightHandItemTransform);
            }
        }

        public override bool IsAnimationAttackPlaying()
        {
            return this.timeAttackAnimationPlaying > 0f;
        }

        public override void StartAnimationAttack()
        {
            this.SetRandomIndex("AttackIndex");
            this.SetTrigger("Attack");
            this.SetRandomIndex("AttackIdleIndex");
            this.timeAttackAnimationPlaying = 0.3f;
        }

        // Main Update method
        protected virtual void Update()
        {
            if(this.timeAttackAnimationPlaying > 0f)
            {
                this.timeAttackAnimationPlaying -= Time.deltaTime;
            }

            // No need to proceed if the model isn't initialized.
            if(this.bipedTransform == null || !this.bipedTransform.gameObject.activeInHierarchy)
            {
                return;
            }

            if(!(this.anim == null) && this.anim.avatar.isValid && this.anim.enabled)
            {
                // Logic to handle our movements
                float num = 0f;
                float num2 = 0f;
                if(!this.entity.IsFlyMode.Value)
                {
                    num = this.entity.speedForward;
                    num2 = this.entity.speedStrafe;
                }
                float num3 = num2;
                if(num3 >= 1234f)
                {
                    num3 = 0f;
                }
                this.SetFloat("Forward", num);
                this.SetFloat("Strafe", num3);
                if(!this.entity.IsDead())
                {
                    if(this.movementStateOverride != -1)
                    {
                        this.SetInt("MovementState", this.movementStateOverride);
                        this.movementStateOverride = -1;
                    }
                    else if(num2 >= 1234f)
                    {
                        this.SetInt("MovementState", 4);
                    }
                    else
                    {
                        float num4 = num * num + num3 * num3;
                        this.SetInt("MovementState", (num4 <= this.entity.moveSpeedAggro * this.entity.moveSpeedAggro) ? ((num4 <= this.entity.moveSpeed * this.entity.moveSpeed) ? ((num4 <= 0.001f) ? 0 : 1) : 2) : 3);
                    }
                }

                if(Mathf.Abs(num) <= 0.01f && Mathf.Abs(num2) <= 0.01f)
                {
                    this.SetBool("IsMoving", false);
                }
                else
                {
                    this.idleTime = 0f;
                    this.SetBool("IsMoving", true);
                }

                if(nextCheck == 0.0f || nextCheck < Time.time)
                {
                    nextCheck = Time.time + CheckDelay;
                    SetRandomIndex("RandomIndex");

                    SetRandomIndex("WalkIndex");
                    SetRandomIndex("RunIndex");
                    SetRandomIndex("IdleIndex");
                }

                this.SetFloat("IdleTime", this.idleTime);
                this.idleTime += Time.deltaTime;
                this.SetFloat("RotationPitch", this.entity.rotation.x);

                base.SendAnimParameters(0.05f);
                return;
            }
        }

        public override bool IsAnimationSpecialAttackPlaying()
        {
            return this.IsAnimationAttackPlaying();
        }

        public override void StartAnimationSpecialAttack(bool _b)
        {
            if(_b)
            {
                this.Log("Firing Special attack");
                this.SetRandomIndex("SpecialAttackIndex");
                this.SetTrigger("SpecialAttack");
                this.idleTime = 0f;
                this.specialAttackTicks = 3;
                this.timeSpecialAttackPlaying = 0.8f;

            }
        }

        // Logic to handle Special Attack
        public override bool IsAnimationSpecialAttack2Playing()
        {
            return this.IsAnimationAttackPlaying();
        }
        public override void StartAnimationSpecialAttack2()
        {
            this.Log("Firing Second Special attack");
            this.SetRandomIndex("SpecialSecondAttack");
            this.SetTrigger("SpecialSecondAttack");
        }


        // Logic to handle Raging
        public override void StartAnimationRaging()
        {
            this.SetRandomIndex("RagingIndex");
            this.SetTrigger("Raging");
        }

        // Logic to handle electrocution
        public override bool IsAnimationElectrocutedPlaying()
        {
            return this.IsElectrocuting;
        }
        public override void StartAnimationElectrocuted()
        {
            if(!this.IsAnimationElectrocutedPlaying())
            {
                this.IsElectrocuting = true;
                this.SetRandomIndex("ElectrocutionIndex");
                this.SetTrigger("Electrocution");
            }
        }

        public override bool IsAnimationHarvestingPlaying()
        {
            return this.IsHarvesting;
        }
        public override void StartAnimationHarvesting()
        {
            if(!this.IsAnimationHarvestingPlaying())
            {
                this.IsHarvesting = true;
                this.SetRandomIndex("HarvestIndex");
                this.SetTrigger("Harvest");
            }
        }

        public override void SetAlive()
        {
            this.SetBool("IsAlive", true);
            this.SetBool("IsDead", false);
            this.SetTrigger("Alive");
        }

        public override void SetDrunk(float _numBeers)
        {
            if(_numBeers > 3f)
            {
                this.SetRandomIndex("DrunkIndex");
                this.SetTrigger("Drunk");
            }
        }


        public override void SetCrouching(bool _bEnable)
        {
            if(_bEnable)
            {
                this.SetRandomIndex("CrouchIndex");
            }
            this.SetBool("IsCrouching", _bEnable);
        }

        public override void SetVisible(bool _b)
        {
            if(this.m_bVisible != _b)
            {
                this.m_bVisible = _b;
                Transform transform = this.bipedTransform;
                if(transform != null)
                {
                    Renderer[] componentsInChildren = transform.GetComponentsInChildren<Renderer>();
                    for(int i = 0; i < componentsInChildren.Length; i++)
                    {
                        componentsInChildren[i].enabled = _b;
                    }
                }
            }
        }

        public override void SetRagdollEnabled(bool _b)
        {
        }

        public override void StartAnimationReloading()
        {
        }

        public override void StartAnimationJumping()
        {
            this.SetRandomIndex("JumpIndex");
            this.SetTrigger("Jump");
        }

        public override void StartAnimationFiring()
        {
        }

        public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random)
        {
            this.SetRandomIndex("PainIndex");
            this.SetTrigger("Pain");
        }

        public override bool IsAnimationHitRunning()
        {
            return false;
        }

        public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float _random)
        {
            this.SetRandomIndex("DeathIndex");
            this.SetBool("IsDead", true);
        }

        public override void SetInRightHand(Transform _transform)
        {
            if(!(this.rightHandItemTransform == null) && !(_transform == null))
            {
                this.Log("Setting Right Hand: " + this.rightHandItemTransform.name.ToString());
                this.idleTime = 0f;
                this.Log("Setting Right Hand Transform");
                this.rightHandItemTransform = _transform;
                if(this.rightHandItemTransform == null)
                {
                    this.Log("Right Hand Animator is Null");
                }
                else
                {
                    this.Log("Right Hand Animator is NOT NULL ");
                    this.rightHandAnimator = this.rightHandItemTransform.GetComponent<Animator>();
                    if(this.rightHandItemTransform != null)
                    {
                        Utils.SetLayerRecursively(this.rightHandItemTransform.gameObject, 0);
                    }
                    this.Log("Done with SetInRightHand");
                }
            }
        }

        public override Transform GetRightHandTransform()
        {
            return this.rightHandItemTransform;
        }

        public override Transform GetActiveModelRoot()
        {
            return (!this.modelTransform) ? this.bipedTransform : this.modelTransform;
        }


        public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
        {
            this.SetRandomIndex("StunIndex");
            this.SetBool("IsStunned", true);
        }

        public override void EndStun()
        {
            this.SetBool("IsStunned", false);
        }



        public override void TriggerSleeperPose(int pose)
        {
            if(this.anim != null)
            {
                this.anim.SetInteger("SleeperPose", pose);
                this.SetTrigger("SleeperTrigger");
            }
        }

        private void Log(string strLog)
        {
            if(this.blDisplayLog)
            {
                if(this.modelTransform == null)
                {
                    Debug.Log(string.Format("Unknown Entity: {0}", strLog));
                }
                else
                {
                    Debug.Log(string.Format("{0}: {1}", this.modelTransform.name, strLog));
                }
            }
        }


        private new void Awake()
        {
            this.Log("Method: " + MethodBase.GetCurrentMethod().Name);
            this.Log("Initializing " + this.entity.name);
            try
            {
                this.Log("Checking For Graphics Transform...");
                this.bipedTransform = base.transform.Find("Graphics");
                if(this.bipedTransform == null || this.bipedTransform.Find("Model") == null)
                {
                    this.Log(" !! Graphics Transform null!");
                    return;
                }

                this.modelTransform = this.bipedTransform.Find("Model").GetChild(0);
                if(this.modelTransform == null)
                {
                    this.Log(" !! Model Transform is null!");
                    return;
                }
                this.Log("Adding Colliders");
                this.AddTransformRefs(this.modelTransform);

                this.Log("Tagging the Body");
                this.AddTagRecursively(this.modelTransform, "E_BP_Body");

                this.Log("Searching for Animator");
                this.anim = this.modelTransform.GetComponent<Animator>();
                if(this.anim == null)
                {
                    this.Log("*** Animator Not Found! Invalid Class");
                    this.CriticalError = true;
                    throw new Exception("Animator Not Found! Wrong class is being used! Try AnimationSDX instead...");
                }
                this.Log("Animator Found");
                this.anim.enabled = true;
                if(!this.anim.runtimeAnimatorController)
                {
                    this.Log(string.Format("{0} : My Animator Controller is null!", this.modelTransform.name));
                    this.CriticalError = true;
                    throw new Exception("Animator Controller is null!");
                }
                this.Log("My Animator Controller has: " + this.anim.runtimeAnimatorController.animationClips.Length + " Animations");
                foreach(AnimationClip animationClip in this.anim.runtimeAnimatorController.animationClips)
                {
                    this.Log("Animation Clip: " + animationClip.name.ToString());
                }

                this.rightHandItemTransform = this.FindTransform(this.bipedTransform, this.bipedTransform, this.RightHand);
                if(this.rightHandItemTransform)
                {
                    this.Log("Right Hand Item Transform: " + this.rightHandItemTransform.name.ToString());
                }
                else
                {
                    this.Log("Right Hand Item Transform: Could not find Transofmr: " + this.RightHand);
                }
            }
            catch(Exception arg)
            {
                this.Log("Exception thrown in Awake() " + arg);
            }
        }

        private Transform FindTransform(Transform root, Transform t, string objectName)
        {
            Transform result;
            if(t.name.Contains(objectName))
            {
                result = t;
            }
            else
            {
                foreach(object obj in t)
                {
                    Transform t2 = (Transform)obj;
                    Transform transform = this.FindTransform(root, t2, objectName);
                    if(transform != null)
                    {
                        return transform;
                    }
                }
                result = null;
            }
            return result;
        }


        private void UpdateCurrentState()
        {
            this.currentBaseState = this.anim.GetCurrentAnimatorStateInfo(0);
        }


        // Since we support many different indexes, we use this to generate a random index, and send it to the state machine.
        public void SetRandomIndex(string strParam)
        {
            int index = 0;
            switch(strParam)
            {
                case "AttackIndex":
                    index = this.GetRandomIndex(this.AttackIndexes);
                    break;
                case "SpecialAttackIndex":
                    index = this.GetRandomIndex(this.SpecialAttackIndexes);
                    break;
                case "SpecialSecondIndex":
                    index = this.GetRandomIndex(this.SpecialSecondIndexes);
                    break;
                case "RagingIndex":
                    index = this.GetRandomIndex(this.RagingIndexes);
                    break;
                case "ElectrocutionIndex":
                    index = this.GetRandomIndex(this.ElectrocutionIndexes);
                    break;
                case "CrouchIndex":
                    index = this.GetRandomIndex(this.CrouchIndexes);
                    break;
                case "StunIndex":
                    index = this.GetRandomIndex(this.StunIndexes);
                    break;
                case "SleeperIndex":
                    index = this.GetRandomIndex(this.SleeperIndexes);
                    break;
                case "HarvestIndex":
                    index = this.GetRandomIndex(this.HarvestIndexes);
                    break;
                case "PainIndex":
                    index = this.GetRandomIndex(this.PainIndexes);
                    break;
                case "DeathIndex":
                    index = this.GetRandomIndex(this.DeathIndexes);
                    break;
                case "RunIndex":
                    index = this.GetRandomIndex(this.RunIndexes);
                    break;
                case "WalkIndex":
                    index = this.GetRandomIndex(this.WalkIndexes);
                    break;
                case "IdleIndex":
                    index = this.GetRandomIndex(this.IdleIndexes);
                    break;
                case "JumpIndex":
                    index = this.GetRandomIndex(this.JumpIndexes);
                    break;
                case "RandomIndex":
                    index = this.GetRandomIndex(this.RandomIndexes);
                    break;
                case "EatingIndex":
                    index = this.GetRandomIndex(this.EatingIndexes);
                    break;
                case "AttackIdleIndex":
                    index = this.GetRandomIndex(this.AttackIdleIndexes);
                    break;
            }

            Log(string.Format("Random Generator: {0} Value: {1}", strParam, index));
            SetInt(strParam, index);
        }

        public int GetRandomIndex(int intMax)
        {
            return UnityEngine.Random.Range(0, intMax);
        }

        private EntityClass GetAvailableTriggers()
        {
            return EntityClass.list[this.entity.entityClass];
        }

        private void AddTransformRefs(Transform t)
        {
            if(t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
            {
                RootTransformRefEntity rootTransformRefEntity = t.gameObject.AddComponent<RootTransformRefEntity>();
                rootTransformRefEntity.RootTransform = base.transform;
            }
            foreach(object obj in t)
            {
                Transform t2 = (Transform)obj;
                this.AddTransformRefs(t2);
            }
        }

        private void AddTagRecursively(Transform trans, string tag)
        {
            if(trans.gameObject.tag.Contains("Untagged"))
            {
                if(trans.name.ToLower().Contains("head"))
                {
                    trans.gameObject.tag = "E_BP_Head";
                }
                else
                {
                    trans.gameObject.tag = tag;
                }
            }
            foreach(object obj in trans)
            {
                Transform trans2 = (Transform)obj;
                this.AddTagRecursively(trans2, tag);
            }
        }

        protected void SpawnLimbGore(Transform parent, string path, bool restoreState)
        {
            if(parent != null)
            {
                // GameObject original = ResourceWrapper.Load1P(path) as GameObject;
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Resources.Load(path) as GameObject, Vector3.zero, Quaternion.identity);
                gameObject.transform.parent = parent;
                gameObject.transform.localPosition = Vector3.zero;
                gameObject.transform.localRotation = Quaternion.identity;
                gameObject.transform.localScale = parent.localScale;
                GorePrefab component = gameObject.GetComponent<GorePrefab>();
                if(component != null)
                {
                    component.restoreState = restoreState;
                }
            }
        }

        protected void assignBodyParts()
        {
            if(this.bipedTransform == null)
            {
                this.Log("assignBodyParts: GraphicsTransform is null!");
            }
            else
            {
                this.Log("Mapping Body Parts");
                this.head = this.FindTransform(this.bipedTransform, this.bipedTransform, "Head");
                this.rightHand = this.FindTransform(this.bipedTransform, this.bipedTransform, this.RightHand);
            }
        }

        public override bool IsAnimationJumpRunning()
        {
            return this.Jumping || this.jumpTag == this.currentBaseState.tagHash;
        }

        public override void StartEating()
        {
            if(!this.isEating)
            {
                this.SetRandomIndex("EatingIndex");
                this.SetBool("IsEating", true);
                this.SetTrigger("IsEatingTrigger");
                this.isEating = true;
            }
        }
        public override void StopEating()
        {
            if(this.isEating)
            {
                this.SetBool("IsEating", false);
                this.isEating = false;
            }
        }


    }
//}