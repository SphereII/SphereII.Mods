using System.Collections.Generic;
using UnityEngine;

public class EAIBreakBlockSDX : EAIBreakBlock
{
    public int attackDelay;
    public float damageBoostPercent;
    public List<Entity> allies = new List<Entity>();


    public override void Update()
    {
        EntityMoveHelper moveHelper = theEntity.moveHelper;
        if (attackDelay > 0)
        {
            attackDelay--;
        }
        if (attackDelay <= 0)
        {
            AttackBlock();
        }
    }

    private WorldRayHitInfo GetHitInfo(out float damageScale)
    {
        EntityMoveHelper moveHelper = theEntity.moveHelper;
        damageScale = moveHelper.DamageScale + damageBoostPercent;
        return moveHelper.HitInfo;
    }

    private void AttackBlock()
    {
        theEntity.SetLookPosition(Vector3.zero);
        ItemActionAttackData itemActionAttackData = theEntity.inventory.holdingItemData.actionData[0] as ItemActionAttackData;
        if (itemActionAttackData == null)
        {
            return;
        }
        damageBoostPercent = 0f;
        Bounds bb = new Bounds(theEntity.position, new Vector3(1.7f, 1.5f, 1.7f));
        theEntity.world.GetEntitiesInBounds(typeof(EntityZombie), bb, allies);
        for (int i = allies.Count - 1; i >= 0; i--)
        {
            if ((EntityZombie)allies[i] != theEntity)
            {
                damageBoostPercent += 0.2f;
            }
        }
        allies.Clear();
        itemActionAttackData.hitDelegate = new ItemActionAttackData.HitDelegate(GetHitInfo);
        if (theEntity.Attack(false))
        {
            theEntity.IsBreakingBlocks = true;
            float num = 0.25f + base.RandomFloat * 0.8f;
            if (theEntity.moveHelper.IsUnreachableAbove)
            {
                num *= 0.5f;
            }
            attackDelay = (int)((num + 0.75f) * 20f);
            theEntity.Attack(true);
        }
    }
}
