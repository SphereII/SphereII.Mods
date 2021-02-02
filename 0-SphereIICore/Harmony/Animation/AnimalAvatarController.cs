using HarmonyLib;
using System;
using UnityEngine;

public class SphereII_AnimalAvatarController
{

    [HarmonyPatch(typeof(AvatarAnimalController))]
    [HarmonyPatch("assignBodyParts")]
    public class SphereII_AvatarAnimalController_assignBodyParts
    {
        // To fix the mapping of some animals.
        public static bool Prefix(EntityAlive ___entityAlive, Transform ___bipedTransform, ref Transform ___head, ref Transform ___leftUpperLeg, ref Transform ___rightUpperLeg, ref Transform ___leftUpperArm, ref Transform ___rightUpperArm, ref float ___limbScale)
        {
            Transform transform = ___bipedTransform.FindInChilds("Hips", false);
            if (transform == null)
                return false;

            // Head is sometimes there, sometimes called something else. It's only really used to calculate the limbScale, which we've zeroed out, since
            // we don't support de-limbing of the animals.
            ___head = ___bipedTransform.FindInChilds("Head", false);
            if (___head == null)
                ___head = ___bipedTransform.FindInChilds("Bip01_Head", false);

            ___leftUpperLeg = ___bipedTransform.FindInChilds("LeftUpLeg", false);
            ___rightUpperLeg = ___bipedTransform.FindInChilds("RightUpLeg", false);
            ___leftUpperArm = ___bipedTransform.FindInChilds("LeftArm", false);
            ___rightUpperArm = ___bipedTransform.FindInChilds("RightArm", false);
            if (___head == null)
                ___limbScale = 0f;
            else
                ___limbScale = (___head.position - transform.position).magnitude * 1.32f / ___head.lossyScale.x;


            return false;
        }
    }

}