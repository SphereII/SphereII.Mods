// using this namespace is necessary for Utilities AI Tasks
//       <task class="IdleSDX, SCore" />
// The game adds UAI.UAITask to the class name for discover.

using UnityEngine;

namespace UAI
{
    public class UAITaskIdleSDX : UAITaskBase
    {
        private float _timeOut = 100f;
        protected override void initializeParameters()
        {
            if (Parameters.ContainsKey("timeout")) _timeOut = StringParsers.ParseFloat(Parameters["timeout"]);
        }

        public override void Start(Context context)
        {
            base.Start(context);
        }
        public override void Update(Context context)
        {
            // Don't do anything until the entity touches the ground; avoid the free in mid-air scenario.
            if (!context.Self.onGround) return;

            EntityUtilities.Stop(context.Self.entityId, true);
            SCoreUtils.TurnToFaceEntity(context);

            var leader = EntityUtilities.GetLeaderOrOwner(context.Self.entityId) as EntityAlive;
            if (leader == null) return;
            SCoreUtils.SetCrouching(context, leader.IsCrouching);
        }
    }
}