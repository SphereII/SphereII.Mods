using UnityEngine;

namespace SCore.Scripts.NetPackage
{
    public class NetPackageDeployNPCSDX :NetPackageVehicleSpawn
    {
        public override void ProcessPackage(World _world, GameManager _callbacks)
        {
            if (_world == null)
            {
                return;
            }
            
            var entity = EntityFactory.CreateEntity(this.entityType, this.pos, this.rot);
            var entityAlive = entity as EntityAliveSDX;
            if (entityAlive == null)
            {
                Debug.Log("Not an EntityAlive!");
                return;
            }
            entityAlive.SetSpawnerSource(EnumSpawnerSource.StaticSpawner);
            entityAlive.SetItemValue(itemValue);
            GameManager.Instance.World.SpawnEntityInWorld(entityAlive);

        }
    }
}