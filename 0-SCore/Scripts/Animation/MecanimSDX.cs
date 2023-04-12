using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

/// <summary>
/// Custom class for humanoid animators. Deprecated.
/// </summary>
internal class MecanimSDX : AvatarController
{
    public float checkDelay = 5f;

    // Our transforms for key elements
    public Transform bipedTransform;
    public Transform modelTransform;
    private int _attackIdleIndexes;


    // Indexes used to add more variety to state machines
    private int _attackIndexes;

    // If set to true, logging will be very verbose for troubleshooting
    private readonly bool blDisplayLog = false;
    private int _crouchIndexes;
    private int _deathIndexes;
    private int _eatingIndexes;
    private int _electrocutionIndexes;
    private int _harvestIndexes;
    private int _idleIndexes;
    private  int _jumpIndexes;
    private readonly bool Jumping = false;
    private  int _ragingIndexes;
    private  int _randomIndexes;
    private  string _rightHand = "RightHand";
    private  int _runIndexes;
    private  int _sleeperIndexes;
    private  int _specialAttackIndexes;
    private  int _specialSecondIndexes;
    private  int _stunIndexes;
    private  int _walkIndexes;
    private int _painIndexes;
    // Animator support method to keep our current state
    private AnimatorStateInfo _currentBaseState;
    private Transform _head;
    private float _idleTime;
    private bool _isEating;

    // bools to check if we are performing an action already
    private bool _isHarvesting;

    // Jumping tags and bools
    private new int _jumpTag;

    // Maintenance varaibles
    private bool _mBVisible;

    private int _movementStateOverride = -1;

    // interval between changing the indexes in the LateUpdate
    private float _nextCheck;

    // This controls the animations if we are holding a weapon.
    private Transform rightHand;
    private Animator _rightHandAnimator;
    private Transform _rightHandItemTransform;

    // support variable for timing attacks.
    private int _specialAttackTicks;
    private float _timeAttackAnimationPlaying;
    private float _timeSpecialAttackPlaying;

   

