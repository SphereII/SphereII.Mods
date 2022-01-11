using HarmonyLib;
using UnityEngine;

namespace Harmony.EntityAlive
{
    public class AddScriptToTransform
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "ComponentMapper";

        [HarmonyPatch(typeof(EModelBase))]
        [HarmonyPatch("Init")]
        public class EmodelInitCommon
        {
            private static void Postfix(EModelBase __instance, Entity ___entity)
            {
                if (__instance.GetModelTransform() == null)
                    return;

                         
                // Add the animation Bridge to all entities that do not have it.
                __instance.GetModelTransform().gameObject.GetOrAddComponent<AnimationEventBridge>();

                // Add any missing Root TransformRef Entity scripts where a CollisionCallForward exists.
                foreach (var collisionCallForward in __instance.GetComponentsInChildren<CollisionCallForward>())
                {
                    collisionCallForward.transform.gameObject.GetOrAddComponent<RootTransformRefEntity>();
                }

                // Check if this feature is enabled.
                if (Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                {
                    EntityUtilities.Traverse(__instance.GetModelTransformParent()?.gameObject);
                }
            }
        }
    }
}