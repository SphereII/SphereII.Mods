using HarmonyLib;
using UnityEngine;

namespace Harmony.Animation
{
    public class AnimalAvatarController
    {
        /// <summary>
        /// Used to fix some mappings for custom animals' who's body parts aren't named the expected way.
        /// </summary>
        [HarmonyPatch(typeof(AvatarAnimalController))]
        [HarmonyPatch("assignBodyParts")]
        public class SCoreAvatarAnimalControllerAssignBodyParts
        {
            public static bool Prefix(ref AvatarAnimalController __instance)
            {
                var transform = __instance.bipedT.FindInChilds("Hips");
                if (transform == null)
                    return false;

                // Head is sometimes there, sometimes called something else. It's only really used to calculate the limbScale, which we've zeroed out, since
                // we don't support de limb of the animals.
                __instance.head = __instance.bipedT.FindInChilds("Head");
                if (__instance.head == null)
                    __instance.head = __instance.bipedT.FindInChilds("Bip01_Head");
                
                __instance.leftUpperLeg = __instance.bipedT.FindInChilds("LeftUpLeg");
                __instance.rightUpperLeg = __instance.bipedT.FindInChilds("RightUpLeg");
                __instance.leftUpperArm = __instance.bipedT.FindInChilds("LeftArm");
                __instance.rightUpperArm = __instance.bipedT.FindInChilds("RightArm");
                if (__instance.head == null)
                    __instance.limbScale = 0f;
                else 
                    __instance.limbScale = (__instance.head.position - transform.position).magnitude * 1.32f / __instance.head.lossyScale.x;


                return false;
            }
        }
    }
}