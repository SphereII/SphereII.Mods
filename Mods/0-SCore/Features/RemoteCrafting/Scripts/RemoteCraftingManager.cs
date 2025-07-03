using System.Collections.Generic;
using SCore.Features.RemoteCrafting.Scripts;

public class RemoteCraftingManager
{
    private static List<ItemStack> _items;
    private static ulong _lastScan = 0;
    private static ulong _delay = 10;
    public List<ItemStack> RefreshAvailableItems(EntityAlive player)
    {
        var currentTime = GameManager.Instance.World.GetWorldTime();
        if (_lastScan + _delay > currentTime || _items.Count == 0 )
        {
            _lastScan = currentTime;
            _items = RemoteCraftingUtils.SearchNearbyContainers(player);
        }

        return _items;
    }

    public void RefreshAvailableContainers()
    {
        
    }
    
}