    private new void Awake()
    {
        entity = transform.gameObject.GetComponent<EntityAlive>();
        var entityClass = EntityClass.list[entity.entityClass];

        // this.AttackHash = this.GenerateLists(entityClass, "AttackAnimations");
        int.TryParse(entityClass.Properties.Values["AttackIndexes"], out _attackIndexes);
        int.TryParse(entityClass.Properties.Values["SpecialAttackIndexes"], out _specialAttackIndexes);
        int.TryParse(entityClass.Properties.Values["SpecialSecondIndexes"], out _specialSecondIndexes);
        int.TryParse(entityClass.Properties.Values["RagingIndexes"], out _ragingIndexes);
        int.TryParse(entityClass.Properties.Values["ElectrocutionIndexes"], out _electrocutionIndexes);
        int.TryParse(entityClass.Properties.Values["CrouchIndexes"], out _crouchIndexes);
        int.TryParse(entityClass.Properties.Values["StunIndexes"], out _stunIndexes);
        int.TryParse(entityClass.Properties.Values["SleeperIndexes"], out _sleeperIndexes);
        int.TryParse(entityClass.Properties.Values["HarvestIndexes"], out _harvestIndexes);
        int.TryParse(entityClass.Properties.Values["PainIndexes"], out _painIndexes);
        int.TryParse(entityClass.Properties.Values["DeathIndexes"], out _deathIndexes);
        int.TryParse(entityClass.Properties.Values["RunIndexes"], out _runIndexes);
        int.TryParse(entityClass.Properties.Values["WalkIndexes"], out _walkIndexes);
        int.TryParse(entityClass.Properties.Values["IdleIndexes"], out _idleIndexes);
        int.TryParse(entityClass.Properties.Values["JumpIndexes"], out _jumpIndexes);
        int.TryParse(entityClass.Properties.Values["EatingIndexes"], out _eatingIndexes);
        int.TryParse(entityClass.Properties.Values["RandomIndexes"], out _randomIndexes);
        int.TryParse(entityClass.Properties.Values["AttackIdleIndexes"], out _attackIdleIndexes);
        if (entityClass.Properties.Values.ContainsKey("RightHandJointName")) _rightHand = entityClass.Properties.Values["RightHandJointName"];
        _jumpTag = Animator.StringToHash("Jump");

        if (entityClass.Properties.Values.ContainsKey("RightHandJointName")) _rightHand = entityClass.Properties.Values["RightHandJointName"];
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

            _rightHandItemTransform = FindTransform(bipedTransform, bipedTransform, _rightHand);
            if (_rightHandItemTransform)
                Log("Right Hand Item Transform: " + _rightHandItemTransform.name);
            else
                Log("Right Hand Item Transform: Could not find Transform: " + _rightHand);
        }
        catch (Exception arg)
        {
            Log("Exception thrown in Awake() " + arg);
        }
    }

    // Main Update method
    protected override void Update()
    {
        if (_timeAttackAnimationPlaying > 0f) _timeAttackAnimationPlaying -= Time.deltaTime;

        if (!_mBVisible && (entity == null || entity.isEntityRemote)) return;

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
            SetFloat("Forward", num);
            SetFloat("Strafe", num3);
            if (!entity.IsDead())
            {
                if (_movementStateOverride != -1)
                {
                    SetInt("MovementState", _movementStateOverride);
                    _movementStateOverride = -1;
                }
                else if (num2 >= 1234f)
                {
                    SetInt("MovementState", 4);
                }
                else
                {
                    var num4 = num * num + num3 * num3;
                    SetInt("MovementState", num4 <= entity.moveSpeedAggro * entity.moveSpeedAggro ? num4 <= entity.moveSpeed * entity.moveSpeed ? num4 <= 0.001f ? 0 : 1 : 2 : 3);
                }
            }

            if (Mathf.Abs(num) <= 0.01f && Mathf.Abs(num2) <= 0.01f)
            {
                SetBool("IsMoving", false);
            }
            else
            {
                _idleTime = 0f;
                SetBool("IsMoving", true);
            }

            if (_nextCheck == 0.0f || _nextCheck < Time.time)
            {
                _nextCheck = Time.time + checkDelay;
                SetRandomIndex("RandomIndex");

                SetRandomIndex("WalkIndex");
                SetRandomIndex("RunIndex");
                SetRandomIndex("IdleIndex");
            }

            SetFloat("IdleTime", _idleTime);
            _idleTime += Time.deltaTime;
            SetFloat("RotationPitch", entity.rotation.x);

            SendAnimParameters(0.05f);
        }
    }


    public override void SwitchModelAndView(string _modelName, bool _bFPV, bool _bMale)
    {
        Log("Running Switch and Model View");
        SetBool("IsDead", entity.IsDead());
        SetBool("IsAlive", entity.IsAlive());

        // dummy assign body parts
        assignBodyParts();

        // Check if this entity has a weapon or not
        if (_rightHandItemTransform != null)
        {
            Log("Setting Right hand position");
            _rightHandItemTransform.parent = _rightHandItemTransform;
            var position = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].position;
            var rotation = AnimationGunjointOffsetData.AnimationGunjointOffset[entity.inventory.holdingItem.HoldType.Value].rotation;
            _rightHandItemTransform.localPosition = position;
            _rightHandItemTransform.localEulerAngles = rotation;
            SetInRightHand(_rightHandItemTransform);
        }
    }

    public override bool IsAnimationAttackPlaying()
    {
        return _timeAttackAnimationPlaying > 0f;
    }

    public override void StartAnimationAttack()
    {
        SetRandomIndex("AttackIndex");
        SetTrigger("Attack");
        SetRandomIndex("AttackIdleIndex");
        _timeAttackAnimationPlaying = 0.3f;
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
            SetTrigger("SpecialAttack");
            _idleTime = 0f;
            _specialAttackTicks = 3;
            _timeSpecialAttackPlaying = 0.8f;
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
        SetTrigger("SpecialSecondAttack");
    }


    // Logic to handle Raging
    public override void StartAnimationRaging()
    {
        SetRandomIndex("RagingIndex");
        SetTrigger("Raging");
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
        return _isHarvesting;
    }

    public override void StartAnimationHarvesting()
    {
        if (!IsAnimationHarvestingPlaying())
        {
            _isHarvesting = true;
            SetRandomIndex("HarvestIndex");
            SetTrigger("Harvest");
        }
    }

    public override void SetAlive()
    {
        SetBool("IsAlive", true);
        SetBool("IsDead", false);
        SetTrigger("Alive");
    }

    public override void SetDrunk(float _numBeers)
    {
        if (_numBeers > 3f)
        {
            SetRandomIndex("DrunkIndex");
            SetTrigger("Drunk");
        }
    }


    public override void SetCrouching(bool _bEnable)
    {
        if (_bEnable) SetRandomIndex("CrouchIndex");
        SetBool("IsCrouching", _bEnable);
    }

    public override void SetVisible(bool _b)
    {
        if (_mBVisible != _b)
        {
            _mBVisible = _b;
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
        SetTrigger("Jump");
    }

    public override void StartAnimationFiring()
    {
    }

    public override void StartAnimationHit(EnumBodyPartHit _bodyPart, int _dir, int _hitDamage, bool _criticalHit, int _movementState, float _random, float _duration)
    {
        SetRandomIndex("PainIndex");
        SetTrigger("Pain");

        base.StartAnimationHit(_bodyPart, _dir, _hitDamage, _criticalHit, _movementState, _random, _duration);
    }

    public override bool IsAnimationHitRunning()
    {
        return false;
    }

    public override void StartDeathAnimation(EnumBodyPartHit _bodyPart, int _movementState, float _random)
    {
        SetRandomIndex("DeathIndex");
        SetBool("IsDead", true);
    }

    public override void SetInRightHand(Transform _transform)
    {
        if (!(_rightHandItemTransform == null) && !(_transform == null))
        {
            Log("Setting Right Hand: " + _rightHandItemTransform.name);
            _idleTime = 0f;
            Log("Setting Right Hand Transform");
            _rightHandItemTransform = _transform;
            if (_rightHandItemTransform == null)
            {
                Log("Right Hand Animator is Null");
            }
            else
            {
                Log("Right Hand Animator is NOT NULL ");
                _rightHandAnimator = _rightHandItemTransform.GetComponent<Animator>();
                if (_rightHandItemTransform != null) Utils.SetLayerRecursively(_rightHandItemTransform.gameObject, 0);
                Log("Done with SetInRightHand");
            }
        }
    }

    public override Transform GetRightHandTransform()
    {
        return _rightHandItemTransform;
    }

    public override Transform GetActiveModelRoot()
    {
        return !modelTransform ? bipedTransform : modelTransform;
    }


    public override void BeginStun(EnumEntityStunType stun, EnumBodyPartHit _bodyPart, Utils.EnumHitDirection _hitDirection, bool _criticalHit, float random)
    {
        SetRandomIndex("StunIndex");
        SetBool("IsStunned", true);
    }

    public override void EndStun()
    {
        SetBool("IsStunned", false);
    }


    public override void TriggerSleeperPose(int pose)
    {
        if (anim != null)
        {
            anim.SetInteger("SleeperPose", pose);
            SetTrigger("SleeperTrigger");
        }
    }

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
        _currentBaseState = anim.GetCurrentAnimatorStateInfo(0);
    }


    // Since we support many different indexes, we use this to generate a random index, and send it to the state machine.
    public void SetRandomIndex(string strParam)
    {
        var index = 0;
        switch (strParam)
        {
            case "AttackIndex":
                index = GetRandomIndex(_attackIndexes);
                break;
            case "SpecialAttackIndex":
                index = GetRandomIndex(_specialAttackIndexes);
                break;
            case "SpecialSecondIndex":
                index = GetRandomIndex(_specialSecondIndexes);
                break;
            case "RagingIndex":
                index = GetRandomIndex(_ragingIndexes);
                break;
            case "ElectrocutionIndex":
                index = GetRandomIndex(_electrocutionIndexes);
                break;
            case "CrouchIndex":
                index = GetRandomIndex(_crouchIndexes);
                break;
            case "StunIndex":
                index = GetRandomIndex(_stunIndexes);
                break;
            case "SleeperIndex":
                index = GetRandomIndex(_sleeperIndexes);
                break;
            case "HarvestIndex":
                index = GetRandomIndex(_harvestIndexes);
                break;
            case "PainIndex":
                index = GetRandomIndex(_painIndexes);
                break;
            case "DeathIndex":
                index = GetRandomIndex(_deathIndexes);
                break;
            case "RunIndex":
                index = GetRandomIndex(_runIndexes);
                break;
            case "WalkIndex":
                index = GetRandomIndex(_walkIndexes);
                break;
            case "IdleIndex":
                index = GetRandomIndex(_idleIndexes);
                break;
            case "JumpIndex":
                index = GetRandomIndex(_jumpIndexes);
                break;
            case "RandomIndex":
                index = GetRandomIndex(_randomIndexes);
                break;
            case "EatingIndex":
                index = GetRandomIndex(_eatingIndexes);
                break;
            case "AttackIdleIndex":
                index = GetRandomIndex(_attackIdleIndexes);
                break;
        }

        Log(string.Format("Random Generator: {0} Value: {1}", strParam, index));
        SetInt(strParam, index);
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
            _head = FindTransform(bipedTransform, bipedTransform, "Head");
            rightHand = FindTransform(bipedTransform, bipedTransform, _rightHand);
        }
    }

    public override bool IsAnimationJumpRunning()
    {
        return Jumping || _jumpTag == _currentBaseState.tagHash;
    }


    public override void StartEating()
    {
        if (!_isEating)
        {
            SetRandomIndex("EatingIndex");
            SetBool("IsEating", true);
            SetTrigger("IsEatingTrigger");
            _isEating = true;
        }
    }

    public override void StopEating()
    {
        if (_isEating)
        {
            SetBool("IsEating", false);
            _isEating = false;
        }
    }
}