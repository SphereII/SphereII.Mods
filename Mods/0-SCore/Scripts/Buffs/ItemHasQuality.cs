// 	<requirement name="ItemHasQuality, SCore" operation="Equals" value="4"/>
public class ItemHasQuality: TargetedCompareRequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (!base.IsValid(_params))
        {
            return false;
        }

        if (!_params.ItemValue.HasQuality) return false;
        
        if (!invert)
        {
                        
            return !compareValues(_params.ItemValue.Quality, operation, value);
        }
        return compareValues(_params.ItemValue.Quality, operation, value);
    }

    
}
