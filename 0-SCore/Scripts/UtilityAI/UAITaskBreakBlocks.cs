using System.Collections;
using UnityEngine;

// using this namespace is necessary for Utilities AI Tasks
//       <task class="AttackTargetEntitySDX, SCore" action_index="0" /> 
// The game adds UAI.UAITask to the class name for discover.
namespace UAI
{
    public class UAITaskBreakBlock : UAITaskAttackTargetEntity
    {
        Vector3 fleePosition = Vector3.zero;
        protected override void initializeParameters()
        {
            base.initializeParameters();
        }

        public override void Start(Context _context)
        {
            if (_context.ActionData.Target is Vector3 vector)
            {
                _context.Self.SetLookPosition(vector);
                var targetType = GameManager.Instance.World.GetBlock(new Vector3i(vector));

                if (targetType.Equals(BlockValue.Air))
                {
                    this.Stop(_context);
                    return;
                }

       

                _context.ActionData.Started = true;
                _context.ActionData.Executing = true;
                return;

            }
            Stop(_context);

        }

        public override void Stop(Context _context)
        {
            if (_context.Self.IsBreakingDoors) return;
            SCoreUtils.SetWeapon(_context);
            base.Stop(_context);
        }

        
        public override void Update(Context _context)
        {
            if (_context.ActionData.Target is Vector3 vector)
            {
                if (_context.Self.IsBreakingDoors)
                {
                    if (fleePosition == Vector3.zero)
                    {
                        RandomPositionGenerator.CalcAway(_context.Self, 5, 10, 2, vector);
                        SCoreUtils.FindPath(_context, fleePosition, true);
                    }
                    _context.Self.moveHelper.SetMoveTo(fleePosition, true);
                    return;
                }


                _context.Self.SetLookPosition(vector);
                var targetType = GameManager.Instance.World.GetBlock(new Vector3i(vector));
                if (targetType.Equals(BlockValue.Air))
                {
                    _context.Self.IsBreakingDoors = false;
                    this.Stop(_context);
                    return;
                }
                if (!SCoreUtils.CheckForClosedDoor(_context))
                {
                        // If we arn't breaking blocks yet, execute the bomb placement
                        if (SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer )
                        {
                            _context.Self.IsBreakingDoors = true;
                        _context.Self.lookAtPosition = vector;
                        _context.Self.RotateTo(vector.x, vector.y, vector.z, 90f, 90f);
                        GameManager.Instance.StartCoroutine(SimulateActionsLibrary.SimulateActionThrownTimedCharge(_context, vector));
                        }
                    return;

                }
            }
        }

      

    }
}