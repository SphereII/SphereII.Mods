using HarmonyLib;
using UnityEngine;

namespace Harmony.TileEntities
{
    public class TileEntitySignGif
    {
        //[HarmonyPatch(typeof(TileEntitySign))]
        //[HarmonyPatch("SetBlockEntityData")]
        //public class SCoreTileEntitySign_SetBlockEntityData
        //{
        //    public static bool Prefix(TileEntitySign __instance, BlockEntityData _blockEntityData)
        //    {
        //        //if (_blockEntityData != null && _blockEntityData.bHasTransform && _blockEntityData.transform != null)
        //        //{
        //        //    if (!GameManager.IsDedicatedServer)
        //        //    {
        //        //     //   if (_blockEntityData.transform.GetComponentInChildren<TextMesh>() == null)
        //        //       //     _blockEntityData.transform.gameObject.AddComponent<TextMesh>();
        //        //    }
        //        //}
        //        return true;
        //    }
        //}

        [HarmonyPatch(typeof(TileEntitySign))]
        [HarmonyPatch("SetText")]
        public class TileEntitySignSetText
        {
            public static bool Prefix(TileEntitySign __instance, SmartTextMesh ___smartTextMesh, string _text)
            {
                if (GameManager.IsDedicatedServer)
                    return true;

                if (___smartTextMesh == null) return true;
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
                }
                else
                {
                    var wrapper = ___smartTextMesh.transform.parent.transform.GetComponent<ImageWrapper>();
                    if (wrapper != null)
                    {
                        //    wrapper.Reset();
                    }

                    ___smartTextMesh.gameObject.SetActive(true);
                }

                return true;
            }
        }
    }
}