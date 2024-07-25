using System.Collections;
using UnityEngine;

public class LeaderUtils {
    private EntityAlive _entityAlive;
    private bool _hasSetOwner;
    private EntityPlayer _owner;
    private bool _hasNavObjectsEnabled;
    private bool _isTeleporting;

    public LeaderUtils(EntityAlive entityAlive) {
        _entityAlive = entityAlive;
    }


    public void LeaderUpdate() {
        if (_entityAlive.IsDead())
        {
            return;
        }

        var flLeader = _entityAlive.Buffs.GetCustomVar("Leader");
        _entityAlive.belongsPlayerId = (int)flLeader;

        if (_entityAlive.belongsPlayerId > 0 && (!_hasSetOwner || _owner == null))
        {
            var player = _entityAlive.world.GetEntity(_entityAlive.belongsPlayerId) as EntityPlayer;
            if (player)
            {
                _owner = player;
            }
        }

        if (!_hasSetOwner && _entityAlive.belongsPlayerId > 0 && _owner != null)
        {
            _hasSetOwner = true;
            _entityAlive.IsEntityUpdatedInUnloadedChunk = true;
            _entityAlive.bWillRespawn = true;
            _entityAlive.bIsChunkObserver = true;

            if (!_hasNavObjectsEnabled)
            {
                if (_entityAlive is EntityAliveV2 entityAliveV2)
                    entityAliveV2.HandleNavObject();
                _hasNavObjectsEnabled = true;
            }

            if (_owner && _entityAlive.IsAlive())
            {
                EntityPlayer player = _owner as EntityPlayer;

                if (player.Companions.IndexOf(_entityAlive) < 0)
                {
                    player.Companions.Add(_entityAlive);
                    int num2 = player.Companions.IndexOf(_entityAlive);
                    var v = Constants.TrackedFriendColors[num2 % Constants.TrackedFriendColors.Length];
                    if (_entityAlive.NavObject != null)
                    {
                        _entityAlive.NavObject.UseOverrideColor = true;
                        _entityAlive.NavObject.OverrideColor = v;
                        _entityAlive.NavObject.name = _entityAlive.EntityName;
                    }
                }
            }
        }

        if (_entityAlive.belongsPlayerId > 0 && _owner != null)
        {
            var distanceToLeader = _entityAlive.GetDistance(_owner);
            if (distanceToLeader > 60)
            {
                TeleportToPlayer(_owner);
            }

            EntityPlayer player = _owner as EntityPlayer;
            switch (_entityAlive.Buffs.GetCustomVar("CurrentOrder"))
            {
                case 1f:
                    // if our leader is attached, that means they are attached to a vehicle
                    if (_owner.AttachedToEntity != null)
                    {
                        if (_entityAlive is EntityAliveV2 entityAliveV2)
                            entityAliveV2.SendOnMission(true);
                        var _position = _owner.GetPosition();
                        _position.y += 10;
                        _entityAlive.SetPosition(_position);
                    }
                    else
                    {
                        if (_entityAlive.Buffs.HasCustomVar("onMission"))
                        {
                            _entityAlive.SetPosition(_owner.GetPosition());
                            if (_entityAlive is EntityAliveV2 entityAliveV2)
                                entityAliveV2.SendOnMission(false);
                        }

                        if (player.Companions.IndexOf(_entityAlive) < 0)
                        {
                            player.Companions.Add(_entityAlive);
                            int num2 = player.Companions.IndexOf(_entityAlive);
                            var v = Constants.TrackedFriendColors[num2 % Constants.TrackedFriendColors.Length];
                            if (_entityAlive.NavObject != null)
                            {
                                _entityAlive.NavObject.UseOverrideColor = true;
                                _entityAlive.NavObject.OverrideColor = v;
                                _entityAlive.NavObject.name = _entityAlive.EntityName;
                            }
                        }
                    }

                    break;
                case 2f:
                    if (player)
                    {
                        player.Companions.Remove(_entityAlive);
                    }

                    break;
                default:
                    if (player)
                    {
                        player.Companions.Remove(_entityAlive);
                    }

                    break;
            }
        }
    }

    public void TeleportToPlayer(EntityAlive target, bool randomPosition = false) {
        if (target == null)
        {
            return;
        }

        float currentOrder = _entityAlive.Buffs.GetCustomVar("CurrentOrder");

        if (currentOrder > 1)
        {
            return;
        }

        if (target.IsInElevator())
        {
            return;
        }

        var position = _entityAlive.position;
        if (!(_entityAlive.HasAnyTags(FastTags.Parse("survivor")) || _entityAlive.HasAnyTags(FastTags.Parse("ally"))))
        {
            return;
        }

        var target2i = new Vector2(target.position.x, target.position.z);
        var mine2i = new Vector2(position.x, position.z);
        var distance = Vector2.Distance(target2i, mine2i);

        if (_isTeleporting)
        {
            return;
        }

        var myPosition = target.position + Vector3.back;
        var player = target as EntityPlayer;
        if (player != null)
        {
            myPosition = player.GetBreadcrumbPos(3 * _entityAlive.rand.RandomFloat);

            // If my target distance is still way off from the player, teleport randomly. That means the bread crumb isn't accurate
            var distance2 = Vector3.Distance(myPosition, player.position);
            if (distance2 > 40f)
            {
                randomPosition = true;
            }

            if (randomPosition)
            {
                Vector3 dirV = target.position - position;
                myPosition = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dirV, 5, 80f);
            }

            float positionY = player.position.y + 1f;
            if (player.position.y > position.y)
            {
                myPosition.y = positionY;
            }
            else
            {
                if (GameManager.Instance.World.GetHeightAt(myPosition.x, myPosition.z) <= player.position.y)
                {
                }
                else
                {
                    positionY = GameManager.Instance.World.GetHeightAt(myPosition.x, myPosition.z) + 1f;
                }
            }

            myPosition.y = positionY;
        }

        _entityAlive.motion = Vector3.zero;
        _entityAlive.navigator?.clearPath();

        _entityAlive.SetRevengeTarget(null);
        _entityAlive.SetAttackTarget(null, 0);

        _entityAlive.SetPosition(myPosition);
        _entityAlive.StartCoroutine(ValidateTeleport(target, randomPosition));
    }

    public void SetDead() {
        var owner = EntityUtilities.GetLeaderOrOwner(_entityAlive.entityId) as EntityPlayer;
        if (owner == null) return;

        if (owner.Companions.IndexOf(_entityAlive) >= 0)
        {
            owner.Companions.Remove(_entityAlive);
        }
    }

    private IEnumerator ValidateTeleport(EntityAlive target, bool randomPosition = false) {
        yield return new WaitForSeconds(1f);
        var position = _entityAlive.position;
        var y = (int)GameManager.Instance.World.GetHeightAt(position.x, position.z);
        if (position.y < y)
        {
            var myPosition = position;

            var player = target as EntityPlayer;
            if (player != null)
                myPosition = player.GetBreadcrumbPos(3 * _entityAlive.rand.RandomFloat);

            if (randomPosition)
            {
                Vector3 dirV = target.position - position;
                myPosition = RandomPositionGenerator.CalcPositionInDirection(target, target.position, dirV, 5, 80f);
            }

            //// Find the ground.
            myPosition.y = (int)GameManager.Instance.World.GetHeightAt(myPosition.x, myPosition.z) + 2;

            // Find the ground.
            _entityAlive.motion = Vector3.zero;
            _entityAlive.navigator?.clearPath();
            _entityAlive.SetPosition(myPosition);
        }

        _isTeleporting = false;
        yield return null;
    }
}