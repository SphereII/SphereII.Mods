using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

/*
 *
 *
    <append xpath="entity_classes/entity_class[@name='zombieArlene']">
		<property name="AltMats" value="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_COLD,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_FROZEN,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW HIGH,#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_SNOW LOW"/>
	</append>
	
 */
namespace Harmony.EntityAlive
{
    [HarmonyPatch(typeof(global::EntityAlive), nameof(global::EntityAlive.Init))]
    public class EntityTintMaterial_Patch
    {
        public static readonly string PropTintColor = "TintColor";
        public static readonly string PropTintMaterial = "TintMaterial";
        public static readonly string PropTintShaderProperties = "TintShaderProperties";

        private static readonly string multiDigitMatchPattern = @"\d+$";

        public static void Postfix(global::EntityAlive __instance)
        {
            DynamicProperties props = __instance.EntityClass?.Properties;

            if (!props.Values.ContainsKey(PropTintColor) && !props.Values.dic.Keys.Any(k => k.StartsWith(PropTintMaterial)))
            {
                return;
            }

            Renderer[] renderers = __instance.RootTransform?.GetComponentsInChildren<Renderer>();

            if (renderers == null || renderers.Length == 0)
            {
                return;
            }

            List<Material> materials = GetAllMaterials(renderers);
            string[] shaderProperties = ParseShaderProperties(props, ',');

            foreach (KeyValuePair<string, string> prop in props.Values.dic)
            {
                if (string.IsNullOrEmpty(prop.Key) || !prop.Key.StartsWith(PropTintMaterial))
                {
                    continue;
                }

                Match multiDigitMatch = Regex.Match(prop.Key, multiDigitMatchPattern);
                int materialIndex = StringParsers.ParseSInt32(multiDigitMatch.Value);

                Color tintMaterialColor = Color.white;
                props.ParseColorHex($"{PropTintMaterial}{materialIndex}", ref tintMaterialColor);

                if (tintMaterialColor != Color.white && materialIndex < materials.Count)
                {
                    for (int i = 0; i < shaderProperties.Length; i++)
                    {
                        Material currentMaterial = materials[materialIndex];

                        if (currentMaterial != null && currentMaterial.HasColor(shaderProperties[i]))
                        {
                            currentMaterial.SetColor(shaderProperties[i], tintMaterialColor);
                        }
                    }
                }
            }

            if (!props.Values.ContainsKey(PropTintColor))
            {
                return;
            }

            Color tintColor = Color.white;
            props.ParseColorHex(PropTintColor, ref tintColor);

            foreach (Material material in materials)
            {
                for (int i = 0; i < shaderProperties.Length; i++)
                {
                    if (material != null && material.HasColor(shaderProperties[i]))
                    {
                        material.SetColor(shaderProperties[i], tintColor);
                    }
                }
            }
        }

        private static List<Material> GetAllMaterials(Renderer[] renderers)
        {
            List<Material> materials = new List<Material>();
            foreach (Material material in renderers.SelectMany(r => r.materials))
            {
                materials.Add(material);
            }

            return materials;
        }

        private static string[] ParseShaderProperties(DynamicProperties props, char separator)
        {
            string tintShaderProperties = string.Empty;
            props.ParseString(PropTintShaderProperties, ref tintShaderProperties);

            return tintShaderProperties.Split(separator);
        }
    }

    [HarmonyPatch(typeof(AvatarController), nameof(AvatarController.DismemberLimb))]
    public class EntityTintMaterial_DismemberLimb_Patch
    {
        public static void Postfix(AvatarController __instance)
        {
            EntityTintMaterial_Patch.Postfix(__instance.entity);
        }
    }
}