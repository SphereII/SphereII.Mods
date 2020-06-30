using DMT;
using HarmonyLib;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
class SphereII_EntityAliveStartingEquipment_Equiment
{

    [HarmonyPatch(typeof(EntityAlive))]
    [HarmonyPatch("PostInit")]
    public class SphereII__EntityAliveStartingEquipment_PostInit
    {
        public static void SetupStartingItems(EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
        {
            for (int i = 0; i < ___itemsOnEnterGame.Count; i++)
            {
                ItemStack itemStack = ___itemsOnEnterGame[i];
                ItemClass forId = ItemClass.GetForId(itemStack.itemValue.type);
                if (forId.HasQuality)
                {
                    itemStack.itemValue = new ItemValue(itemStack.itemValue.type, 1, 6, false, null, 1f);
                }
                else
                {
                    itemStack.count = forId.Stacknumber.Value;
                }
                __instance.inventory.SetItem(i, itemStack);
            }
        }

        public static void Postfix(EntityAlive __instance, List<ItemStack> ___itemsOnEnterGame)
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

            if (___itemsOnEnterGame.Count > 0)
            {
                SetupStartingItems(__instance, ___itemsOnEnterGame);
                __instance.inventory.SetHoldingItemIdx(0);
            }
        }
    }
}

