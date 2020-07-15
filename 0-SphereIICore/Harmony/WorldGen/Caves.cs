using DMT;
using GamePath;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;
using WorldGenerationEngine;

public class SphereII_CaveProject
{
    private static string AdvFeatureClass = "CaveConfiguration";
    private static string Feature = "CaveEnabled";
    public class SphereII_CaveProject_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + this.GetType().ToString());

            // Reduce extra logging stuff
            Application.SetStackTraceLogType(UnityEngine.LogType.Log, StackTraceLogType.None);
            Application.SetStackTraceLogType(UnityEngine.LogType.Warning, StackTraceLogType.None);

            var harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }




    // caveBlock02 is used as air blocks below the terrain, so we need to add a check here, so we can replace it with another block.
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("overlapsWithOtherBlock")]
    public class SphereII_CaveProject_overlapsWithOtherBlock
    {
        public static bool Prefix(Block __instance, WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue)
        {

            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            if (!__instance.isMultiBlock)
            {
                int type = _world.GetBlock(_clrIdx, _blockPos).type;
                return ( type != 0 || Block.list[type].GetBlockName() == "caveBlock02") && !Block.list[type].blockMaterial.IsGroundCover && !Block.list[type].blockMaterial.IsLiquid;
            }
            byte rotation = _blockValue.rotation;
            for (int i = __instance.multiBlockPos.Length - 1; i >= 0; i--)
            {
                int type2 = _world.GetBlock(_clrIdx, _blockPos + __instance.multiBlockPos.Get(i, _blockValue.type, (int)rotation)).type;
                if (type2 != 0 && !Block.list[type2].blockMaterial.IsGroundCover && !Block.list[type2].blockMaterial.IsLiquid)
                {
                    return true;
                }
            }
            return false;
        }
    }


    // caveBlock02 is used as air blocks below the terrain, so we need to add a check here, so we don't get floating blocks.
    [HarmonyPatch(typeof(Block))]
    [HarmonyPatch("OnNeighborBlockChange")]
    public class SphereII_CaveProject_OnNeighborBlockChange
    {
        public static bool Prefix(Block __instance, WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue, Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue)
        {

            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;

            // skip this check if its terrain
            if (_myBlockValue.Block.shape.IsTerrain())
                return true;

            // if the block that's changed is an air block, and it's below the block, crumble it, since we don't want it floating.
            if (_newNeighborBlockValue.Block.GetBlockName() == "caveBlock02" && _blockPosThatChanged == _myBlockPos + Vector3i.down)
            {
                var block = world.GetBlock(_myBlockPos);
                block.Block.DamageBlock(world, _clrIdx, _myBlockPos, _myBlockValue, block.Block.MaxDamage, -1);
                return false;

            }

            return true;
        }
    }

    // caveBlock02 is used as air blocks below the terrain, so we need to add a check here, so we can replace it with another block.
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch("PickupBlockServer")]
    public class SphereII_CaveProject_PickupBlockServer
    {
        public static bool Prefix(GameManager __instance, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _playerId)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return true;


            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendToServer(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_clrIdx, _blockPos, _blockValue, _playerId), false);
                return true;
            }
            if (__instance.World.GetBlock(_clrIdx, _blockPos).type != _blockValue.type)
            {
                return true;
            }
            if (__instance.World.IsLocalPlayer(_playerId))
            {
                __instance.PickupBlockClient(_clrIdx, _blockPos, _blockValue, _playerId);
            }
            else
            {
                SingletonMonoBehaviour<ConnectionManager>.Instance.SendPackage(NetPackageManager.GetPackage<NetPackagePickupBlock>().Setup(_clrIdx, _blockPos, _blockValue, _playerId), false, _playerId, -1, -1, -1);
            }
            BlockValue blockValue = (Block.list[_blockValue.type].PickupSource != null) ? Block.GetBlockValue(Block.list[_blockValue.type].PickupSource, false) : BlockValue.Air;
            if ( blockValue.type == BlockValue.Air.type && _blockPos.y < __instance.World.GetTerrainHeight( _blockPos.x, _blockPos.z))
                blockValue = new BlockValue((uint)Block.GetBlockByName("caveBlock02", false).blockID);
            __instance.World.SetBlockRPC(_blockPos, blockValue);

            return false;
        }
    }

    [HarmonyPatch(typeof(TerrainGeneratorWithBiomeResource))]
    [HarmonyPatch("GenerateTerrain")]
    [HarmonyPatch(new Type[] { typeof(World), typeof(Chunk), typeof(GameRandom), typeof(Vector3i), typeof(Vector3i), typeof(bool) })]
    public class SphereII_CaveProject_TerrainGeneratorWithBiomoeResource
    {
        public static void Postfix( Chunk _chunk )
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Allow us to throttle which chunks get caves or not by creating a list of world positions.
            SphereCache.GenerateCaveChunks();
            SphereII_CaveTunneler.AddCaveToChunk(_chunk);
        }
    }


    [HarmonyPatch(typeof(WorldDecoratorBlocksFromBiome))]
    [HarmonyPatch("decoratePrefabs")]
    public class SphereII_CaveProject_WorldDecoratorBlocksFromBiome
    {
        public static void Postfix(Chunk _chunk)
        {
            if(!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                return;

            // Allow us to throttle which chunks get caves or not by creating a list of world positions.
            SphereCache.GenerateCaveChunks();
            SphereII_CaveTunneler.AddDecorationsToCave(_chunk);
        }
    }
}
