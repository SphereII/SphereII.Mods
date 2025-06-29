using System.Collections.Generic;
public class RecipePropertiesManager
{
    private static readonly Dictionary<int, string> RecipeProperties = new Dictionary<int, string>();

    public static void SetRecipeProperties(int traderID, string currency)
    {
        RecipeProperties.Add(traderID, currency);
    }

    public static string GetRecipeProperties(int traderID)
    {
        var result= RecipeProperties.GetValueOrDefault(traderID, "casinoCoin");
        return result;
    }
}