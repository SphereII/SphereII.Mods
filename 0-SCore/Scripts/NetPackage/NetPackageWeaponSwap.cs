    // using UnityEngine;
    //
    // public class NetPackageWeaponSwap : NetPackage
    // {
    //     private int _entityId;
    //     private ItemValue _itemId;
    //     public NetPackageWeaponSwap Setup(int targetId, ItemValue itemClass)
    //     {
    //         _entityId = targetId;
    //         _itemId = itemClass;
    //         return this;
    //     }
    //
    //     public override void read(PooledBinaryReader reader)
    //     {
    //         _entityId = reader.ReadInt32();
    //         //_itemId = reader.readItem;
    //     }
    //
    //     public override void write(PooledBinaryWriter writer)
    //     {
    //         base.write(writer);
    //         writer.Write(_entityId);
    //         writer.Write(_itemId);
    //     }
    //
    //     public override void ProcessPackage(World _world, GameManager _callbacks)
    //     {
    //         var entityAlive = _world.GetEntity(_entityId) as EntityAliveSDX;
    //         if (entityAlive == null)
    //         {
    //             Debug.Log("Entity alive is null.");
    //             return;
    //         }
    //
    //         var itemClass = ItemClass.GetForId(_itemId);
    //         Debug.Log($"Item: {itemClass.GetItemName()} ID: {_itemId}");
    //         var item = ItemClass.GetItem(itemClass.GetItemName());
    //         entityAlive.UpdateWeapon(item);
    //     }
    //
    //     public override int GetLength()
    //     {
    //         return 8;
    //     }
    // }
