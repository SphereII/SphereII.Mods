using HarmonyLib;

namespace SCore.Harmony.Blocks
{
    public class UpdateCurrentBlockposAndValueFire
    {
        [HarmonyPatch(typeof(EntityAlive))]
        [HarmonyPatch("updateCurrentBlockPosAndValue")]
        public class SCoreBlock_updateCurrentBlockPosAndValue
        {
            public static void Postfix(EntityAlive __instance)
            {
                if (FireManager.Instance == null) return;

                Vector3i blockPosition = __instance.GetBlockPosition();
                if (FireManager.Instance.isBurning(blockPosition) && GameManager.Instance.HasBlockParticleEffect(blockPosition))
                {
                    var buff = Configuration.GetPropertyValue("FireManagement", "BuffOnFire");
                    if (!string.IsNullOrEmpty(buff))
                        __instance.Buffs.AddBuff(buff);
                }

            }
        }

    }
}
