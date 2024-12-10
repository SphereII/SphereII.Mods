using System.Globalization;
using UnityEngine;

/*
     <property name="Class" value="TriggeredSDX, SCore"/>
     <property name="AlwaysActive" value="true" />

      <!-- How far out the tile entity will re-scan to detect the player -->
      <property name="ActivationDistance" value="5" />

       <!-- this triggers a SetTrigger("On") when looked at -->
      <property name="ActivateOnLook" value="true" />

    <!-- allows the block to be used as a storage device -->
      <property name="IsContainer" value="true" />

      <!-- Triggers the block if the buff buffCursed is active on the player, or if the player has a cvar called "cvar" with a value of 4, or if myOtherCvar is available, regardless of value -->
       <property name="ActivationBuffs" value="buffCursed,cvar(4),myOtherCvar" />

       <property name="CopyCVarToAnimator" value="cvarName1;mycvarName2" />

*/
internal class BlockTriggeredSDX : BlockLoot {
    private static readonly string AdvFeatureClass = "AdvancedTileEntities";
    private int RandomIndex;
    private bool _isLootContainer;
    private bool _activateOnLook;
    private string _copyCVars;
    private string _activationBuffs;


    public override void Init() {
        base.Init();

        // A Random index added to the animator, in case you want to use a random digit.
        if (Properties.Values.ContainsKey("RandomIndex"))
            RandomIndex = StringParsers.ParseSInt32(Properties.Values["RandomIndex"], 0, -1, NumberStyles.Any);

        // Is it a loot container? Should it show the interact prompt?
        if (Properties.Values.ContainsKey("IsContainer"))
            _isLootContainer = StringParsers.ParseBool(Properties.Values["IsContainer"]);

        // Should the block activate when you look at it?
        if (Properties.Values.ContainsKey("ActivateOnLook"))
            _activateOnLook = StringParsers.ParseBool(Properties.Values["ActivateOnLook"]);

        if (Properties.Values.ContainsKey("CopyCVarToAnimator"))
            _copyCVars = Properties.Values["CopyCVarToAnimator"];
        
        if (Properties.Values.ContainsKey("ActivationBuffs"))
            _activationBuffs = Properties.Values["ActivationBuffs"];
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos,
        EntityAlive _entityFocusing) {
        UpdateAnimator(_world, _blockPos, _entityFocusing);
        if (_activateOnLook)
        {
            TriggerActivationBuffs(_entityFocusing);
            ActivateBlock(_world, _clrIdx, _blockPos, _blockValue, true, true);
        }

        if (_isLootContainer)
        {
            return base.GetActivationText(_world, _blockValue, _clrIdx, _blockPos, _entityFocusing);
        }

        
        return "";
    }

    private void TriggerActivationBuffs(EntityAlive _entityAlive) {
        if (string.IsNullOrEmpty(_activationBuffs)) return;
        
        foreach( var buff in _activationBuffs.Split(';'))
        {
            _entityAlive.Buffs.AddBuff(buff);
        }
    }
    // don't open the loot container.
    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue,
        EntityPlayerLocal _player) {
        
        TriggerActivationBuffs(_player);
        UpdateAnimator(_world, _blockPos, _player);
        if (_isLootContainer)
        {
            return base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
        }

        return true;
    }

    private void UpdateAnimator(WorldBase _world, Vector3i _blockPos, EntityAlive _entityAlive) {
        
        if (string.IsNullOrEmpty(_copyCVars)) return;
        
        var ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null)
            return;

        var componentsInChildren = ebcd.transform.GetComponentsInChildren<Animator>();
        if (componentsInChildren == null) return;
        for (var i = componentsInChildren.Length - 1; i >= 0; i--)
        {
            var animator = componentsInChildren[i];

            foreach (var cvar in _copyCVars.Split(';'))
            {
                if (_entityAlive.Buffs.HasCustomVar(cvar))
                {
                    animator.SetFloat(cvar, _entityAlive.Buffs.GetCustomVar(cvar));
                }
            }
        }
    }

    public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue,
        bool isOn, bool isPowered) {
        // If there's no transform, no sense on keeping going for this class.
        var ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null)
            return false;
        
        var componentsInChildren = ebcd.transform.GetComponentsInChildren<Animator>();
        if (componentsInChildren == null) return true;
        for (var i = componentsInChildren.Length - 1; i >= 0; i--)
        {
            var animator = componentsInChildren[i];
   
            AdvLogging.DisplayLog(AdvFeatureClass,
                _blockValue.Block.GetBlockName() + ": Animator: " + animator.name + " : Active: " + isOn);
            if (isOn)
            {
                var random = Random.Range(0, RandomIndex);
                AdvLogging.DisplayLog(AdvFeatureClass,
                    _blockValue.Block.GetBlockName() + ": Random Index for " + animator.name + " Value: " + random);

                animator.SetInteger("RandomIndex", random);
                AdvLogging.DisplayLog(AdvFeatureClass,
                    _blockValue.Block.GetBlockName() + ": Setting Bool for On: True " + animator.name);
                animator.SetBool("On", true);
                AdvLogging.DisplayLog(AdvFeatureClass,
                    _blockValue.Block.GetBlockName() + ": Trigger for On: " + animator.name);
                animator.SetTrigger("TriggerOn");
            }
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass,
                    _blockValue.Block.GetBlockName() + ": Setting Bool for On: false" + animator.name);
                animator.SetBool("On", false);
                //  AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Turning Off Animator " + animator.name);
                //  animator.enabled = false;
            }
        }

        return true;
    }
}