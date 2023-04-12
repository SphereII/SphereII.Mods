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

*/
internal class BlockTriggeredSDX : BlockLoot
{
    private static readonly string AdvFeatureClass = "AdvancedTileEntities";
    private int RandomIndex;

    public override void Init()
    {
        base.Init();
        if (Properties.Values.ContainsKey("RandomIndex"))
            RandomIndex = StringParsers.ParseSInt32(Properties.Values["RandomIndex"], 0, -1, NumberStyles.Any);
    }

    public override string GetActivationText(WorldBase _world, BlockValue _blockValue, int _clrIdx, Vector3i _blockPos, EntityAlive _entityFocusing)
    {
        if (!_blockValue.Block.Properties.Values.ContainsKey("ActivateOnLook")) return "";
        var activateOnLook = StringParsers.ParseBool(_blockValue.Block.Properties.Values["ActivateOnLook"]);
        if (!activateOnLook) return "";
        AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Activating Block on GetActivationText");

        ActivateBlock(_world, _clrIdx, _blockPos, _blockValue, true, true);

        return "";
    }

    // don't open the loot container.
    public override bool OnBlockActivated(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
    {
        if (!_blockValue.Block.Properties.Values.ContainsKey("IsContainer")) return true;
        var isContainer = StringParsers.ParseBool(_blockValue.Block.Properties.Values["IsContainer"]);
        if (!isContainer) return true;
        base.OnBlockActivated(_world, _cIdx, _blockPos, _blockValue, _player);
        return true;

    }

    public override bool ActivateBlock(WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, bool isOn, bool isPowered)
    {
        // If there's no transform, no sense on keeping going for this class.
        var ebcd = _world.GetChunkFromWorldPos(_blockPos).GetBlockEntity(_blockPos);
        if (ebcd == null || ebcd.transform == null)
            return false;

        var componentsInChildren = ebcd.transform.GetComponentsInChildren<Animator>();
        if (componentsInChildren == null) return true;
        for (var i = componentsInChildren.Length - 1; i >= 0; i--)
        {
            var animator = componentsInChildren[i];

            AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Animator: " + animator.name + " : Active: " + isOn);
            if (isOn)
            {
                var random = Random.Range(0, RandomIndex);
                AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Random Index for " + animator.name + " Value: " + random);

                animator.SetInteger("RandomIndex", random);
                AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Setting Bool for On: True " + animator.name);
                animator.SetBool("On", true);
                AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Trigger for On: " + animator.name);
                animator.SetTrigger("TriggerOn");
            }
            else
            {
                AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Setting Bool for On: false" + animator.name);
                animator.SetBool("On", false);
                //  AdvLogging.DisplayLog(AdvFeatureClass, _blockValue.Block.GetBlockName() + ": Turning Off Animator " + animator.name);
                //  animator.enabled = false;
            }
        }

        return true;
    }
}