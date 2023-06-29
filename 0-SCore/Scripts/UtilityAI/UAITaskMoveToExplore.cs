using UnityEngine;

namespace UAI
{
    public class UAITaskMoveToExplore : UAITaskMoveToTarget
    {
        private static readonly string AdvFeatureClass = "AdvancedTroubleshootingFeatures";
        private static readonly string Feature = "UtilityAILoggingMin";

        private string _buff;
        private bool _hadBuff;
        private Vector3 _vector;

        protected override void initializeParameters()
        {
            base.initializeParameters();
            if (Parameters.ContainsKey("buff")) _buff = Parameters["buff"];
        }

        public override void Update(Context _context)
        {
            // true if you looted it.
            if (CheckContainer(_context))
            {
                Stop(_context);
                return;
            }

            base.Update(_context);
            SCoreUtils.CheckForClosedDoor(_context);

            if (SCoreUtils.IsBlocked(_context))
            {
                EntityUtilities.Stop(_context.Self.entityId);
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Am Blocked.: {_context.Self.EntityName} ( {_context.Self.entityId} ");
                Stop(_context);
                _context.Self.getNavigator().clearPath();
            }
        }

        public override void Reset(Context _context)
        {
            _hadBuff = false;
            _vector = Vector3.zero;
            base.Reset(_context);
        }

        public override void Stop(Context _context)
        {
            if (SCoreUtils.HasBuff(_context, _buff))
            {
                EntityUtilities.Stop(_context.Self.entityId);
                // Interrupt the buff we are being attacked.
                if (EntityUtilities.GetAttackOrRevengeTarget(_context.Self.entityId) == null)
                    return;
            }

            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Stop: {_context.Self.EntityName} ( {_context.Self.entityId} ");
            _vector = Vector3.zero;
            base.Stop(_context);
        }

        public override void Start(Context _context)
        {
            var path = SphereCache.GetPaths(_context.Self.entityId);
            SCoreUtils.SetCrouching(_context);
            if (path?.Count > 0)
            {
                _vector = path[0];
                AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"{GetType()} Start Workstation: {_context.Self.EntityName} ( {_context.Self.entityId} Position: {_vector} ");
                SCoreUtils.FindPath(_context, _vector + Vector3.forward, false);
                //_context.Self.FindPath(_vector + Vector3.forward, _context.Self.GetMoveSpeed(), true, null);
                _context.ActionData.Started = true;
                _context.ActionData.Executing = true;
                return;
            }

            // If there's no targets, don't bother.
            this.Stop(_context);
        }

        private void GetItemFromContainer(Context _context, TileEntityLootContainer tileLootContainer)
        {
            var blockPos = tileLootContainer.ToWorldPos();
            if (string.IsNullOrEmpty(tileLootContainer.lootListName))
                return;
            if (tileLootContainer.bTouched)
                return;

            tileLootContainer.bTouched = true;
            tileLootContainer.bWasTouched = true;

            // Nothing to loot.
            if (tileLootContainer.items == null) return;

            //   SCoreUtils.SetLookPosition(_context,blockPos);

            _context.Self.MinEventContext.TileEntity = tileLootContainer;
            _context.Self.FireEvent(MinEventTypes.onSelfOpenLootContainer);

            var lootContainer = LootContainer.GetLootContainer(tileLootContainer.lootListName);
            if (lootContainer == null)
                return;
            //            var gameStage = EffectManager.GetValue(PassiveEffects.LootGamestage, null, 10, _context.Self);
            //var array = lootContainer.Spawn(_context.Self.rand, tileLootContainer.items.Length, gameStage, 0f, null, new FastTags());
            var array = lootContainer.Spawn(_context.Self.rand, tileLootContainer.items.Length, (float)_context.Self.Progression.GetLevel(), 0f, null, new FastTags(), lootContainer.UniqueItems, false);



            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"GetItemFromContainers(): {_context.Self.EntityName} ( {_context.Self.entityId}");
            for (var i = 0; i < array.Count; i++)
                _context.Self.lootContainer.AddItem(array[i].Clone());

            _context.Self.FireEvent(MinEventTypes.onSelfLootContainer);
        }

        private bool CheckContainer(Context _context)
        {
            if (!_context.Self.onGround)
                return false;

            //    SCoreUtils.SetLookPosition(_context, _vector);

            var lookRay = new Ray(_context.Self.position, _context.Self.GetLookVector());
            if (!Voxel.Raycast(_context.Self.world, lookRay, 3f, false, false))
                return false; // Not seeing the target.

            if (!Voxel.voxelRayHitInfo.bHitValid)
                return false; // Missed the target. Overlooking?

            // Still too far away.
            var sqrMagnitude2 = (_vector - _context.Self.position).sqrMagnitude;
            if (sqrMagnitude2 > 2f)
                return false;

            var tileEntity = _context.Self.world.GetTileEntity(Voxel.voxelRayHitInfo.hit.clrIdx, new Vector3i(_vector));
            switch (tileEntity)
            {
                // if the TileEntity is a loot container, then loot it.
                case TileEntityLootContainer tileEntityLootContainer:
                    GetItemFromContainer(_context, tileEntityLootContainer);
                    break;
            }

            // Stop the move helper, so the entity does not slide around.
            EntityUtilities.Stop(_context.Self.entityId);

            // Trigger a buff if its defined.
            if (string.IsNullOrEmpty(_buff) || _hadBuff) return true;

            AdvLogging.DisplayLog(AdvFeatureClass, Feature, $"ExplorationPoint(): {_context.Self.EntityName} ( {_context.Self.entityId} I am close enough to {tileEntity.ToString()}");

            _hadBuff = true;
            _context.Self.Buffs.AddBuff(_buff);
            SphereCache.RemovePath(_context.Self.entityId, _vector);
            return true;
        }
    }
}