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
            public static bool Prefix(global::EntityAlive ___entityAlive, Transform ___bipedTransform, ref Transform ___head, ref Transform ___leftUpperLeg, ref Transform ___rightUpperLeg,
                ref Transform ___leftUpperArm, ref Transform ___rightUpperArm, ref float ___limbScale)
            {
                var transform = ___bipedTransform.FindInChilds("Hips");
                if (transform == null)
                    return false;

                // Head is sometimes there, sometimes called something else. It's only really used to calculate the limbScale, which we've zeroed out, since
                // we don't support de limb of the animals.
                ___head = ___bipedTransform.FindInChilds("Head");
                if (___head == null)
                    ___head = ___bipedTransform.FindInChilds("Bip01_Head");

                ___leftUpperLeg = ___bipedTransform.FindInChilds("LeftUpLeg");
                ___rightUpperLeg = ___bipedTransform.FindInChilds("RightUpLeg");
                ___leftUpperArm = ___bipedTransform.FindInChilds("LeftArm");
                ___rightUpperArm = ___bipedTransform.FindInChilds("RightArm");
                if (___head == null)
                    ___limbScale = 0f;
                else
                    ___limbScale = (___head.position - transform.position).magnitude * 1.32f / ___head.lossyScale.x;


                return false;
            }
        }
    }
}