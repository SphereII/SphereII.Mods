
    using UnityEngine;

    public class MissionUtils {
        private EntityAlive _entityAlive;
        private Vector3 scale;

        public MissionUtils(EntityAlive entityAlive) {
            _entityAlive = entityAlive;
        }
        public bool IsOnMission() {
            return _entityAlive.Buffs.HasCustomVar("onMission") && _entityAlive.Buffs.GetCustomVar("onMission") == 1f;
        }

        public void SendOnMission(bool send) {
            if (send)
            {

                var enemy = _entityAlive.GetRevengeTarget();
                if (enemy != null)
                {
                    _entityAlive.SetAttackTarget(null, 0);
                    enemy.SetAttackTarget(null, 0);
                    enemy.SetRevengeTarget(null);
                    enemy.DoRagdoll(new DamageResponse());
                    _entityAlive.SetRevengeTarget(null);
                }

                _entityAlive.SetIgnoredByAI(true);
                scale = _entityAlive.transform.localScale;
                _entityAlive.transform.localScale = new Vector3(0, 0, 0);
                _entityAlive.emodel.SetVisible(false, false);
                _entityAlive.Buffs.AddCustomVar("onMission", 1f);

                if (_entityAlive.NavObject != null)
                    _entityAlive.NavObject.IsActive = false;
                _entityAlive.DebugNameInfo = "";

                _entityAlive.SetupDebugNameHUD(false);
            }
            else
            {
                _entityAlive.transform.localScale = scale;

                _entityAlive.emodel.SetVisible(true, true);
                _entityAlive.enabled = true;
                _entityAlive.Buffs.RemoveCustomVar("onMission");
                if (_entityAlive.NavObject != null)
                    _entityAlive.NavObject.IsActive = true;
                _entityAlive.SetIgnoredByAI(false);
            }
        }

    }
