using System.Collections.Generic;
using UnityEngine;

namespace UAI
{
    public class UAIConsiderationHasPath : UAIConsiderationTargetType
    {
        public override float GetScore(Context _context, object target)
        {
            var paths = SphereCache.GetPaths(_context.Self.entityId) ?? ScanForTileEntities(_context);
            if (paths.Count == 0)
                return 0f;

            SphereCache.AddPaths(_context.Self.entityId, paths);
            return 1f;
        }

        private List<Vector3> ScanForTileEntities(Context _context)
        {
            var paths = new List<Vector3>();
            var blockPosition = _context.Self.GetBlockPosition();
            var chunkX = World.toChunkXZ(blockPosition.x);
            var chunkZ = World.toChunkXZ(blockPosition.z);

            for (var i = -1; i < 2; i++)
            {
                for (var j = -1; j < 2; j++)
                {
                    var chunk = (Chunk)_context.Self.world.GetChunkSync(chunkX + j, chunkZ + i);
                    if (chunk == null) continue;

                    var tileEntities = chunk.GetTileEntities();
                    for (var k = 0; k < tileEntities.list.Count; k++)
                    {
                        var position = tileEntities.list[k].ToWorldPos().ToVector3();
                        paths.Add(position);
                    }
                }
            }

            // sort the paths to keep the closes one.
            paths.Sort(new SCoreUtils.NearestPathSorter(_context.Self));
            return paths;
        }
    }
}