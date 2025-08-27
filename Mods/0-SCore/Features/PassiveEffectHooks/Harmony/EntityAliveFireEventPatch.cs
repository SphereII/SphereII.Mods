using HarmonyLib;
using UnityEngine;

namespace SCore.Features.PassiveEffectHooks.Harmony
{
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch(nameof(EntityAlive.FireEvent))]
    public class EntityAliveFireEventPatch
    {
        public static void Postfix(EntityAlive __instance, MinEventTypes _eventType)
        {
            switch (_eventType)
            {
                case MinEventTypes.onSelfExplosionDamagedOther:
                case MinEventTypes.onSelfExplosionAttackedOther:
                    break;
                default:
                    return;
            }

            
            if (!__instance.isEntityRemote) return;
            
            var minEventContext = __instance.MinEventContext;
            var otherId = -1;
            if (minEventContext.Other)
                otherId = minEventContext.Other.entityId;
            
            var package = NetPackageManager.GetPackage<NetPackageMinEventFire>();
            if (minEventContext.ItemValue != null)
                package.Setup(__instance.entityId, otherId, _eventType, minEventContext.ItemValue);
            else
                package.Setup(__instance.entityId, otherId, _eventType, minEventContext.BlockValue);
            SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(package, false, __instance.entityId);
            
            
        }
    }
}