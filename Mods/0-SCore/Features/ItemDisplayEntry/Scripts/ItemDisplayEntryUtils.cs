public class ItemDisplayEntryUtils
{
    public static float ParseCustomValue(DisplayInfoEntry display, ItemValue itemValue)
    {
        var customName = display.CustomName;
        if (!itemValue.HasMetadata(customName)) return -1;
        var metadata = itemValue.GetMetadata(customName);

        switch (display.DisplayType)
        {
            case DisplayInfoEntry.DisplayTypes.Integer:
                if (metadata is int iValue)
                {
                    return (iValue);
                }
                break;
            case DisplayInfoEntry.DisplayTypes.Decimal1:
                if (metadata is float fValue)
                {
                    return fValue;
                }
                break;
        }

        return -1;
    }
}
