using HarmonyLib;
using System.Collections.Generic;

namespace Harmony.NPCFeatures
{
    internal class EntityAliveStartingEquipment
    {
        [HarmonyPatch(typeof(global::EntityAlive))]
        [HarmonyPatch("PostInit")]
        public class EntityAliveStartingEquipmentPostInit
        {
            private static void SetupStartingItems(global::EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
            {
                for (var i = 0; i < ___itemsOnEnterGame.Count; i++)
                {
                    var itemStack = ___itemsOnEnterGame[i];
                    var forId = ItemClass.GetForId(itemStack.itemValue.type);
                    if (forId.HasQuality)
                        itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6);
                    else
                        itemStack.count = forId.Stacknumber.Value;
                    __instance.inventory.SetItem(i, itemStack);
                }
            }

            public static void Postfix(global::EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
            {
                if (__instance is EntityPlayerLocal)
                    return;

                //if (__instance.RootTransform.GetComponentsInChildren<AnimationEventBridge>().Length == 0)
                //{
                //    Debug.Log("Missing AnimationEventBridge, adding.");
                //    __instance.RootTransform.gameObject.AddComponent<AnimationEventBridge>();
                //    foreach (var component in __instance.RootTransform.GetComponentsInChildren<Component>())
                //    {
                //        //if (component is Animator)
                //        //{
                //        //    Debug.Log("Adding AnimationEvenBridge to " + component.ToString() + " Parent");
                //        //    component.gameObject.AddComponent<AnimationEventBridge>();
                //        //}

                //        foreach (var temp in component.GetComponents<Component>())
                //        {
                //            // For each component that has an AvatarRootMotion, add an animationEvenBridge to it.
                //            if (temp is AvatarRootMotion)
                //            {
                //                Debug.Log("Adding AnimationEvenBridge to " + component.ToString() + " Parent");
                //                if (component.gameObject.GetComponentsInChildren<AnimationEventBridge>().Length == 0)
                //                    component.gameObject.AddComponent<AnimationEventBridge>();
                //                break; // Only add one
                //            }
                //        }


                //    }
                //}
                //Debug.Log("Entity: " + __instance.EntityName);
                //foreach (var component in __instance.RootTransform.GetComponentsInChildren<Component>())
                //{
                //    Debug.Log("\tComponent: " + component.ToString());

                //    foreach (var temp in component.GetComponents<Component>())
                //    {
                //        Debug.Log("\t\tSub Component: " + temp.ToString() + " Tag: " + temp.tag);
                //    }
                //}

                if (___itemsOnEnterGame.Count <= 0) return;

                SetupStartingItems(__instance, ___itemsOnEnterGame);
                __instance.inventory.SetHoldingItemIdx(0);
            }
        }
    }
}