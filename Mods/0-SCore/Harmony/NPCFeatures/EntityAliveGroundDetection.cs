using HarmonyLib;

namespace Harmony.NPCFeatures
{
    [HarmonyPatch(typeof(global::EntityAlive))]
    [HarmonyPatch("OnUpdateLive")]
    public class EntityBuffsRemoveBuffs
    {
        private static void Postfix(global::EntityAlive __instance)
        {
            switch (__instance)
            {
                case EntityZombie _:
                case EntityPlayerLocal _:
                case EntityPlayer _:
                case EntityVulture _:
                    return;
            }

            if (__instance.isEntityRemote)
                return;

            if (!__instance.emodel) return;

            var avatarController = __instance.emodel.avatarController;
            if (!avatarController) return;

            var flag = __instance.onGround || __instance.isSwimming;

            var canFall = !__instance.emodel.IsRagdollActive && __instance.bodyDamage.CurrentStun == EnumEntityStunType.None && !__instance.isSwimming && !__instance.IsDead();
            avatarController.SetFallAndGround(canFall, flag);
        }
    }
}