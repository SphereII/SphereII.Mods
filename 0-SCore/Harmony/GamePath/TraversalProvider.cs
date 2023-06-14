using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using GamePath;
using System.Reflection;
using UnityEngine;
using Pathfinding;

namespace Harmony
{
    public class GamePathPatches
    {
        private static readonly string AdvFeatureClass = "AdvancedZombieFeatures";
        private static readonly string Feature = "SmarterEntities";

        // Fixing for a reverse condition for isGameStarted
        [HarmonyPatch(typeof(AstarVoxelGrid))]
        [HarmonyPatch("CheckHeights")]
        public class RecalculateCell
        {
            private static BlockValue GetBlock(IChunk chunk, int _x, int _y, int _z)
            {
                return _y is >= 256 or <= 0 ? BlockValue.Air : chunk.GetBlock(_x, _y, _z);
            }

            private static ushort CalcBlockingFlags(Vector3 pos, float offsetY, Vector2i[] ___neighboursOffsetV2)
            {
                PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
                int num = 0;
                pos.y += 0.2f + offsetY;
                Vector3 vector;
                vector.y = 0f;
                for (int i = 0; i < 4; i++)
                {
                    Vector2i vector2i = ___neighboursOffsetV2[i];
                    vector.x = (float)vector2i.x;
                    vector.z = (float)vector2i.y;
                    Vector3 origin = pos - vector * 0.2f;
                    RaycastHit raycastHit;
                    if (defaultPhysicsScene.SphereCast(origin, 0.1f, vector, out raycastHit, 0.59f, 1073807360, QueryTriggerInteraction.Ignore))
                    {
                        if (offsetY > 0.5f || raycastHit.normal.y < 0.643f)
                        {
                            num |= 1 << i;
                        }
                        else if (Vector3.Dot(vector, raycastHit.normal) > -0.35f)
                        {
                            num |= 1 << i;
                        }
                        else
                        {
                            num |= 256 << i;
                        }
                    }
                }
                return (ushort)num;
            }

            public static bool Prefix(AstarVoxelGrid __instance, ref int ___heightsUsed, ref Vector3 position,
                ref AstarVoxelGrid.HitData[] ___cellHits,
                ref AstarVoxelGrid.HitData[] ___heights,
                Vector2i[] ___neighboursOffsetV2)
            {

                // Check if this feature is enabled.
                if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature))
                    return true;

