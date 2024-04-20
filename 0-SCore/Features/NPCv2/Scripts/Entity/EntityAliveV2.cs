using System;
using System.Collections.Generic;
using System.IO;
using HarmonyLib;
using UnityEngine;


public class EntityAliveV2 : EntityNPC {
    private PushOutOfBlocksUtils _pushOutOfBlocksUtils;
    private LeaderUtils _leaderUtils;
    private MissionUtils _missionUtils;
    private NPCUtils _npcUtils;

    public string NewName = "";
    public string _strMyName = "Bob";
    private string _strTitle;

    private TileEntity _tileEntity;
    public override void Init(int _entityClass) {
        base.Init(_entityClass);
        _pushOutOfBlocksUtils = new PushOutOfBlocksUtils(this);
        _npcUtils = new NPCUtils(this);
    }
    
    public override void PostInit() {
        base.PostInit();
        PhysicsTransform.gameObject.SetActive(false);
        if (_npcUtils != null)
            _npcUtils = new NPCUtils(this);
        _npcUtils?.SetupStartingItems();

        inventory.SetHoldingItemIdx(0);
    }

    public override void CopyPropertiesFromEntityClass() {
        base.CopyPropertiesFromEntityClass();
        var _entityClass = EntityClass.list[entityClass];

        // Read in a list of names then pick one at random.
        if (_entityClass.Properties.Values.ContainsKey("Names"))
        {
            var text = _entityClass.Properties.Values["Names"];
            var names = text.Split(',');

            var index = UnityEngine.Random.Range(0, names.Length);
            _strMyName = names[index];
        }
        if (_entityClass.Properties.Classes.ContainsKey("Boundary"))
        {
            var strBoundaryBox = "0,0,0";
            var strCenter = "0,0,0";
            var dynamicProperties3 = _entityClass.Properties.Classes["Boundary"];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                switch (keyValuePair.Key)
                {
                    case "BoundaryBox":
                        strBoundaryBox = dynamicProperties3.Values[keyValuePair.Key];
                        continue;
                    case "Center":
                        strCenter = dynamicProperties3.Values[keyValuePair.Key];
                        break;
                }
            }

            var box = StringParsers.ParseVector3(strBoundaryBox);
            var center = StringParsers.ParseVector3(strCenter);
            if (_npcUtils == null)
                _npcUtils = new NPCUtils(this);
            scaledExtent = _npcUtils.ConfigureBoundaryBox(box, center);
        }

