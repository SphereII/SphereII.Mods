
using HarmonyLib;
using UnityEngine;

// The XUiV_Texture in A19 calls Resources.UnloadAsset(), which fails when loading textures from the Mods folder as they are not in the Resource assets scope.
public class SphereII_XUiV_Texture
{
  

    [HarmonyPatch(typeof(XUiV_Texture))]
    [HarmonyPatch("UnloadTexture")]
    public class SphereII_XUiC_Texture_UnloadTexture
    {
        public static bool Prefix(XUiV_Texture __instance, ref UITexture ___uiTexture, ref string ___pathName)
        {

            if (__instance.Texture != null)
            {
                if (___pathName.Contains("modfolder"))
                {
                   // Debug.Log("SphereII: UnloadTexture() : " + ___pathName);
                    ___uiTexture.mainTexture = null;
                    __instance.Texture = null;
                    ___pathName = null;
                    return false;
                }
            }
            return true;
        }


    }

}