using System;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Scripting;
using System.Linq; // Required for splitting and selecting random path

/*
 * XML Usage Example (Showing comma-delimited materials):
 * <append xpath="entity_classes/entity_class[@name='zombieArleneRadiated']">
       <effect_group name="ReplaceMaterial">
       <!-- The replace_material attribute now takes a comma-delimited list of paths -->
       <triggered_effect trigger="onSelfFirstSpawn" action="ReplaceMaterial, SCore" target_material_name="HD_Arlene_Radiated" replace_material="#@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad1, #@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad2, #@modfolder:Resources/ww_zeds_1.unity3d?HD_Arlene_Rad3"/>
       </effect_group>
    </append>
 */
[Preserve]
public class MinEventActionReplaceMaterial : MinEventActionTargetedBase
{
    // Holds the target material name from XML
    private string targetMaterialName = string.Empty;
    
    // Holds the comma-delimited string of replacement material paths from XML
    private string replaceMaterialPathsString = string.Empty;


    private void DebugInfo(string entity, string material, string selectedPath)
    {
        if (GameManager.IsDedicatedServer) return;
        if (GamePrefs.GetBool(EnumGamePrefs.DebugMenuShowTasks) )
        {
            // Log the selected path for debugging
            Log.Out($"ReplaceMaterial: Entity='{entity}', Target='{material}', Selected Replacement Path='{selectedPath}'");
        }
    }

    public override void Execute(MinEventParams _params)
    {
        // 1. Split the paths string into an array, removing empty entries and trimming whitespace.
        string[] paths = replaceMaterialPathsString
            .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToArray();

        // Check if we have any valid paths to proceed
        if (paths.Length == 0)
        {
            Log.Warning($"replace_material attribute is empty or invalid for target material '{targetMaterialName}'. Check your XML.");
            return;
        }

        // 2. Randomly select one path from the array.
        // UnityEngine.Random.Range(minInclusive, maxExclusive)
        int randomIndex = UnityEngine.Random.Range(0, paths.Length);
        string selectedPath = paths[randomIndex];

        Renderer[] renderers = _params.Self?.RootTransform?.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
        {
            return;
        }

        // 3. Load the replacement material only once.
        Material replaceMaterial = DataLoader.LoadAsset<Material>(selectedPath);

        if (replaceMaterial == null)
        {
            Log.Warning(
                $"Failed to replace target material '{targetMaterialName}'. The randomly selected material '{selectedPath}' could not be loaded!");
            return;
        }

        foreach (Renderer renderer in renderers)
        {
            // Get a copy of the materials array to modify
            Material[] materials = renderer.materials;

            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] == null || string.IsNullOrEmpty(materials[i].name))
                {
                    continue;
                }

                string currentMaterialName = materials[i].name;
                
                // Remove the (Instance) suffix if present
                if (currentMaterialName.EndsWith("(Instance)"))
                {
                    currentMaterialName = materials[i].name.Replace("(Instance)", string.Empty).Trim();
                }

                if (currentMaterialName == targetMaterialName)
                {
                    // Found a match, replace it
                    DebugInfo(_params.Self?.EntityName, currentMaterialName, selectedPath);
                    materials[i] = replaceMaterial;
                }
            }

            // Apply the modified materials array back to the renderer after checking all slots
            renderer.materials = materials;
        }
    }

    public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params)
    {
        return base.CanExecute(_eventType, _params) && _params.Self != null && _params.Self.world != null;
    }

    public override bool ParseXmlAttribute(XAttribute _attribute)
    {
        bool flag = base.ParseXmlAttribute(_attribute);
        if (!flag)
        {
            string localName = _attribute.Name.LocalName;
            if (localName == "target_material_name")
            {
                targetMaterialName = _attribute.Value;
                return true;
            }

            if (localName == "replace_material")
            {
                replaceMaterialPathsString = _attribute.Value;
                
                // Preload all bundles during XML parsing to reduce runtime lag.
                string[] paths = replaceMaterialPathsString
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();
                
                foreach (string path in paths)
                {
                    if (!string.IsNullOrEmpty(path))
                    {
                        DataLoader.PreloadBundle(path);
                    }
                }
            }
        }

        return flag;
    }
}
