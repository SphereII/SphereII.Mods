using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;

namespace SCore.Features.ItemDegradation.Harmony.General
{
    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch(nameof(EntityAlive.ApplyLocalBodyDamage))]
    public class EntityAliveApplyLocalBodyDamage
    {
        public static void Postfix(global::EntityAlive __instance, DamageResponse _dmResponse)
        {
            if (__instance.equipment == null) return;

            var wornArmor = __instance.equipment.GetArmor();
            foreach (var armor in wornArmor)
            {
                if (armor.ItemClass is not ItemClassArmor armorItemClass)
                {
                    continue;
                }

                if (_dmResponse.ArmorSlot == armorItemClass.EquipSlot)
                {
                    ItemDegradationHelpers.CheckModificationOnItem(armor.Modifications, __instance);
                }
            }
            
        }
    }
}