              //  __instance.cutCorners = false;
                ___heightsUsed = 0;
                int num = 0;
                Vector3 vector = position;
                vector.y += 320f;
                PhysicsScene defaultPhysicsScene = Physics.defaultPhysicsScene;
                Vector3 down = Vector3.down;
                AstarVoxelGrid.HitData hitData;
                hitData.blockerFlags = 4096;
                RaycastHit raycastHit;
                while (defaultPhysicsScene.Raycast(vector, down, out raycastHit, 320.01f, 1073807360, QueryTriggerInteraction.Ignore))
                {
                    vector.y = raycastHit.point.y - 0.11f;
                    hitData.point = raycastHit.point;
                    hitData.point.y = hitData.point.y + 0.05f;
                    ___cellHits[num] = hitData;
                    if (++num >= 512)
                    {
                        Log.Warning("AstarVoxelGrid CheckHeights too many hits");
                        break;
                    }
                }
                World world = GameManager.Instance.World;
                ChunkCluster chunkCache = world.ChunkCache;
                Vector3 position2 = Origin.position;
                int type = BlockValue.Air.type;
                int num2 = Utils.Fastfloor(position.x + position2.x);
                int num3 = Utils.Fastfloor(position.z + position2.z);
                Vector3i vector3i = new Vector3i(num2, 0, num3);
                Vector3i vector3i2 = vector3i;
                IChunk chunkFromWorldPos = world.GetChunkFromWorldPos(vector3i);
                if (chunkFromWorldPos == null)
                {
                    return true;
                }
                int x = World.toBlockXZ(num2);
                int z = World.toBlockXZ(num3);
                int i = 0;
                float num4 = float.MaxValue;
            IL_791:
                while (i < num)
                {
                    float num5 = num4;
                    hitData = ___cellHits[i++];
                    num4 = hitData.point.y;
                    float num6 = num5 - num4;
                    vector3i.y = Utils.Fastfloor(num4 + position2.y);
                    BlockValue block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
                    int type2 = block.type;
                    Block block2 = block.Block;


                    if (block2.shape.IsTerrain())
                    {
                        AstarVoxelGrid.HitData[] array = ___heights;
                        int num7 = ___heightsUsed;
                        ___heightsUsed = num7 + 1;
                        array[num7] = hitData;
                    }
                    else if (block2 is BlockModelTree )
                    {
                        hitData.point.y = vector.y + 0.03f;
                        hitData.blockerFlags = 4111;
                        AstarVoxelGrid.HitData[] array4 = ___heights;
                        int num7 = ___heightsUsed;
                        ___heightsUsed = num7 + 1;
                        array4[num7] = hitData;
                        continue;
                    }
                    else
                    {
                        if (num6 > 0.95f)
                        {
                            if (block2.PathType > 0)
                            {
                                float num8 = (float)Utils.Fastfloor(hitData.point.y);
                                if (hitData.point.y - num8 > 0.4f)
                                {
                                    vector3i.y++;
                                    block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
                                    type2 = block.type;
                                    block2 = block.Block;
                                    num4 = num8 + 1.01f;
                                    hitData.point.y = num4;
                                }
                            }
                            int num7;
                            if (block2.PathType > 0)
                            {
                                hitData.blockerFlags = 4111;
                            }
                            else
                            {
                                if (type2 != type)
                                {
                                    hitData.blockerFlags |= CalcBlockingFlags(hitData.point, 0.2f, ___neighboursOffsetV2);
                                    Vector2 pathOffset = block2.GetPathOffset((int)block.rotation);
                                    hitData.point.x = hitData.point.x + pathOffset.x;
                                    hitData.point.z = hitData.point.z + pathOffset.y;
                                }
                                vector3i2.y = vector3i.y + 1;
                                BlockValue block3 = GetBlock(chunkFromWorldPos, x, vector3i2.y, z);
                                Block block4 = block3.Block;
                                if (block2.HasTag(BlockTags.Door) || block2.HasTag(BlockTags.ClosetDoor))
                                {
                                    if (!block2.isMultiBlock || !block.ischild || block.parenty == 0)
                                    {
                                        hitData.blockerFlags |= 16384;
                                    }
                                }
                                else if (block4.HasTag(BlockTags.Door) && (!block4.isMultiBlock || !block3.ischild || block3.parenty == 0))
                                {
                                    hitData.blockerFlags |= 16384;
                                }
                                if (num6 > 2.95f && (block2.IsElevator((int)block.rotation) || block4.IsElevator((int)block3.rotation)))
                                {
                                    hitData.blockerFlags |= 8192;
                                    Vector3i vector3i3 = vector3i2;
                                    BlockValue blockValue = block3;
                                    Block block5 = block4;
                                    int num9 = (int)num5 - 1;
                                    int num10 = 0;
                                    while (vector3i3.y <= num9)
                                    {
                                        if (block5.IsElevator((int)blockValue.rotation))
                                        {
                                            num10 = 0;
                                        }
                                        else
                                        {
                                            if (!blockValue.isair || num10 >= 1)
                                            {
                                                break;
                                            }
                                            num10++;
                                        }
                                        vector3i3.y++;
                                        blockValue = GetBlock(chunkFromWorldPos, x, vector3i3.y, z);
                                        block5 = blockValue.Block;
                                    }
                                    vector3i3.y -= num10;
                                    Vector3 vector2 = vector;
                                    float num11 = num4 + position2.y - -0.2f;
                                    while ((float)vector3i3.y > num11)
                                    {
                                        vector2.y = (float)vector3i3.y - position2.y;
                                        AstarVoxelGrid.HitData hitData2;
                                        hitData2.blockerFlags = (ushort)(8192 | CalcBlockingFlags(vector2, 0f, ___neighboursOffsetV2));
                                        hitData2.point.x = vector2.x;
                                        hitData2.point.z = vector2.z;
                                        hitData2.point.y = vector2.y + -0.2f;
                                        AstarVoxelGrid.HitData[] array2 = ___heights;
                                        num7 = ___heightsUsed;
                                        ___heightsUsed = num7 + 1;
                                        array2[num7] = hitData2;
                                        vector3i3.y--;
                                    }
                                }
                            }
                            AstarVoxelGrid.HitData[] array3 = ___heights;
                            num7 = ___heightsUsed;
                            ___heightsUsed = num7 + 1;
                            array3[num7] = hitData;
                        }
                        float num12 = float.MinValue;
                        if (i < num)
                        {
                            num12 = ___cellHits[i].point.y;
                        }
                        for (; ; )
                        {
                            vector3i.y--;
                            vector.y = (float)vector3i.y - position2.y;
                            if (vector.y <= num12)
                            {
                                goto IL_791;
                            }
                            if (vector3i.y < 0)
                            {
                                break;
                            }
                            block = GetBlock(chunkFromWorldPos, x, vector3i.y, z);
                            type2 = block.type;
                            if (type2 == type)
                            {
                                goto IL_791;
                            }
                            block2 = block.Block;
                            if (block2.shape.IsTerrain() || block2.IsElevator())
                            {
                                goto IL_791;
                            }
                            if (!block2.HasTag(BlockTags.Door) && block2.PathType > 0 && block2.IsMovementBlocked(world, vector3i, block, BlockFace.Top))
                            {
                                bool flag = true;
                                for (int j = 0; j < 4; j++)
                                {
                                    Vector2i vector2i = ___neighboursOffsetV2[j];
                                    Vector3i vector3i4 = vector3i;
                                    vector3i4.x += vector2i.x;
                                    vector3i4.z += vector2i.y;
                                    block = chunkCache.GetBlock(vector3i4);

                                    block2 = block.Block;


                                    if (block2.PathType <= 0 || !block2.IsMovementBlocked(world, vector3i4, block, BlockFace.Top))
                                    {
                                        vector3i4.y--;
                                        block = chunkCache.GetBlock(vector3i4);
                                        block2 = block.Block;
                                        if (block2.PathType > 0)
                                        {
                                            flag = false;
                                        }
                                        else if (block2.IsMovementBlocked(world, vector3i4, block, BlockFace.Top))
                                        {
                                            Vector3 origin;
                                            origin.x = (float)vector3i4.x - position2.x;
                                            origin.y = vector.y + 0.51f;
                                            origin.z = (float)vector3i4.z - position2.z;
                                            RaycastHit raycastHit2;
                                            if (defaultPhysicsScene.Raycast(origin, down, out raycastHit2, 1.6f, 1073807360, QueryTriggerInteraction.Ignore))
                                            {
                                                flag = false;
                                                break;
                                            }
                                            break;
                                        }
                                    }
                                }
                                if (!flag)
                                {
                                    hitData.point.y = vector.y + 0.03f;
                                    hitData.blockerFlags = 4111;
                                    AstarVoxelGrid.HitData[] array4 = ___heights;
                                    int num7 = ___heightsUsed;
                                    ___heightsUsed = num7 + 1;
                                    array4[num7] = hitData;
                                }
                            }
                        }
                        i = int.MaxValue;
                    }
                }
                return false;
            }

        }
        //[HarmonyPatch()]
        //class GamePathASPPathFinder
        //{
        //    static MethodBase TargetMethod()
        //    {
        //        // ASPPathFinder is an internal class, and can't be targetting by regular annotations.
        //        var type = AccessTools.TypeByName("GamePath.ASPPathNavigate");
        //        return AccessTools.Method(type, "SetPath");
        //    }
        //    static bool Postfix(bool __result, PathInfo _pathInfo, float _speed, ref PathEntity ___currentPath)
        //    {
        //        if (_pathInfo == null) return __result;
        //        if (___currentPath == null) return __result;

        //        if (!EntityClass.list[_pathInfo.entity.entityClass].UseAIPackages)
        //            return __result;


        //        if (___currentPath.getCurrentPathLength() == 0) return __result;
        //        for (int i = 0; i < ___currentPath.getCurrentPathLength(); i++)
        //        {
        //            PathPoint pathPointFromIndex = ___currentPath.getPathPointFromIndex(i);
        //            var block = GameManager.Instance.World.GetBlock(new Vector3i(pathPointFromIndex.xCoord, pathPointFromIndex.yCoord, pathPointFromIndex.zCoord));
        //            if (block.Block is BlockTrunkTip)
        //            {
        //                //                        Debug.Log("Removing spike...");
        //                ___currentPath.setCurrentPathLength(i - 1);
        //                break;
        //            }
        //            if (block.Block is BlockModelTree)
        //            {
        //                //                      Debug.Log("Removing tree...");
        //                ___currentPath.setCurrentPathLength(i - 1);
        //                break;
        //            }
        //        }

        //        return __result;
        //    } //etc
        //}

    //    [HarmonyPatch()]
    //    class GamePathTraversalProvider
    //    {
    //        static MethodBase TargetMethod()
    //        {
    //            // ASPPathFinder is an internal class, and can't be targetting by regular annotations.
    //            var type = AccessTools.TypeByName("GamePath.TraversalProvider");
    //            return AccessTools.Method(type, "CanTraverse");
    //        }
    //        static bool Prefix(Path path, GraphNode node)
    //        {
    //            Debug.Log($"Node: {node.Tag} Nod Flags: {node.Flags} {node.position} : Walkable: {node.Walkable} : Enabled tags: {path.enabledTags} {(int)node.Tag & 1}  Penalty: {node.Penalty}");
    //            return true;
    //        } //etc
    //    }

    //    [HarmonyPatch()]
    //    class GamePathTraversalProviderNoBreak
    //    {
    //        static MethodBase TargetMethod()
    //        {
    //            // ASPPathFinder is an internal class, and can't be targetting by regular annotations.
    //            var type = AccessTools.TypeByName("GamePath.TraversalProviderNoBreak");
    //            return AccessTools.Method(type, "CanTraverse");
    //        }
    //        static bool Prefix(Path path, GraphNode node)
    //        {
    //            Debug.Log($"NoBreak: Node: {node.Tag} Nod Flags: {node.Flags} {node.position} : Walkable: {node.Walkable} : Enabled tags: {path.enabledTags} {(int)node.Tag & 1} Penalty: {node.Penalty}");
    //            return true;
    //        } //etc
    //    }
    }
}