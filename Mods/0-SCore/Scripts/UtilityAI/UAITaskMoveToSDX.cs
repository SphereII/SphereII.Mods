using GamePath;
using UnityEngine;

namespace UAI
{
    public class UAITaskMoveToSDX : UAITaskMoveToTarget
    {
        private EntityAlive _theEntity;
        private int _relocateTicks;
        private EntityAlive _entityTarget;
        private Vector3 _entityTargetPos;
        private Vector3 _entityTargetVel;
        private int _attackTimeout;
        private int _pathCounter;
        private GameRandom _random;
        private Vector2 _seekPosOffset;

        public override void Start(Context context)
        {
            // Reset crouch status to default
            SCoreUtils.SetCrouching(context);
            _theEntity = context.Self;
            _random = _theEntity.rand;
            var entityAlive = UAIUtils.ConvertToEntityAlive(context.ActionData.Target);
            if (entityAlive != null)
            {
                
                _entityTarget = entityAlive;
                _entityTargetPos = entityAlive.position;
                if (entityAlive.IsWalkTypeACrawl())
                    _entityTargetPos = entityAlive.getHeadPosition();
                context.Self.SetInvestigatePosition(_entityTargetPos, 1200);
            }

            if (context.ActionData.Target is Vector3 vector)
            {
                _entityTarget = null;
            }

            context.ActionData.Started = true;
            context.ActionData.Executing = true;
        }

        public override void Stop(Context context)
        {
            // Did we lose our target? do we need to keep looking for it?
            if (_entityTarget != null && _entityTarget.IsAlive())
            {
                var num = context.Self.CalcInvestigateTicks(Constants.cEnemySenseMemory * 20, _entityTarget);
                context.Self.SetInvestigatePosition(_entityTarget.position, num);
            }

            base.Stop(context);
        }

        public override void Update(Context context)
        {
            // We don't call base.Update() here, because it's checking if the entity has no path, then it stops the task.
            // However, at times, we do want the opportunity to repath without stopping the task. This stops the pauses the entity does.
            if (SCoreUtils.IsBlocked(context))
            {
                Stop(context);
                return;
            }

            CheckBlockLocation(context);
            CheckEntityLocation(context);
        }

