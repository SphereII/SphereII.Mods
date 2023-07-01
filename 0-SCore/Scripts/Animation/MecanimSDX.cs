using System;
using System.Reflection;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Custom class for humanoid animators. Deprecated.
/// </summary>
internal class MecanimSDX : AvatarController
{
    public float CheckDelay = 5f;
    public new float animSyncWaitTime = 0.5f;

    // Our transforms for key elements
    public Transform bipedTransform;
    public Transform modelTransform;
    private int AttackIdleIndexes;


    // Indexes used to add more variety to state machines
    private int AttackIndexes;

    // If set to true, logging will be very verbose for troubleshooting
    private bool blDisplayLog = false;
    private int CrouchIndexes;
    private int DeathIndexes;
    private int EatingIndexes;
    private int ElectrocutionIndexes;
    private int HarvestIndexes;
    private int IdleIndexes;
    private int JumpIndexes;
    private bool Jumping = false;
    private int RagingIndexes;
    private int RandomIndexes;
    private string RightHand = "RightHand";
    private int RunIndexes;
    private int SleeperIndexes;
    private int SpecialAttackIndexes;
    private int SpecialSecondIndexes;
    private int StunIndexes;
    private int WalkIndexes;
    private int PainIndexes;
    // Animator support method to keep our current state
    protected AnimatorStateInfo currentBaseState;
    protected Transform head;
    protected float idleTime;
    private bool isEating;

    // bools to check if we are performing an action already
    private bool IsHarvesting;

    // Jumping tags and bools
    protected new int jumpTag;

    // Maintenance varaibles
    protected bool m_bVisible;

    protected int movementStateOverride = -1;

    // interval between changing the indexes in the LateUpdate
    private float nextCheck;
    private Transform rightHand;

    // This controls the animations if we are holding a weapon.
    protected Animator rightHandAnimator;
    private Transform rightHandItemTransform;

    // support variable for timing attacks.
    protected int specialAttackTicks;
    protected float timeAttackAnimationPlaying;
    protected float timeSpecialAttackPlaying;



