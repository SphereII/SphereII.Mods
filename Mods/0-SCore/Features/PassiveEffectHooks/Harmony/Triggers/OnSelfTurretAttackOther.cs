using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks
{
    [HarmonyPatch(typeof(ItemActionAttack))]
    [HarmonyPatch(nameof(ItemActionAttack.Hit))]
    public class ItemActionAttackHitPatchTurrents
    {
        public static void Postfix(ItemActionAttack __instance, int _attackerEntityId, WorldRayHitInfo hitInfo,
            int ownedEntityId, ItemValue damagingItemValue)
        {
            // Check for the turret
            if (GameManager.Instance.World.GetEntity(ownedEntityId) is not EntityTurret entityTurret) return;
            if (hitInfo?.tag == null) return;
            if (!hitInfo.tag.StartsWith("E_")) return;

            // The player
            var entityAlive = GameManager.Instance.World.GetEntity(_attackerEntityId) as EntityAlive;
            if (entityAlive == null) return;

            // Don't credit anyone else's turrets
            if (entityTurret.Owner.entityId != _attackerEntityId) return;
            
            var minEventContext2 = entityAlive.MinEventContext;

            // The zombie?
            var entity = ItemActionAttack.FindHitEntityNoTagCheck(hitInfo, out string text4);
            if (entity is not EntityAlive entityAlive2) return;
            if (!entity.CanDamageEntity(_attackerEntityId)) return;

            minEventContext2.Other = entityAlive2;
            minEventContext2.ItemValue = damagingItemValue;
            if (entityAlive2 && entityAlive2.RecordedDamage.Strength > 0)
            {
                if (entityAlive.isEntityRemote)
                {
                    SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                        NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, entityAlive2.entityId,
                            (MinEventTypes)SCoreMinEventTypes.onSelfTurretDamagedOther, entityTurret.OriginalItemValue), false, entityAlive.entityId);
                }
                else
                {
                    entityAlive.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfTurretDamagedOther);    
                }
            }

            if (!entity.IsDead()) return;
            if (entityAlive.isEntityRemote)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(
                    NetPackageManager.GetPackage<NetPackageMinEventFire>().Setup(entityAlive.entityId, entityAlive2.entityId,
                        (MinEventTypes)SCoreMinEventTypes.onSelfTurretKilledOther, entityTurret.OriginalItemValue), false, entityAlive.entityId);
            }
            else
            {
                entityAlive.FireEvent((MinEventTypes)SCoreMinEventTypes.onSelfTurretKilledOther);
            }

        }
    }
}