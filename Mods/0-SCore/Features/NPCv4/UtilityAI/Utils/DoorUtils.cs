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

            if (!Block.list[block.type].HasTag(BlockTags.Door) || (block.meta & 1) != 0)
                return false;

            // Respect locks.
            var doorComposite = GameManager.Instance.World.GetTileEntity(blockPos) as TileEntityComposite;
            var doorLockable = doorComposite?.GetFeature<TEFeatureLockable>();
            if (doorLockable != null && doorLockable.IsLocked())
                return false;

            SphereCache.AddDoor(context.Self.entityId, blockPos);
            EntityUtilities.OpenDoor(context.Self.entityId, blockPos);

            // ContinueWith runs on a thread pool thread. CloseDoor activates the door block and
            // writes SphereCache.DoorCache, both of which the AI touches from the main thread every
            // tick, so the continuation only queues the close back onto the main thread.
            Task.Delay(AIConstants.DoorAutoCloseDelayMs)
                .ContinueWith(t =>
                    ThreadManager.AddSingleTaskMainThread("SCore.UAI.CloseDoor",
                        delegate { CloseDoor(context, blockPos); }));

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