        private void CheckEntityLocation(Context context)
        {
            var entityAlive = UAIUtils.ConvertToEntityAlive(context.ActionData.Target);
            if (entityAlive == null) return;
            if (entityAlive.IsDead())
            {
                Stop(context);
                return;
            }
            //
            // // Position is where we were going. Check to see if the entity has changed position since our last path finding.
            // if (Vector3.Distance(entityAlive.position, _entityTargetPos) < 2f)
            // {
            //     // Things are fine, we are still pathing.
            //     context.Self.SetLookPosition(_entityTargetPos);
            //     context.Self.RotateTo(_entityTargetPos.x, _entityTargetPos.y, _entityTargetPos.z, 30f, 30f);
            //     context.Self.moveHelper.SetMoveTo(_entityTargetPos, true);
            //     return;
            // }

            if (_relocateTicks > 0)
            {
                if (!_theEntity.navigator.noPathAndNotPlanningOne())
                {
                    _relocateTicks--;
                    _theEntity.moveHelper.SetFocusPos(this._entityTarget.position);
                    return;
                }

                _relocateTicks = 0;
            }

            var vector2 = _entityTarget.position;
            var a = vector2 - _entityTargetPos;
            if (a.sqrMagnitude < 1f)
            {
                _entityTargetVel = _entityTargetVel * 0.7f + a * 0.3f;
            }

            _entityTargetPos = vector2;
            _attackTimeout--;

            _theEntity.moveHelper.CalcIfUnreachablePos();
            float num2;
            float num3;
            var holdingItemItemValue = this._theEntity.inventory.holdingItemItemValue;
            var holdingItemIdx = this._theEntity.inventory.holdingItemIdx;
            var itemAction = holdingItemItemValue.ItemClass.Actions[holdingItemIdx];
            num2 = 1.095f;
            if (itemAction != null)
            {
                num2 = itemAction.Range;
                if (num2 == 0f)
                {
                    num2 = EffectManager.GetItemValue(PassiveEffects.MaxRange, holdingItemItemValue, 0f);
                }
            }

            num3 = Utils.FastMax(0.7f, num2 - 0.35f);
            var num4 = num3 * num3;
            var estimatedTicks = 1f + _random.RandomFloat * 10f;
            var targetXZDistanceSq = this.GetTargetXZDistanceSq(estimatedTicks);
            var num5 = vector2.y - this._theEntity.position.y;
            var num6 = Utils.FastAbs(num5);
            var flag = targetXZDistanceSq <= num4 && num6 < 1f;
            if (!flag)
            {
                if (num6 < 3f && !PathFinderThread.Instance.IsCalculatingPath(this._theEntity.entityId))
                {
                    var path = this._theEntity.navigator.getPath();
                    if (path != null && path.NodeCountRemaining() <= 2)
                    {
                        this._pathCounter = 0;
                    }
                }

                var num = _pathCounter - 1;
                _pathCounter = num;
                if (num <= 0 && _theEntity.CanNavigatePath() && !PathFinderThread.Instance.IsCalculatingPath(_theEntity.entityId))
                {
                    _pathCounter = 6 + _random.RandomRange(0,10);
                    var moveToLocation = this.GetMoveToLocation(num3);
                    if (moveToLocation.y - this._theEntity.position.y < -8f)
                    {
                        _pathCounter += 40;
                        if (_random.RandomFloat < 0.2f)
                        {
                            _seekPosOffset.x += (_random.RandomFloat * 0.6f - 0.3f);
                            _seekPosOffset.y += (_random.RandomFloat * 0.6f - 0.3f);
                        }

                        moveToLocation.x += _seekPosOffset.x;
                        moveToLocation.z += _seekPosOffset.y;
                    }
                    else
                    {
                        var num7 = (moveToLocation - _theEntity.position).magnitude - 5f;
                        if (num7 > 0f)
                        {
                            if (num7 > 60f)
                            {
                                num7 = 60f;
                            }

                            _pathCounter += (int) (num7 / 20f * 20f);
                        }
                    }

                    _theEntity.FindPath(moveToLocation, _theEntity.GetMoveSpeedAggro(), true, null);
                }
            }

            if (_theEntity.Climbing)
            {
                return;
            }

            var flag2 = _theEntity.CanSee(_entityTarget);
            _theEntity.SetLookPosition((flag2 && !_theEntity.IsBreakingBlocks)
                ? _entityTarget.getHeadPosition()
                : Vector3.zero);
            if (!flag)
            {
                if (_theEntity.navigator.noPathAndNotPlanningOne() && _pathCounter > 0 && num5 < 2.1f)
                {
                    var moveToLocation2 = GetMoveToLocation(num3);
                    _theEntity.moveHelper.SetMoveTo(moveToLocation2, true);
                }
            }
            else
            {
                _theEntity.moveHelper.Stop();
                _pathCounter = 0;
            }

            var num8 = num2 - 0.1f;
            var num9 = num8 * num8;
            if (targetXZDistanceSq > num9 || num6 >= 1.25f)
            {
                return;
            }

            // if (targetXZDistanceSq < 1.2)
            // {
            //     var moveback = _theEntity.position + Vector3.back;
            //     _theEntity.moveHelper.SetMoveTo(moveback, true);
            //     return;
            // }

            _theEntity.IsBreakingBlocks = false;
            _theEntity.IsBreakingDoors = false;
            if (_theEntity.bodyDamage.HasLimbs && !_theEntity.Electrocuted)
            {
                _theEntity.RotateTo(vector2.x, vector2.y, vector2.z, 30f, 30f);
            }

            if (_theEntity.GetDamagedTarget() == _entityTarget || 
                (_entityTarget != null &&
                 _entityTarget.GetDamagedTarget() == _theEntity))
            {
                _theEntity.ClearDamagedTarget();
                if (_entityTarget)
                {
                    _entityTarget.ClearDamagedTarget();
                }
            }

            if (_attackTimeout > 0)
            {
                return;
            }


            _theEntity.SleeperSupressLivingSounds = false;
            if (!_theEntity.Attack(false)) return;
            _attackTimeout = _theEntity.GetAttackTimeoutTicks();
            _theEntity.Attack(true);
        }

        private Vector3 GetMoveToLocation(float maxDist)
        {
            var vector = _entityTarget.position;
            vector += _entityTargetVel * 6f;
            vector = _entityTarget.world.FindSupportingBlockPos(vector);
            if (maxDist < 0f) return vector;


            var vector2 = new Vector3(_theEntity.position.x, vector.y, _theEntity.position.z);
            var vector3 = vector - vector2;
            var magnitude = vector3.magnitude;
            if (magnitude > 3f) return vector;


            if (magnitude <= maxDist)
            {
                var num = vector.y - _theEntity.position.y;
                if (num < -3f || num > 1.5f)
                {
                    return vector;
                }

                return vector2;
            }

            vector3 *= maxDist / magnitude;
            var vector4 = vector - vector3;
            vector4.y += 0.51f;
            var pos = World.worldToBlockPos(vector4);
            var block = _entityTarget.world.GetBlock(pos);
            var block2 = block.Block;
            if (block2.PathType > 0) return vector;

            if (Physics.Raycast(vector4 - Origin.position, Vector3.down, out var raycastHit, 1.02f, 1082195968))
            {
                vector4.y = raycastHit.point.y + Origin.position.y;
                return vector4;
            }

            if (!block2.IsElevator((int) block.rotation)) return vector;
            vector4.y = vector.y;
            return vector4;
        }

        private float GetTargetXZDistanceSq(float estimatedTicks)
        {
            var vector = _entityTarget.position;
            vector += _entityTargetVel * estimatedTicks;
            var vector2 = _theEntity.position + _theEntity.motion * estimatedTicks - vector;
            vector2.y = 0f;
            return vector2.sqrMagnitude;
        }

        private void CheckBlockLocation(Context context)
        {
            if (context.ActionData.Target is not Vector3 vector3) return;

            context.Self.SetLookPosition(vector3);
            if (context.Self.getNavigator().noPathAndNotPlanningOne())
            {
                Stop(context);
                return;
            }

            if (Vector3.Distance(context.Self.position, vector3) < 2f)
            {
                Stop(context);
            }
        }
    }
}