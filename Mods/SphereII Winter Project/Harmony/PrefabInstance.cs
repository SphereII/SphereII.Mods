using HarmonyLib;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading;
using GlobalSnowEffect;
using UnityEngine;

/*
   public class PrefabInstance
   {
	   public PrefabInstance(int _id, string _filename, Vector3i _position, byte _rotation, Prefab _bad, int _standaloneBlockSize)
       {
       }
   }
*/
public class SphereII_WinterProject
{
   
    public static string modFolder;

    public class SphereIIWinterProjectInit : IModApi {
        private GlobalSnow _globalSnow;
        private EntityPlayerLocal _player;

        public void InitMod(Mod _modInstance) {
            Log.Out(" Loading Patch: " + GetType());

            // Reduce extra logging stuff
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());

            if (GameManager.IsDedicatedServer)
                return;

            RegisterEvents();

        }

        private void RegisterEvents() {
            ModEvents.GameUpdate.RegisterHandler(UpdateSnow);
        }

        private void InitGlobalSnow() {
            _player = GameManager.Instance.World.GetPrimaryPlayer();

            // Asset Bundles are defined in GlobalSnow.ReadFromModlet(), triggered from GlobalSnow.LoadResources.
            _globalSnow = _player.playerCamera.gameObject.GetOrAddComponent<GlobalSnow>();

            // Let the game handle the snow particles
            _globalSnow.snowfall = true;

            _globalSnow.snowAmount = 1f;

            // Let the game handle its own frost
            _globalSnow.cameraFrost = false;

            // cover the ground a bit more.
            _globalSnow.groundCoverage = 0.50f;

            _globalSnow.slopeThreshold = 0.9f;

            // Foot prints will only appear at a certain depth of snow amount.
            _globalSnow.footprints = true;
            _globalSnow.groundCheck = GROUND_CHECK.CharacterController;
            _globalSnow.characterController = _player.RootTransform.GetComponent<CharacterController>();

        }

        private void UpdateSnow() {
            if (_globalSnow == null)
                InitGlobalSnow();

        }

    }

    [HarmonyPatch(typeof(DynamicPrefabDecorator))]
    [HarmonyPatch("AddPrefab")]
    public class SphereII_DynamicPrefabDecorator
    {
        public static bool Prefix(DynamicPrefabDecorator __instance, PrefabInstance _pi)
        {

            if (_pi.prefab.size.y < 11)
                return false;

            // Prefabs with too great of an offset should be removed.
            // Example: Size y size 30 with an offset of -25 would only be 5 above terrain; not visible.
            if (_pi.prefab.size.y - Math.Abs(_pi.prefab.yOffset) < 11)
                return false;

            // Keep the trader above.
            if (_pi.prefab.PrefabName.Contains("trader_hugh"))
                return true;

            // Check if the current thread has a name. the GenerateWorlds for RWG has a named thread; the others do not.
            if (Thread.CurrentThread.Name != null)
                return true;

            // Sink the prefab into the ground
            // This also sinks the SleeperVolumes, so they work as expected in clear quests.
            var depth = Configuration.GetPropertyValue("WinterProject", "SnowDepth");
            if (string.IsNullOrEmpty(depth))
                return true;
            
            _pi.boundingBoxPosition.y -= StringParsers.ParseSInt32(depth);
            return true;
        }

    }
    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("readBlockData")]
    public class SphereII_WinterProject_readBlockData
    {
        public static bool Postfix(bool __result, ref Prefab __instance, ref List<string> ___allowedZones)
        {
            if (!__result) return __result;
            if (__instance.PrefabName.Contains("trader_hugh")) return __result;
            __instance.bTraderArea = false;
            __instance.bExcludeDistantPOIMesh = true;
            __instance.bCopyAirBlocks = true;
            return __result;
        }

    }


    // Sinks the prefabs
    [HarmonyPatch(typeof(Prefab))]
    [HarmonyPatch("CopyIntoLocal")]
    public class SphereII_WinterProject_Prefab_Prefix
    {
        public static void Postfix(Prefab __instance, Vector3i _destinationPos, ChunkCluster _cluster, FastTags<TagGroup.Poi> _questTags)
        {

            if (__instance.Tags.Test_AllSet(FastTags<TagGroup.Poi>.Parse("SKIP_HARMONY_COPY_INTO_LOCAL")))
                return;

            if (!__instance.PrefabName.Contains("trader_hugh"))
                WinterProjectPrefab.SetSnowPrefab(__instance, _cluster, _destinationPos, _questTags);
        }

    }



    
    // Biome Decorations
    [HarmonyPatch(typeof(PrefabInstance))]
    [HarmonyPatch("CopyIntoChunk")]
    public class SphereII_WinterProject_PrefabInstance_CopyIntoChunk
    {
        public static void Postfix(PrefabInstance __instance, Chunk _chunk)
        {

            if (!__instance.prefab.PrefabName.Contains("trader_hugh"))
                WinterProjectPrefab.SetSnowChunk(_chunk, __instance.boundingBoxPosition, __instance.boundingBoxSize);
        }
    }


    // Rally Flag is too high for buried supplies, and other Create-type quests.
    // This is due to the offset on the flag marker.
    [HarmonyPatch(typeof(ObjectiveRallyPoint))]
    [HarmonyPatch("HandleRallyPoint")]
    public class SphereII_WinterProject_ObjectiveRallyPoint_HandleRallyPoint
    {
        public static void Postfix(ObjectiveRallyPoint __instance, Vector3i ___rallyPos, ObjectiveRallyPoint.RallyStartTypes ___RallyStartType)
        {
            if (___RallyStartType == ObjectiveRallyPoint.RallyStartTypes.Find) return;
            
            var blockValue = Block.GetBlockValue("questRallyMarker", false);
            var position = ___rallyPos;
            var snowDepth = Configuration.GetPropertyValue("WinterProject", "SnowDepth");
            if (string.IsNullOrEmpty(snowDepth))
                position.y -= 7;
            else
            {
                var depth = StringParsers.ParseSInt32(snowDepth);
                position.y -= depth - 1;
            }
            GameManager.Instance.World.SetBlockRPC(position, blockValue, sbyte.MaxValue);
            __instance.OwnerQuest.SetPositionData(Quest.PositionDataTypes.Activate, position);

        }
    }

}
