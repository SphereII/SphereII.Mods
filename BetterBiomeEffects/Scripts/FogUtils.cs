using UnityEngine;
using VolumetricFogAndMist;
using XMLData.Parsers;

public class FogUtils
{
    private static string _defaultAssetBundle = $"#@modfolder({BetterBiomeEffects.modFolder}):Resources/VolumetricFog.unity3d";

    public static GameObject GetGameObject(string objectName)
    {
        return DataLoader.LoadAsset<GameObject>($"{_defaultAssetBundle}?{objectName}");
    }

    public static Texture2D GetTexture(string textureName)
    {
        return DataLoader.LoadAsset<Texture2D>($"{_defaultAssetBundle}?{textureName}");
    }

    public static Shader GetShader(string shaderName)
    {
        return DataLoader.LoadAsset<Shader>($"{_defaultAssetBundle}?{shaderName}");
    }
    public static void InitAssetBundles(string assets = "")
    {
        if (string.IsNullOrEmpty(assets))
        {
            DataLoader.PreloadBundle(_defaultAssetBundle);
            return;
        }
        DataLoader.PreloadBundle(assets);
    }

    public static void SetFogPreset(string preset, bool volume = false)
    {
        if (!EnumUtils.TryParse<FOG_PRESET>(preset, out var fogPreset, true))
        {
            Log.Out($"Invalid Fog Preset: {preset}");
            return;
        }
        Log.Out($"Setting Fog: {fogPreset}");
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        var volumetricFog = player.playerCamera.gameObject.GetOrAddComponent<VolumetricFog>();
        
        if (volume)
        {
            volumetricFog.instances.Clear();
            var boxSize = new Vector3i(500, 5, 500);
            var groundPosition = player.playerCamera.transform.position;
            groundPosition.y -= 1.8f;
            var surfaceFog = VolumetricFog.CreateFogArea(groundPosition, boxSize);
            surfaceFog.preset = fogPreset;
            volumetricFog.instances.Add(surfaceFog);
            return;
        }
        player.playerCamera.gameObject.GetOrAddComponent<VolumetricFogPosT>();
        volumetricFog.character = player.gameObject;
        volumetricFog.preset = fogPreset;
        
        return;

//        volumetricFog.character = player.gameObject;
//        volumetricFog.preset = fogPreset;

    }
    
    public static void SetupVolumes()
    {
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        var volumetricFog = player.playerCamera.gameObject.GetOrAddComponent<VolumetricFog>();
        volumetricFog.instances.Clear();

        var boxSize = new Vector3i(100, 10, 100);
        var playerPosition = player.GetPosition();
        var terrainHeight = GameManager.Instance.World.GetTerrainHeight((int) playerPosition.x, (int) playerPosition.z);
        var groundPosition = player.playerCamera.transform.position;
        groundPosition.y = terrainHeight;
        var surfaceFog = VolumetricFog.CreateFogArea(groundPosition, boxSize);
        surfaceFog.preset = FOG_PRESET.HeavyFog;
        Debug.Log($"Adding Surface Fog: {groundPosition}");
        volumetricFog.instances.Add(surfaceFog);
            
        var undergroundPosition = groundPosition;
        undergroundPosition.y -= 30;
        var undergroundFog = VolumetricFog.CreateFogArea(undergroundPosition, boxSize);
        undergroundFog.preset = FOG_PRESET.ToxicSwamp;
        Debug.Log($"Adding underground fog: {undergroundPosition}");
        volumetricFog.instances.Add(undergroundFog);
    }
}