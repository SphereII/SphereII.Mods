using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UAI;

namespace SCore.Harmony.MiniTurret
{
    [HarmonyPatch(typeof(AutoTurretFireController))]
    [HarmonyPatch("shouldIgnoreTarget")]
    public class AutoTurretFireControllerShouldIgnoreTarget
    {
        private static void Postfix(AutoTurretFireController __instance, ref bool __result, Entity _target)
        {
            var entity = GameManager.Instance.World.GetEntity(__instance.TileEntity.OwnerEntityID) as EntityAlive;
            if (entity == null) return;

            // If we shouldn't ignore them, do a quick ally check
            if (!__result)
            {
                if (SCoreUtils.IsAlly(entity, _target))
                    __result = true;
                return;
            }

            // If they are our enemy, target them.
            __result = !SCoreUtils.IsEnemy(entity, _target);
            return;
        }
    }

    [HarmonyPatch(typeof(MiniTurretFireController))]
    [HarmonyPatch("shouldIgnoreTarget")]
    public class MiniTurretFireControllerShouldIgnoreTarget
    {
        private static void Postfix(MiniTurretFireController __instance, ref bool __result, Entity _target)
        {
            var entity = GameManager.Instance.World.GetEntity(__instance.entityTurret.belongsPlayerId) as EntityAlive;
            if (entity == null) return;

            // If we shouldn't ignore them, do a quick ally check
            if (!__result)
            {
                if ( SCoreUtils.IsAlly(entity, _target))
                    __result = true;
                return;
            }

            // If they are our enemy, target them.
            __result =  !SCoreUtils.IsEnemy(entity, _target);
            return;
        }
    }
}