    private new void Awake()
    {
        Log("Method: " + MethodBase.GetCurrentMethod().Name);

        entity = transform.gameObject.GetComponent<EntityAlive>();
        var entityClass = EntityClass.list[entity.entityClass];

        // this.AttackHash = this.GenerateLists(entityClass, "AttackAnimations");
        int.TryParse(entityClass.Properties.Values["AttackIndexes"], out AttackIndexes);
        int.TryParse(entityClass.Properties.Values["SpecialAttackIndexes"], out SpecialAttackIndexes);
        int.TryParse(entityClass.Properties.Values["SpecialSecondIndexes"], out SpecialSecondIndexes);
        int.TryParse(entityClass.Properties.Values["RagingIndexes"], out RagingIndexes);
        int.TryParse(entityClass.Properties.Values["ElectrocutionIndexes"], out ElectrocutionIndexes);
        int.TryParse(entityClass.Properties.Values["CrouchIndexes"], out CrouchIndexes);
        int.TryParse(entityClass.Properties.Values["StunIndexes"], out StunIndexes);
        int.TryParse(entityClass.Properties.Values["SleeperIndexes"], out SleeperIndexes);
        int.TryParse(entityClass.Properties.Values["HarvestIndexes"], out HarvestIndexes);
        int.TryParse(entityClass.Properties.Values["PainIndexes"], out PainIndexes);
        int.TryParse(entityClass.Properties.Values["DeathIndexes"], out DeathIndexes);
        int.TryParse(entityClass.Properties.Values["RunIndexes"], out RunIndexes);
        int.TryParse(entityClass.Properties.Values["WalkIndexes"], out WalkIndexes);
        int.TryParse(entityClass.Properties.Values["IdleIndexes"], out IdleIndexes);
        int.TryParse(entityClass.Properties.Values["JumpIndexes"], out JumpIndexes);
        int.TryParse(entityClass.Properties.Values["EatingIndexes"], out EatingIndexes);
        int.TryParse(entityClass.Properties.Values["RandomIndexes"], out RandomIndexes);
        int.TryParse(entityClass.Properties.Values["AttackIdleIndexes"], out AttackIdleIndexes);
        if (entityClass.Properties.Values.ContainsKey("RightHandJointName")) RightHand = entityClass.Properties.Values["RightHandJointName"];
        jumpTag = Animator.StringToHash("Jump");

        if (entityClass.Properties.Values.ContainsKey("RightHandJointName")) RightHand = entityClass.Properties.Values["RightHandJointName"];
        Log("Initializing " + entity.name);
        try
        {
            Log("Checking For Graphics Transform...");
            bipedTransform = transform.Find("Graphics");
            if (bipedTransform == null || bipedTransform.Find("Model") == null)
            {
                Log(" !! Graphics Transform null!");
                return;
            }

            modelTransform = bipedTransform.Find("Model").GetChild(0);
            if (modelTransform == null)
            {
                Log(" !! Model Transform is null!");
                return;
            }

            Log("Adding Colliders");
            AddTransformRefs(modelTransform);

            Log("Tagging the Body");
            AddTagRecursively(modelTransform, "E_BP_Body");

            Log("Searching for Animator");
            anim = modelTransform.GetComponent<Animator>();
            if (anim == null)
            {
                Log("*** Animator Not Found! Invalid Class");
                throw new Exception("Animator Not Found! Wrong class is being used! Try AnimationSDX instead...");
            }

            Log("Animator Found");
            anim.enabled = true;
            if (!anim.runtimeAnimatorController)
            {
                Log(string.Format("{0} : My Animator Controller is null!", modelTransform.name));
                throw new Exception("Animator Controller is null!");
            }

            Log("My Animator Controller has: " + anim.runtimeAnimatorController.animationClips.Length + " Animations");
            foreach (var animationClip in anim.runtimeAnimatorController.animationClips) Log("Animation Clip: " + animationClip.name);

            rightHandItemTransform = FindTransform(bipedTransform, bipedTransform, RightHand);
            if (rightHandItemTransform)
                Log("Right Hand Item Transform: " + rightHandItemTransform.name);
            else
                Log("Right Hand Item Transform: Could not find Transform: " + RightHand);
        }
        catch (Exception arg)
        {
            Log("Exception thrown in Awake() " + arg);
        }
    }

    // Main Update method
    protected override void Update()
    {
        if (timeAttackAnimationPlaying > 0f) timeAttackAnimationPlaying -= Time.deltaTime;

        if (!m_bVisible && (entity == null || entity.isEntityRemote)) return;

        // No need to proceed if the model isn't initialized.
        if (bipedTransform == null || !bipedTransform.gameObject.activeInHierarchy) return;

        if (!(anim == null) && anim.avatar.isValid && anim.enabled)
        {
            // Logic to handle our movements
            var num = 0f;
            var num2 = 0f;
            if (!entity.IsFlyMode.Value)
            {
                num = entity.speedForward;
                num2 = entity.speedStrafe;
            }

            var num3 = num2;
            if (num3 >= 1234f) num3 = 0f;
            UpdateFloat("Forward", num);
            UpdateFloat("Strafe", num3);
            if (!entity.IsDead())
            {
                if (movementStateOverride != -1)
                {
                    UpdateInt("MovementState", movementStateOverride);
                    movementStateOverride = -1;
                }
                else if (num2 >= 1234f)
                {
                    UpdateInt("MovementState", 4);
                }
                else
                {
                    var num4 = num * num + num3 * num3;
                    UpdateInt("MovementState", num4 <= entity.moveSpeedAggro * entity.moveSpeedAggro ? num4 <= entity.moveSpeed * entity.moveSpeed ? num4 <= 0.001f ? 0 : 1 : 2 : 3);
                }
            }

            if (Mathf.Abs(num) <= 0.01f && Mathf.Abs(num2) <= 0.01f)
            {
                UpdateBool("IsMoving", false);
            }
            else
            {
                idleTime = 0f;
                UpdateBool("IsMoving", true);
            }

            if (nextCheck == 0.0f || nextCheck < Time.time)
            {
                nextCheck = Time.time + CheckDelay;
                SetRandomIndex("RandomIndex");

                SetRandomIndex("WalkIndex");
                SetRandomIndex("RunIndex");
                SetRandomIndex("IdleIndex");
            }

            UpdateFloat("IdleTime", idleTime);
            idleTime += Time.deltaTime;
            UpdateFloat("RotationPitch", entity.rotation.x);

            SendAnimParameters(0.05f);
        }
    }


    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        Log("Running Switch and Model View");
        UpdateBool("IsDead", entity.IsDead());
        UpdateBool("IsAlive", entity.IsAlive());

