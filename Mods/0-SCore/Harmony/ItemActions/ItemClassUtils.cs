public class ItemClassUtils
{
    public static ItemValue GetItemValue(XUiController itemController)
    {
        ItemValue itemValue;
        if (itemController is not XUiC_ItemStack stack)
        {
            if (itemController is not XUiC_EquipmentStack stack2) return null;
            if (stack2.itemStack.IsEmpty()) return null;
            itemValue = stack2.ItemValue;
        }
        else
        {
            if (stack.itemStack.IsEmpty()) return null;
            itemValue = stack.itemStack.itemValue;
        }

        return itemValue;
    }
}
