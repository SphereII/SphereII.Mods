using System.Collections.Generic;
using UnityEngine;

public class EAIBreakBlockSDX : EAIBreakBlock
{
    public List<Entity> allies = new List<Entity>();
    public int attackDelay;
    public float damageBoostPercent;


    public override void Update()
    {
        var moveHelper = theEntity.moveHelper;
        if (attackDelay > 0) attackDelay--;
        if (attackDelay <= 0) AttackBlock();
    }

    private WorldRayHitInfo GetHitInfo(out float damageScale)
    {
        var moveHelper = theEntity.moveHelper;
        damageScale = moveHelper.DamageScale + damageBoostPercent;
        return moveHelper.HitInfo;
    }

    private void AttackBlock()
    {
        theEntity.SetLookPosition(Vector3.zero);
        var itemActionAttackData = theEntity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
        if (itemActionAttackData == null) return;
        damageBoostPercent = 0f;
        var bb = new Bounds(theEntity.position, new Vector3(1.7f, 1.5f, 1.7f));
        theEntity.world.GetEntitiesInBounds(typeof(EntityZombie), bb, allies);
        for (var i = allies.Count - 1; i >= 0; i--)
            if ((EntityZombie)allies[i] != theEntity)
                damageBoostPercent += 0.2f;
        allies.Clear();
        itemActionAttackData.hitDelegate = GetHitInfo;
        if (theEntity.Attack(false))
        {
            theEntity.IsBreakingBlocks = true;
            var num = 0.25f + RandomFloat * 0.8f;
            if (theEntity.moveHelper.IsUnreachableAbove) num *= 0.5f;
            attackDelay = (int)((num + 0.75f) * 20f);
            theEntity.Attack(true);
        }
    }
}