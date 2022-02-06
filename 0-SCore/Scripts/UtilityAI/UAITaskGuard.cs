// using this namespace is necessary for Utilities AI Tasks
//       <task class="Guard, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using UnityEngine;

namespace UAI
{
    public class UAITaskGuard : UAITaskBase
    {
        private float _timeOut = 100f;
        private float _currentTimeout;
        public override void Start(Context _context)
        {
            base.Start(_context);
            _currentTimeout = _timeOut;
        }
        public override void Update(Context _context)
        {
            var entityAliveSDX = UAIUtils.ConvertToEntityAlive(_context.ActionData.Target) as EntityAliveSDX;
            if (entityAliveSDX == null)
            {
                Stop(_context);
                return;
            }
        
            base.Update(_context);

            // Don't do anything until the entity touches the ground; avoid the free in mid-air scenario.
            if (!_context.Self.onGround) return;


            // Have the NPC stand in the center of the block, and path to it.
            var entityPosition = EntityUtilities.CenterPosition(entityAliveSDX.position);
            var guardPosition = EntityUtilities.CenterPosition(entityAliveSDX.guardPosition);
            if (guardPosition != entityPosition)
            {
                entityAliveSDX.moveHelper.SetMoveTo(guardPosition, true);
                return;
            }
            else // Turn around or face the same way a player did they spoke to you.
            {
                entityAliveSDX.SetLookPosition(entityAliveSDX.guardLookPosition);
                entityAliveSDX.RotateTo(entityAliveSDX.guardLookPosition.x, entityAliveSDX.guardLookPosition.y, entityAliveSDX.guardLookPosition.z, 30f, 30f);
                
            }

          
            // Check if a player is in your bounds, and face them if they are.
            // ...But only if you're not asleep.
            var leader = EntityUtilities.GetLeaderOrOwner(_context.Self.entityId) as EntityAlive;

            // Check if a player is in your bounds, and face them if they are.
            SCoreUtils.TurnToFaceEntity(_context, leader);

            //if (!_context.Self.IsSleeping)
            //{
            //    var entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(_context.Self, new Bounds(_context.Self.position, Vector3.one * 5f));
            //    if (entitiesInBounds.Count > 0)
            //    {
            //        Entity lookEntity = null;

            //        foreach (var entity in entitiesInBounds)
            //        {
            //            // Prioritize your leader over non-leader players
            //            if (leader != null && entity.entityId == leader.entityId)
            //            {
            //                lookEntity = entity;
            //                break;
            //            }

            //            if (entity is EntityPlayerLocal || entity is EntityPlayer)
            //            {
            //                if (EntityTargetingUtilities.IsEnemy(_context.Self, entity))
            //                    continue;

            //                lookEntity = entity;
            //            }
            //        }

            //        if (lookEntity != null)
            //            SCoreUtils.SetLookPosition(_context, lookEntity);
            //    }
            //}


            _currentTimeout--;
            if (_currentTimeout < 0) Stop(_context);
        }
    }
}