using Platform;
using System.Threading.Tasks;
using UnityEngine;

namespace UAI
{
    /// <summary>
    /// V4 door-interaction utilities: automatic door opening and deferred closing.
    /// <para>
    /// Differences from <see cref="SCoreUtils"/>:
    /// <list type="bullet">
    ///   <item>Magic delay value replaced with <see cref="AIConstants.DoorAutoCloseDelayMs"/>.</item>
    /// </list>
    /// </para>
    /// </summary>
    public static class DoorUtils
    {
        /// <summary>
        /// Checks whether the entity's path is blocked by a closed door it has permission to open.
        /// If so, opens the door, schedules an automatic close after
        /// <see cref="AIConstants.DoorAutoCloseDelayMs"/> milliseconds, clears the blocked state,
        /// and returns <c>true</c>.
        /// </summary>
        public static bool CheckForClosedDoor(Context context)
        {
            if (!(context.Self.moveHelper.BlockedTime >= SCoreConstants.BlockedTime))
                return false;

            // Only humans or hired entities may open doors.
            if (!EntityUtilities.IsHuman(context.Self.entityId) &&
                !EntityUtilities.IsHired(context.Self.entityId))
                return false;

            var blockPos = context.Self.moveHelper.HitInfo.hit.blockPos;
            var block    = GameManager.Instance.World.GetBlock(blockPos);

            if (!Block.list[block.type].HasTag(BlockTags.Door) || BlockDoor.IsDoorOpen(block.meta))
                return false;

            // Respect locks.
            if (GameManager.Instance.World.GetTileEntity(0, blockPos) is TileEntitySecureDoor securedDoor)
            {
                if (securedDoor.IsLocked())
                    return false;
            }

            SphereCache.AddDoor(context.Self.entityId, blockPos);
            EntityUtilities.OpenDoor(context.Self.entityId, blockPos);

            Task.Delay(AIConstants.DoorAutoCloseDelayMs)
                .ContinueWith(_ => CloseDoor(context, blockPos));

            context.Self.moveHelper.ClearBlocked();
            return true;
        }

        /// <summary>
        /// Closes the door at <paramref name="doorPos"/> and removes it from the entity's
        /// open-door cache.
        /// </summary>
        public static void CloseDoor(Context context, Vector3i doorPos)
        {
            EntityUtilities.CloseDoor(context.Self.entityId, doorPos);
            SphereCache.RemoveDoor(context.Self.entityId, doorPos);
        }
    }
}
