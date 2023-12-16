using HarmonyLib;
//using System;
using System.Reflection;
using GlobalSnowEffect;
using UnityEngine;
using VolumetricFogAndMist;

public class BetterBiomeEffects
{
    public static string modFolder;

    public class BetterBiomeEffectsInit : IModApi
    {
        private GlobalSnow _globalSnow;
        private VolumetricFog _volumetricFog;
        private EntityPlayerLocal _player;
        
        private string resourceReference;

        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());

            // Reduce extra logging stuff
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());


            // Used to convert the bundle path correctly.
            // Example from GlobalSnow.cs:
            // private string resourceReference = $"#@modfolder({BetterBiomeEffects.modFolder}):Resources/GlobalSnow.unity3d?";
            modFolder = _modInstance.Name;

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            ModEvents.GameUpdate.RegisterHandler(UpdateSnow);
          //  ModEvents.GameUpdate.RegisterHandler(UpdateVolumetricFog);
        }

        private void InitGlobalSnow( )
        {
            _player = GameManager.Instance.World.GetPrimaryPlayer();
            // Asset Bundles are defined in GlobalSnow.ReadFromModlet(), triggered from GlobalSnow.LoadResources.
            _globalSnow = _player.playerCamera.gameObject.GetOrAddComponent<GlobalSnow>();
            
            // Let the game handle the snow particles
            _globalSnow.snowfall = false;

            // Let the game handle its own frost
            _globalSnow.cameraFrost = false;

            // cover the ground a bit more.
            _globalSnow.groundCoverage = 0.45f;

            // Foot prints? Don't quite work yet.
            _globalSnow.footprints = true;
            _globalSnow.groundCheck = GROUND_CHECK.CharacterController;
            _globalSnow.characterController = _player.PhysicsTransform.GetComponent<CharacterController>();
            
            // Biome test
            var biome = _player.biomeStandingOn;

        }

        private void UpdateSnow()
        {
            if (_globalSnow == null)
                InitGlobalSnow();

            if (_globalSnow == null) return;
            //_globalSnow.snowAmount = WeatherManager.Instance.GetCurrentSnowfallValue();

            // Vanilla coverage value? Good enough?
            _globalSnow.snowAmount = WeatherManager.currentWeather.snowCoverParam.value;

            // Give a bonus if we are in a snowy area, so we can layer everything in a fine dust.
            if (_player.biomeStandingOn?.m_SpectrumName == "snow")
                _globalSnow.snowAmount += 0.5f;
        }

        private void InitVolumetricFog()
        {
            _player = GameManager.Instance.World.GetPrimaryPlayer();
            FogUtils.InitAssetBundles();
          //  _player.playerCamera.gameObject.GetOrAddComponent<VolumetricFogPosT>();
            _volumetricFog = _player.playerCamera.gameObject.GetOrAddComponent<VolumetricFog>();
            //_volumetricFog.character = _player.gameObject;
          //  SetupVolumes();
            //    VolumetricFogConfiguration.SetVolumetricFogProfile("Fog");
            //Debug.Log($"Main Camera: {_volumetricFog.hasCamera} {_volumetricFog.distance}");
            
        }

     

      
        private void UpdateVolumetricFog()
        {
            if ( _volumetricFog == null) 
                InitVolumetricFog();
            
        }
    }
}