        /*
         * 	<property class="NPCConfiguration" >
         *	    <property name="Hireable" value="true" />
         *  	<property name="Missions" value="true"/>
         *	</property>
         */
        if (_entityClass.Properties.Classes.ContainsKey("NPCConfiguration"))
        {
            var dynamicProperties3 = _entityClass.Properties.Classes["NPCConfiguration"];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
            {
                switch (keyValuePair.Key)
                {
                    case "Hireable":
                        if (StringParsers.ParseBool(keyValuePair.Value.ToString()))
                            _leaderUtils = new LeaderUtils(this);
                        continue;
                    case "Missions":
                        if (StringParsers.ParseBool(keyValuePair.Value.ToString()))
                            _missionUtils = new MissionUtils(this);
                        break;
                }
            }
        }
    }

    public override void PlayOneShot(string clipName, bool sound_in_head = false) {
        if (IsOnMission()) return;
        base.PlayOneShot(clipName, sound_in_head);
    }

    protected override void playStepSound(string stepSound) {
        if (IsOnMission()) return;
        if (HasAnyTags(FastTags.Parse("floating"))) return;

        base.playStepSound(stepSound);
    }

    public override bool IsAttackValid() {
        if (IsOnMission()) return false;
        return base.IsAttackValid();
    }

    // Helper Method is maintain backwards compatibility, while also pushing the code out to another class to clean it up.
    private bool IsOnMission() {
        if (_missionUtils == null) return false;
        return _missionUtils.IsOnMission();
    }

    // Helper Method is maintain backwards compatibility, while also pushing the code out to another class to clean it up.
    public void SendOnMission(bool send) {
        _npcUtils?.ToggleCollisions(!send);
        _missionUtils?.SendOnMission(send);
    }

    public new void SetAttackTarget(EntityAlive _attackTarget, int _attackTargetTime) {
        if (_attackTarget != null && _attackTarget.IsDead()) return;

        if (IsOnMission()) return;

        // Some of the AI tasks resets the attack target when it falls down stunned; this will prevent the NPC from ignoring its stunned opponent.
        if (_attackTarget == null)
        {
            if (attackTarget != null && attackTarget.IsAlive())
                return;
        }

        base.SetAttackTarget(_attackTarget, _attackTargetTime);
    }

    public new void SetRevengeTarget(EntityAlive _other) {
        if (IsOnMission()) return;
        base.SetRevengeTarget(_other);
    }

    public override void ProcessDamageResponse(DamageResponse _dmResponse) {
        if (IsOnMission()) return;
        base.ProcessDamageResponse(_dmResponse);
    }

    public override bool IsImmuneToLegDamage {
        get {
            if (IsOnMission()) return true;
            return base.IsImmuneToLegDamage;
        }
    }


 

    public override void ProcessDamageResponseLocal(DamageResponse _dmResponse) {
        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable")) return;
        if (Buffs.HasBuff("buffInvulnerable")) return;

        if (!isEntityRemote)
        {
            // If we are being attacked, let the state machine know it can fight back
            emodel.avatarController.UpdateBool("IsBusy", false);
        }

        base.ProcessDamageResponseLocal(_dmResponse);
    }

    public override void MarkToUnload() {
        if (!bWillRespawn)
        {
            base.MarkToUnload();
        }
    }

    public override bool CanBePushed() {
        return true;
    }

    protected override float getNextStepSoundDistance() {
        return !IsRunning ? 0.5f : 0.25f;
    }

    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit,
        float _impulseScale) {
        if (IsOnMission()) return 0;

        if (EntityUtilities.GetBoolValue(entityId, "Invulnerable"))
            return 0;

        if (Buffs.HasBuff("buffInvulnerable"))
            return 0;

        // If we are being attacked, let the state machine know it can fight back
        if (!EntityTargetingUtilities.CanTakeDamage(this, world.GetEntity(_damageSource.getEntityId())))
            return 0;

        var damage = base.DamageEntity(_damageSource, _strength, _criticalHit, _impulseScale);
        return damage;
    }

    public override void SetDead() {
        _leaderUtils?.SetDead();
        bWillRespawn = false;
        if (NavObject != null)
        {
            NavObjectManager.Instance.UnRegisterNavObject(NavObject);
            NavObject = null;
        }

        base.SetDead();
    }

    public void TeleportToPlayer(EntityAlive target, bool randomPosition = false) {
        _leaderUtils?.TeleportToPlayer(target, randomPosition);
    }


    public override string EntityName {
        get {
            // No configured name? return the default.
            if (_strMyName == "Bob")
                return entityName;

            if (string.IsNullOrEmpty(_strTitle))
                return Localization.Get(_strMyName);
            return Localization.Get(_strMyName) + " the " + Localization.Get(_strTitle);
        }
        set {
            if (value.Equals(entityName)) return;

            entityName = value;

            // Don't set the internal name if it's the name of the entity class, since the
            // EntityFactory calls the setter with the class name when it creates the entity.
            // But set the internal name otherwise, because the setter is also called when the
            // entity is re-created after being picked up and placed again.
            if (value?.Equals(EntityClass.list[entityClass].entityClassName) != true)
            {
                _strMyName = value;
            }

            bPlayerStatsChanged |= !isEntityRemote;
        }
    }

    public override void Write(BinaryWriter _bw, bool bNetworkWrite) {
        base.Write(_bw, bNetworkWrite);
        if (NewName != "")
        {
            _bw.Write(NewName);
        }
        else
        {
            _bw.Write(_strMyName);
        }

        _bw.Write(factionId);
        Buffs.Write(_bw);
    }

    public override void Read(byte _version, BinaryReader _br) {
        base.Read(_version, _br);
        _strMyName = _br.ReadString();
        factionId = _br.ReadByte();
        Buffs.Read(_br);
    }

    public override bool CanEntityJump() {
        return true;
    }

    public override void OnUpdateLive() {
        _leaderUtils?.LeaderUpdate();
        _pushOutOfBlocksUtils.CheckStuck(position, width, depth);
        _npcUtils?.CheckCollision();
        _npcUtils?.CheckFallAndGround();
        base.OnUpdateLive();
    }



    public override EntityActivationCommand[] GetActivationCommands(Vector3i _tePos, EntityAlive _entityFocusing) {
        if (this.IsDead() || NPCInfo == null)
        {
            Debug.Log("NPC info is null.");
            return new EntityActivationCommand[0];
        }

        return new EntityActivationCommand[] {
            new EntityActivationCommand("talk", "talk", true),
            new EntityActivationCommand("talk", "talk", true),
            new EntityActivationCommand("talk", "talk", true)

        };
    }

    public override bool OnEntityActivated(int _indexInBlockActivationCommands, Vector3i _tePos,
        EntityAlive _entityFocusing) {
        var vector2 = _entityFocusing.position;
        SetLookPosition(vector2);
        RotateTo(vector2.x, vector2.y + 2, vector2.z, 8f, 8f);

        _entityFocusing.Buffs.SetCustomVar("CurrentNPC", entityId);
        Buffs.SetCustomVar("CurrentPlayer", _entityFocusing.entityId);

        var uiforPlayer = LocalPlayerUI.GetUIForPlayer(_entityFocusing as EntityPlayerLocal);
        uiforPlayer.xui.Dialog.Respondent = this;
        uiforPlayer.windowManager.CloseAllOpenWindows(null, false);
        uiforPlayer.windowManager.Open("dialog", true, false, true);

        return false;
    }

    // Helper Method to access the protected HandleNavObject of the base class
    public new void HandleNavObject() {
        base.HandleNavObject();
    }

    // We don't have access to JumpState outside of the class, so we can make a nifty public helper to get the value.
    public JumpState GetJumpState() {
        return jumpState;
    }

    public virtual TileEntity GetTileEntity() {
        var chunk = GameManager.Instance.World.GetChunkFromWorldPos(World.worldToBlockPos(position)) as Chunk;
        if (_tileEntity == null)
        {
            // TileEntityAoE is a basic tile entity, so we are just re-using it.
            _tileEntity = new TileEntityAoE(chunk) {
                entityId = entityId
            };
        }
        else
        {
            _tileEntity.SetChunk(chunk);
        }

        return _tileEntity;
    }

    public LeaderUtils GetLeaderUtils() {
        return _leaderUtils;
    }
}