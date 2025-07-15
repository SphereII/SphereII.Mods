using System;
using System.Xml.Linq;
using UnityEngine;

public class FireConfig
{
    private const string AdvFeatureClass = "FireManagement";

    // Core settings
    public bool Enabled { get; private set; }
    public bool FireSpread { get; private set; }
    public float CheckInterval { get; private set; }
    public float FireDamage { get; private set; }
    public float ChanceToExtinguish { get; private set; }

    public bool Logging { get; private set; }
    
    // Smoke settings
    public float SmokeTime { get; private set; }
    public string SmokeParticle { get; private set; }

    // Fire settings
    public string FireParticle { get; private set; }
    public string FireSound { get; private set; }
    public float FireSoundRadius { get; private set; }

    // Light settings
    public int MaxLights { get; private set; }
    public float LightIntensity { get; private set; }
    public float LightRange { get; private set; }
    public Color LightColor { get; private set; }

    public string MaterialSurface { get; set; }

    public string MaterialDamage { get; set; }

    public string MaterialID { get; set; }
    
    // Network settings
    public float NetworkSyncInterval { get; private set; }
    public int MaxFiresPerUpdate { get; private set; }

    public static FireConfig LoadFromXml()
    {
        var config = new FireConfig();
        config.Initialize();
        return config;
    }

    private void Initialize()
    {
        try
        {
            // Core settings
            Enabled = ParseBoolSetting("FireEnable", true);
            FireSpread = ParseBoolSetting("FireSpread", true);
            CheckInterval = ParseFloatSetting("CheckInterval", 120f);
            FireDamage = ParseFloatSetting("FireDamage", 1f);
            ChanceToExtinguish = ParseFloatSetting("ChanceToExtinguish", 0.05f);
            Logging = ParseBoolSetting("Logging", false);
            
            // Smoke settings
            SmokeTime = ParseFloatSetting("SmokeTime", 60f);
            SmokeParticle = ParseStringSetting("SmokeParticle", "smokeParticle");
            BlockUtilitiesSDX.CheckAndLoadParticles(SmokeParticle);                

            // Fire settings
            FireSound = ParseStringSetting("FireSound", "fireSound");
            FireSoundRadius = ParseFloatSetting("FireSoundRadius", 10f);
            FireParticle = ParseStringSetting("FireParticle", "fireParticle");
            BlockUtilitiesSDX.CheckAndLoadParticles(FireParticle);
            
            // Light settings
            MaxLights = ParseIntSetting("MaxLights", 32);
            LightIntensity = ParseFloatSetting("LightIntensity", 1f);
            LightRange = ParseFloatSetting("LightRange", 5f);
            LightColor = ParseColorSetting("LightColor", Color.yellow);

            // Network settings
            NetworkSyncInterval = ParseFloatSetting("NetworkSyncInterval", 1f);
            MaxFiresPerUpdate = ParseIntSetting("MaxFiresPerUpdate", 100);

            // Material Settings
            MaterialID = ParseStringSetting("MaterialID", "");
            MaterialDamage = ParseStringSetting("MaterialDamage", "");
            MaterialSurface = ParseStringSetting("MaterialSurface", "");
            
            ValidateSettings();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading fire configuration: {ex.Message}");
            SetDefaultValues();
        }
    }

   
    private void ValidateSettings()
    {
        CheckInterval = Mathf.Max(0.1f, CheckInterval);
        FireDamage = Mathf.Max(0f, FireDamage);
        ChanceToExtinguish = Mathf.Clamp01(ChanceToExtinguish);
        SmokeTime = Mathf.Max(0f, SmokeTime);
        MaxLights = Mathf.Max(1, MaxLights);
        LightIntensity = Mathf.Max(0f, LightIntensity);
        LightRange = Mathf.Max(0f, LightRange);
        NetworkSyncInterval = Mathf.Max(0.1f, NetworkSyncInterval);
        MaxFiresPerUpdate = Mathf.Max(1, MaxFiresPerUpdate);
    }

    private void SetDefaultValues()
    {
        Enabled = true;
        FireSpread = true;
        CheckInterval = 120f;
        FireDamage = 1f;
        ChanceToExtinguish = 0.05f;
        SmokeTime = 60f;
        SmokeParticle = "smokeParticle";
        FireParticle = "fireParticle";
        FireSound = "fireSound";
        FireSoundRadius = 10f;
        MaxLights = 32;
        LightIntensity = 1f;
        LightRange = 5f;
        LightColor = Color.yellow;
        NetworkSyncInterval = 1f;
        MaxFiresPerUpdate = 100;
    }

    private bool ParseBoolSetting(string key, bool defaultValue)
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, key);
        return string.IsNullOrEmpty(value) ? defaultValue : StringParsers.ParseBool(value);
    }

    private float ParseFloatSetting(string key, float defaultValue)
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, key);
        return string.IsNullOrEmpty(value) ? defaultValue : StringParsers.ParseFloat(value);
    }

    private int ParseIntSetting(string key, int defaultValue)
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, key);
        return string.IsNullOrEmpty(value) ? defaultValue : int.Parse(value);
    }

    private string ParseStringSetting(string key, string defaultValue)
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, key);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    private Color ParseColorSetting(string key, Color defaultValue)
    {
        var value = Configuration.GetPropertyValue(AdvFeatureClass, key);
        if (string.IsNullOrEmpty(value)) return defaultValue;
        
        var parts = value.Split(',');
        if (parts.Length != 3 && parts.Length != 4) return defaultValue;

        try
        {
            float r = float.Parse(parts[0]) / 255f;
            float g = float.Parse(parts[1]) / 255f;
            float b = float.Parse(parts[2]) / 255f;
            float a = parts.Length == 4 ? float.Parse(parts[3]) / 255f : 1f;
            return new Color(r, g, b, a);
        }
        catch
        {
            return defaultValue;
        }
    }

    public override string ToString()
    {
        return $"FireConfig[Enabled={Enabled}, Spread={FireSpread}, Interval={CheckInterval}, " +
               $"Damage={FireDamage}, ExtinguishChance={ChanceToExtinguish}]";
    }
}