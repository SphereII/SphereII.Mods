using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;
using Component = System.ComponentModel.Component;

namespace Harmony.TileEntities {
    public class TileEntitySignGif {
        private static readonly bool EnableExtendedSigns =
            Configuration.CheckFeatureStatus("AdvancedPlayerFeatures", "ExtendedSigns");

        [HarmonyPatch(typeof(SmartTextMesh))]
        [HarmonyPatch(nameof(SmartTextMesh.CanRenderString))]
        public class SmartTextMeshCanRenderString {
            public static bool Prefix(ref bool __result, SmartTextMesh __instance, string _text) {
                if (!EnableExtendedSigns) return true;
                // This will sometimes null ref if the signs have had a http://, but no longer does.
                // This is to protect against timing issues when switching from a http:// to a non-http:// text.
                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(TileEntitySign))]
        [HarmonyPatch(nameof(TileEntitySign.RefreshTextMesh))]
        public class TileEntitySignSetText {
            public static bool Prefix(TileEntitySign __instance) {
                if (GameManager.IsDedicatedServer)
                    return true;
                var smartTextMesh = __instance.smartTextMesh;
                if (smartTextMesh == null) return true;
                var parentTransform = smartTextMesh.transform.parent;
                if (parentTransform.transform.childCount < 2)
                    return true;
                var signMesh = parentTransform.transform.GetChild(0);
                var prefab = parentTransform.transform.GetChild(1);
                
                // use a third child to hole a separate copy of the signMesh prefab, so we can make adjustments,
                // while also letting us revert back to a regular sign.
                Transform wrapperPrefab = null;
                if (parentTransform.transform.childCount > 2)
                    wrapperPrefab = parentTransform.transform.GetChild(2);
                if (wrapperPrefab == null)
                {
                    var go = Object.Instantiate(signMesh.gameObject, parentTransform);
                    go.name = "WrapperPrefab";
                    wrapperPrefab = go.transform;
                    wrapperPrefab.SetAsLastSibling();
                }

                if (EnableExtendedSigns)
                {
                    var text = __instance.signText.Text;
                    if (text.StartsWith("http"))
                    {
                        var wrapper = wrapperPrefab.gameObject.GetOrAddComponent<ImageWrapper>();
                        // Check for supported url, and do some converting if necessary
                        if (!wrapper.ValidURL(ref text)) return true;
                        if (!wrapper.IsNewURL(text)) return true;
                        wrapper.Pause();
                        wrapper.Init(text);

                        __instance.SetModified();
                        signMesh.gameObject.SetActive(false);
                        prefab.gameObject.SetActive(false);
                        return true;
                    }
                }

                if (wrapperPrefab != null)
                    Object.Destroy(wrapperPrefab.gameObject);
                if ( prefab != null)
                    prefab.gameObject.SetActive(true);
                if ( signMesh != null)
                    signMesh.gameObject.SetActive(true);
                
                return true;
            }
        }
    }
}
