    public class NetPackageWeaponSwap : NetPackage
    {
        private int _entityId;
        private string _itemClassName;
        public NetPackageWeaponSwap Setup(int targetId, string itemClass)
        {
            _entityId = targetId;
            _itemClassName = itemClass;
            return this;
        }

        public override void read(PooledBinaryReader _reader)
        {
            _entityId = _reader.ReadInt32();
            _itemClassName = _reader.ReadString();
        }

        public override void write(PooledBinaryWriter _writer)
        {
            base.write(_writer);
            _writer.Write(_entityId);
            _writer.Write(_itemClassName);
        }

        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (_world == null || !_world.IsRemote())
            {
                return;
            }
            var entityAlive = _world.GetEntity(_entityId) as EntityAliveSDX;
            if (entityAlive == null)
            {
                return;
            }

            var item = ItemClass.GetItem(_itemClassName);
            entityAlive.UpdateWeapon(item);
        }

        public override int GetLength()
        {
            return 8;
        }
    }
