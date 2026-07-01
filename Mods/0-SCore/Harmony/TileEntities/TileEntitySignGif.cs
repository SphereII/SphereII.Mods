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


        [HarmonyPatch(typeof(TEFeatureSignable))]
        [HarmonyPatch(nameof(TEFeatureSignable.RefreshTextMesh))]
        public class TileEntitySignSetText {
            public static bool Prefix(TEFeatureSignable __instance) {
                if (!EnableExtendedSigns) return true;
                if (GameManager.IsDedicatedServer) return true;
                var smartTextMeshes = __instance.smartTextMesh;
                if (smartTextMeshes == null || smartTextMeshes.Length == 0) return true;
                var smartTextMesh = smartTextMeshes[0];
                if (smartTextMesh == null) return true;
                var parentTransform = smartTextMesh.transform.parent;
                if (parentTransform.transform.childCount < 2)
                    return true;
                var signMesh = parentTransform.transform.GetChild(0);
                var prefab = parentTransform.transform.GetChild(1);

                // Use a third child as a separate copy of the signMesh prefab for media display,
                // so we can revert back to a regular sign. Locate by name to avoid mistakenly
                // treating real prefab children (index 2+) as the wrapper on multi-child blocks.
                Transform wrapperPrefab = null;
                for (var i = 2; i < parentTransform.transform.childCount; i++)
                {
                    if (parentTransform.transform.GetChild(i).name == "WrapperPrefab")
                    {
                        wrapperPrefab = parentTransform.transform.GetChild(i);
                        break;
                    }
                }

                var text = __instance.signText.Text;
                if (text.StartsWith("http"))
                {
                    if (wrapperPrefab == null)
                    {
                        var go = Object.Instantiate(signMesh.gameObject, parentTransform);
                        go.name = "WrapperPrefab";
                        wrapperPrefab = go.transform;
                        wrapperPrefab.SetAsLastSibling();
                    }

                    var wrapper = wrapperPrefab.gameObject.GetOrAddComponent<ImageWrapper>();
                    if (!wrapper.ValidURL(ref text)) return true;
                    if (!wrapper.IsNewURL(text)) return true;
                    wrapper.Pause();
                    wrapper.Init(text);

                    __instance.SetModified();
                    signMesh.gameObject.SetActive(false);
                    prefab.gameObject.SetActive(false);
                    return true;
                }

                // Non-URL text: remove any media wrapper and restore the normal sign mesh.
                if (wrapperPrefab != null)
                    Object.Destroy(wrapperPrefab.gameObject);
                if (prefab != null)
                    prefab.gameObject.SetActive(true);
                if (signMesh != null)
                    signMesh.gameObject.SetActive(true);

                return true;
            }
        }
    }
}
