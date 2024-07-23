using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace Harmony.TileEntities {
    public class TileEntitySignGif {
        private static readonly bool EnableExtendedSigns =
            Configuration.CheckFeatureStatus("AdvancedPlayerFeatures", "ExtendedSigns");
        //
        // [HarmonyPatch(typeof(SmartTextMesh))]
        // [HarmonyPatch("CanRenderString")]
        // public class SmartTextMeshCanRenderString {
        //     public static bool Prefix(SmartTextMesh __instance, string _text) {
        //         if (!EnableExtendedSigns) return true;
        //         return !_text.StartsWith("http");
        //     }
        // }


        [HarmonyPatch(typeof(TileEntitySign))]
        [HarmonyPatch("RefreshTextMesh")]
        public class TileEntitySignSetText {
            public static bool Prefix(TileEntitySign __instance) {
                if (GameManager.IsDedicatedServer)
                    return true;
                var smartTextMesh = __instance.smartTextMesh;
                if (smartTextMesh == null) return true;
                var parentTransform = smartTextMesh.transform.parent;
                var signMesh = parentTransform.transform.GetChild(0);
                var prefab = parentTransform.transform.GetChild(1);
                var textMesh = prefab.gameObject.GetComponent<TextMesh>();

                if (EnableExtendedSigns)
                {
                    var text = __instance.signText.Text;
                    if (text.StartsWith("http"))
                    {
                        var wrapper = signMesh.gameObject.GetOrAddComponent<ImageWrapper>();
                        // Check for supported url, and do some converting if necessary
                        if (!wrapper.ValidURL(ref text)) return true;
                        if (wrapper.IsNewURL(text))
                        {
                            wrapper.Pause();
                            wrapper.Init(text);

                            __instance.SetModified();
                        }
                        return true;
                    }
                }

                var wrapper2 = signMesh.transform.gameObject.GetComponent<ImageWrapper>();
                if (wrapper2 != null)
                    Object.Destroy(wrapper2);

                return true;
            }
        }
    }
}