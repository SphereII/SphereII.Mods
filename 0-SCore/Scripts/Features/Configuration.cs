using System;

public static class Configuration
{
    public static bool CheckFeatureStatus(string strFeature)
    {
        var ConfigurationFeatureBlock = Block.GetBlockValue("ConfigFeatureBlock");
        if (ConfigurationFeatureBlock.type == 0)
            return false;

        var result = false;
        if (ConfigurationFeatureBlock.Block.Properties.Contains(strFeature))
            result = ConfigurationFeatureBlock.Block.Properties.GetBool(strFeature);

        return result;
    }


    public static bool CheckFeatureStatus(string strClass, string strFeature)
    {
        var ConfigurationFeatureBlock = Block.GetBlockValue("ConfigFeatureBlock");
        if (ConfigurationFeatureBlock.type == 0)
            //   UnityEngine.Debug.Log("Feature Block not found: " + strClass + " " + strFeature);
            return false;

        var result = false;
        if (ConfigurationFeatureBlock.Block.Properties.Classes.ContainsKey(strClass))
        {
            var dynamicProperties3 = ConfigurationFeatureBlock.Block.Properties.Classes[strClass];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
                if (string.Equals(keyValuePair.Key, strFeature, StringComparison.CurrentCultureIgnoreCase))
                    result = StringParsers.ParseBool(dynamicProperties3.Values[keyValuePair.Key]);
            //   UnityEngine.Debug.Log("Found: " + strClass + " " + strFeature + " : result: " + result);
        }

        return result;
    }

    public static string GetPropertyValue(string strClass, string strFeature)
    {
        var ConfigurationFeatureBlock = Block.GetBlockValue("ConfigFeatureBlock");
        if (ConfigurationFeatureBlock.type == 0)
            return string.Empty;


        var result = string.Empty;
        if (ConfigurationFeatureBlock.Block.Properties.Classes.ContainsKey(strClass))
        {
            var dynamicProperties3 = ConfigurationFeatureBlock.Block.Properties.Classes[strClass];
            foreach (var keyValuePair in dynamicProperties3.Values.Dict.Dict)
                if (keyValuePair.Key == strFeature)
                    return keyValuePair.Value.ToString();
        }

        return result;
    }
}