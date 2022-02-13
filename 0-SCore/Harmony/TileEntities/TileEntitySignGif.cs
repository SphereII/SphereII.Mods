using HarmonyLib;
using UnityEngine;

namespace Harmony.TileEntities
{
    public class TileEntitySignGif
    {

        private static readonly bool enableExtendedSigns = Configuration.CheckFeatureStatus("AdvancedPlayerFeatures", "ExtendedSigns");

        [HarmonyPatch(typeof(TileEntitySign))]
        [HarmonyPatch("SetText")]
        public class TileEntitySignSetText
        {
            public static bool Prefix(TileEntitySign __instance, SmartTextMesh ___smartTextMesh, string _text)
            {
                if (GameManager.IsDedicatedServer)
                    return true;

                if (___smartTextMesh == null) return true;
                if (enableExtendedSigns)
                {
                    if (_text.StartsWith("http"))
                    {
                        var wrapper = ___smartTextMesh.transform.parent.transform.GetComponent<ImageWrapper>();
                        if (wrapper == null)
                            wrapper = ___smartTextMesh.transform.parent.transform.gameObject.AddComponent<ImageWrapper>();

                        // Check for supported url, and do some converting if necessary
                        if (!wrapper.ValidURL(ref _text))
                        {
                            Debug.Log("ImageWrapper: Only supported files: .gif, .gifs, .jpg, and .png");
                            return true;
                        }

                        if (wrapper.IsNewURL(_text))
                        {
                            wrapper.Pause();
                            wrapper.Init(_text);

                            __instance.SetModified();
                        }

                        ___smartTextMesh.gameObject.SetActive(false);
                        return true;
                    }
                }

                var wrapper2 = ___smartTextMesh.transform.parent.transform.GetComponent<ImageWrapper>();
                if (wrapper2 != null)
                    GameObject.Destroy(wrapper2);
                
                return true;
            }
        }
    }
}