        // dummy assign body parts
        assignBodyParts();

        // Check if this entity has a weapon or not
        if (rightHandItemTransform != null)
        {
            Log("Setting Right hand position");
            rightHandItemTransform.parent = rightHandItemTransform;
            var position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
            var rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
            rightHandItemTransform.localPosition = position;
            rightHandItemTransform.localEulerAngles = rotation;
            SetInRightHand(rightHandItemTransform);
        }
    }

    public override bool IsAnimationAttackPlaying()
    {
        return timeAttackAnimationPlaying > 0f;
    }

    public override void StartAnimationAttack()
    {
        SetRandomIndex("AttackIndex");
        TriggerEvent("Attack");
        SetRandomIndex("AttackIdleIndex");
        timeAttackAnimationPlaying = 0.3f;
    }

    public override bool IsAnimationSpecialAttackPlaying()
    {
        return IsAnimationAttackPlaying();
    }

    public override void StartAnimationSpecialAttack(bool _b, int _animType)
    {
        if (_b)
        {
            Log("Firing Special attack");
            SetRandomIndex("SpecialAttackIndex");
            TriggerEvent("SpecialAttack");
            idleTime = 0f;
            specialAttackTicks = 3;
            timeSpecialAttackPlaying = 0.8f;
        }
    }
    //public override void StartAnimationSpecialAttack(bool _b)
    //{
    //    if (_b)
    //    {
    //        Log("Firing Special attack");
    //        SetRandomIndex("SpecialAttackIndex");
    //        SetTrigger("SpecialAttack");
    //        idleTime = 0f;
    //        specialAttackTicks = 3;
    //        timeSpecialAttackPlaying = 0.8f;
    //    }
    //}

    // Logic to handle Special Attack
    public override bool IsAnimationSpecialAttack2Playing()
    {
        return IsAnimationAttackPlaying();
    }

    public override void StartAnimationSpecialAttack2()
    {
        Log("Firing Second Special attack");
        SetRandomIndex("SpecialSecondAttack");
        TriggerEvent("SpecialSecondAttack");
    }


    // Logic to handle Raging
    public override void StartAnimationRaging()
    {
        SetRandomIndex("RagingIndex");
        TriggerEvent("Raging");
    }

    // Logic to handle electrocution A19 b180
    //public override bool IsAnimationElectrocutedPlaying()
    //{
    //    return IsElectrocuting;
    //}
    //public override void StartAnimationElectrocuted()
    //{
    //    if(!IsAnimationElectrocutedPlaying())
    //    {
    //        IsElectrocuting = true;
    //        SetRandomIndex("ElectrocutionIndex");
    //        SetTrigger("Electrocution");
    //    }
    //}

    public override bool IsAnimationHarvestingPlaying()
    {
        return IsHarvesting;
    }

    public override void StartAnimationHarvesting()
    {
        if (!IsAnimationHarvestingPlaying())
        {
            IsHarvesting = true;
            SetRandomIndex("HarvestIndex");
            TriggerEvent("Harvest");
        }
    }

    public override void SetAlive()
    {
        UpdateBool("IsAlive", true);
        UpdateBool("IsDead", false);
        TriggerEvent("Alive");
    }

    public override void SetDrunk(float _numBeers)
    {
        if (_numBeers > 3f)
        {
            SetRandomIndex("DrunkIndex");
            TriggerEvent("Drunk");
        }
    }


    public override void SetCrouching(bool _bEnable)
    {
        if (_bEnable) SetRandomIndex("CrouchIndex");
        UpdateBool("IsCrouching", _bEnable);
    }

    public override void SetVisible(bool _b)
    {
        if (m_bVisible != _b)
        {
            m_bVisible = _b;
            var transform = bipedTransform;
            if (transform != null)
            {
                var componentsInChildren = transform.GetComponentsInChildren<Renderer>();
                for (var i = 0; i < componentsInChildren.Length; i++) componentsInChildren[i].enabled = _b;
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
        SetRandomIndex("JumpIndex");
        TriggerEvent("Jump");
    }

    public override void StartAnimationFiring()
    {
    }

    public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
    {
        SetRandomIndex("PainIndex");
        TriggerEvent("Pain");

        base.StartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, _random, _duration);
    }

    public override bool IsAnimationHitRunning()
    {
        return false;
    }

    public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float _random)
    {
        SetRandomIndex("DeathIndex");
        UpdateBool("IsDead", true);
    }

    public override void SetInRightHand(Transform _transform)
    {
        if (!(rightHandItemTransform == null) && !(_transform == null))
        {
            Log("Setting Right Hand: " + rightHandItemTransform.name);
            idleTime = 0f;
            Log("Setting Right Hand Transform");
            rightHandItemTransform = _transform;
            if (rightHandItemTransform == null)
            {
                Log("Right Hand Animator is Null");
            }
            else
            {
                Log("Right Hand Animator is NOT NULL ");
                rightHandAnimator = rightHandItemTransform.GetComponent<Animator>();
                if (rightHandItemTransform != null) Utils.SetLayerRecursively(rightHandItemTransform.gameObject, 0);
                Log("Done with SetInRightHand");
            }
        }
    }

    public override Transform GetRightHandTransform()
    {
        return rightHandItemTransform;
    }

    public override Transform GetActiveModelRoot()
    {
        return !modelTransform ? bipedTransform : modelTransform;
    }


    public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
    {
        SetRandomIndex("StunIndex");
        UpdateBool("IsStunned", true);
    }

    public override void EndStun()
    {
        UpdateBool("IsStunned", false);
    }


    // public override void TriggerSleeperPose(int pose)
    // {
    //     if (anim != null)
    //     {
    //         anim.SetInteger("SleeperPose", pose);
    //         TriggerEvent("SleeperTrigger");
    //     }
    // }

    private void Log(string strLog)
    {
        if (blDisplayLog)
        {
            if (modelTransform == null)
                Debug.Log(string.Format("Unknown Entity: {0}", strLog));
            else
                Debug.Log(string.Format("{0}: {1}", modelTransform.name, strLog));
        }
    }

    private Transform FindTransform(Transform root, Transform t, string objectName)
    {
        Transform result;
        if (t.name.Contains(objectName))
        {
            result = t;
        }
        else
        {
            foreach (var obj in t)
            {
                var t2 = (Transform)obj;
                Log("\t Transform: " + t2.name);
                var transform = FindTransform(root, t2, objectName);
                if (transform != null) return transform;
            }

            result = null;
        }

        return result;
    }


    private void UpdateCurrentState()
    {
        currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
    }


    // Since we support many different indexes, we use this to generate a random index, and send it to the state machine.
    public void SetRandomIndex(string strParam)
    {
        var index = 0;
        switch (strParam)
        {
            case "AttackIndex":
                index = GetRandomIndex(AttackIndexes);
                break;
            case "SpecialAttackIndex":
                index = GetRandomIndex(SpecialAttackIndexes);
                break;
            case "SpecialSecondIndex":
                index = GetRandomIndex(SpecialSecondIndexes);
                break;
            case "RagingIndex":
                index = GetRandomIndex(RagingIndexes);
                break;
            case "ElectrocutionIndex":
                index = GetRandomIndex(ElectrocutionIndexes);
                break;
            case "CrouchIndex":
                index = GetRandomIndex(CrouchIndexes);
                break;
            case "StunIndex":
                index = GetRandomIndex(StunIndexes);
                break;
            case "SleeperIndex":
                index = GetRandomIndex(SleeperIndexes);
                break;
            case "HarvestIndex":
                index = GetRandomIndex(HarvestIndexes);
                break;
            case "PainIndex":
                index = GetRandomIndex(PainIndexes);
                break;
            case "DeathIndex":
                index = GetRandomIndex(DeathIndexes);
                break;
            case "RunIndex":
                index = GetRandomIndex(RunIndexes);
                break;
            case "WalkIndex":
                index = GetRandomIndex(WalkIndexes);
                break;
            case "IdleIndex":
                index = GetRandomIndex(IdleIndexes);
                break;
            case "JumpIndex":
                index = GetRandomIndex(JumpIndexes);
                break;
            case "RandomIndex":
                index = GetRandomIndex(RandomIndexes);
                break;
            case "EatingIndex":
                index = GetRandomIndex(EatingIndexes);
                break;
            case "AttackIdleIndex":
                index = GetRandomIndex(AttackIdleIndexes);
                break;
        }

        Log(string.Format("Random Generator: {0} Value: {1}", strParam, index));
        UpdateInt(strParam, index);
    }

    public int GetRandomIndex(int intMax)
    {
        return Random.Range(0, intMax);
    }

    private EntityClass GetAvailableTriggers()
    {
        return EntityClass.list[entity.entityClass];
    }

    private void AddTransformRefs(Transform t)
    {
        if (t.GetComponent<Collider>() != null && t.GetComponent<RootTransformRefEntity>() == null)
        {
            var rootTransformRefEntity = t.gameObject.AddComponent<RootTransformRefEntity>();
            rootTransformRefEntity.RootTransform = transform;
        }

        foreach (var obj in t)
        {
            var t2 = (Transform)obj;
            AddTransformRefs(t2);
        }
    }

    private void AddTagRecursively(Transform trans, string tag)
    {
        if (trans.gameObject.tag.Contains("Untagged"))
        {
            Log("AddTagRecursively: " + trans.name);
            if (trans.name.ToLower().Contains("head"))
                trans.gameObject.tag = "E_BP_Head";
            else
                trans.gameObject.tag = tag;
        }

        foreach (var obj in trans)
        {
            var trans2 = (Transform)obj;
            AddTagRecursively(trans2, tag);
        }
    }

    protected void SpawnLimbGore(Transform parent, string path, bool restoreState)
    {
        if (parent != null)
        {
            // GameObject original = ResourceWrapper.Load1P(path) as GameObject;
            var gameObject = Instantiate(Resources.Load(path) as GameObject, Vector3.zero, Quaternion.identity);
            gameObject.transform.parent = parent;
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localRotation = Quaternion.identity;
            gameObject.transform.localScale = parent.localScale;
            var component = gameObject.GetComponent<GorePrefab>();
            if (component != null) component.restoreState = restoreState;
        }
    }

    protected void assignBodyParts()
    {
        if (bipedTransform == null)
        {
            Log("assignBodyParts: GraphicsTransform is null!");
        }
        else
        {
            Log("Mapping Body Parts");
            head = FindTransform(bipedTransform, bipedTransform, "Head");
            rightHand = FindTransform(bipedTransform, bipedTransform, RightHand);
        }
    }

    public override bool IsAnimationJumpRunning()
    {
        return Jumping || jumpTag == currentBaseState.tagHash;
    }


    public override void StartEating()
    {
        if (!isEating)
        {
            SetRandomIndex("EatingIndex");
            UpdateBool("IsEating", true);
            TriggerEvent("IsEatingTrigger");
            isEating = true;
        }
    }

    public override void StopEating()
    {
        if (isEating)
        {
            UpdateBool("IsEating", false);
            isEating = false;
        }
    }
}