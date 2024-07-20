using HarmonyLib;
using UnityEngine;
using UnityEngine.Video;

namespace Harmony.TileEntities {
    public class TileEntitySignGif {
        private static readonly bool enableExtendedSigns =
            Configuration.CheckFeatureStatus("AdvancedPlayerFeatures", "ExtendedSigns");

        [HarmonyPatch(typeof(TileEntitySign))]
        //[HarmonyPatch("SetText")]
        [HarmonyPatch("RefreshTextMesh")]
        public class TileEntitySignSetText {
            public static bool Prefix(TileEntitySign __instance) {
                if (GameManager.IsDedicatedServer)
                    return true;
                var smartTextMesh = __instance.smartTextMesh;
                if (smartTextMesh == null) return true;
                if (enableExtendedSigns)
                {
                    var text = __instance.signText.Text;
                    if (text.Contains("youtube"))
                        text = "";
                    if (text.StartsWith("http"))
                    {
                        // var videoPlayer = smartTextMesh.transform.parent.transform.GetComponent<SCoreVideo>();
                        // if (videoPlayer == null)
                        //     videoPlayer = smartTextMesh.transform.parent.transform.gameObject.AddComponent<SCoreVideo>();
                        // videoPlayer.Configure(text);
                        var wrapper = smartTextMesh.transform.parent.transform.gameObject.GetOrAddComponent<ImageWrapper>();
                        // Check for supported url, and do some converting if necessary
                        if (!wrapper.ValidURL(ref text))
                        {
                            Debug.Log("ImageWrapper: Only supported files: .gif, .gifs, .jpg, and .png");
                            return true;
                        }
                        
                        if (wrapper.IsNewURL(text))
                        {
                            wrapper.Pause();
                            wrapper.Init(text);
                        
                            __instance.SetModified();
                        }

                     //  smartTextMesh.gameObject.SetActive(false);
                    
                        return true;
                    }
                }

                var wrapper2 = smartTextMesh.transform.parent.transform.GetComponent<ImageWrapper>();
                if (wrapper2 != null)
                    GameObject.Destroy(wrapper2);

                return true;
            }
        }
    }
}