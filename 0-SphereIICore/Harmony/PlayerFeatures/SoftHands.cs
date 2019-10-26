using Harmony;

public class SphereII__SoftHands
{
    private static string AdvFeatureClass = "AdvancedPlayerFeatures";
    private static string Feature = "SoftHands";

    // Adds new feature where Durability affects the damage a weapon can do.
    [HarmonyPatch(typeof(ItemActionAttack))]
    [HarmonyPatch("Hit")]
    public class SphereII_ItemAction_Hit_EntityPlayerLocal
    {
        public static void Postfix(ItemActionAttack __instance, ItemActionAttack.AttackHitInfo _attackDetails, ref float _weaponCondition, int _attackerEntityId)
        {
            // Check if this feature is enabled.
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return ;

            EntityAlive entityAlive = GameManager.Instance.World.GetEntity(_attackerEntityId) as EntityAlive;
            if (entityAlive)
            {
                bool isWearingGloves = false;
                //LocalPlayerUI uiforPlayer = LocalPlayerUI.GetUIForPlayer(entityAlive as EntityPlayerLocal);
                //if(uiforPlayer)
                //{
                //    // Grab a hand item to see if its being worn.
                //    ItemValue handItems = ItemClass.GetItem("armorClothGloves", false );
                //    if(uiforPlayer.xui.PlayerEquipment.IsEquipmentTypeWorn( handItems ))
                //        isWearingGloves = true;
                //}

                //BlockValue blockValue = _attackDetails.blockBeingDamaged;
                //if (blockValue.type != 0 )
                //{
                //    if (blockValue.Block.blockMaterial.MaxDamage <= 1)
                //        isWearingGloves = true;
                //}
                // Check if its the player hand
                if (entityAlive.inventory.holdingItem.GetItemName() == "meleeHandPlayer" && _attackDetails.damageGiven > 0 && !isWearingGloves) 
                {
                    AdvLogging.DisplayLog(AdvFeatureClass, "Attacking Entity is an EntityAlive: " + entityAlive.inventory.holdingItemItemValue.ItemClass.GetItemName() + " Inflicting Damage");
                    DamageSource dmg = new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.Bashing);
                    entityAlive.DamageEntity(dmg, 1, false, 1f);
                }
            }

            return ;
        }
    }

  
}