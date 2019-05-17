
using System;
using UnityEngine;

public static class ModGeneralUtilities
{
    public static Vector3 StringToVector3(string sVector)
    {
        if(String.IsNullOrEmpty(sVector))
            return Vector3.zero;

        // Remove the parentheses
        if(sVector.StartsWith("(") && sVector.EndsWith(")"))
            sVector = sVector.Substring(1, sVector.Length - 2);

        // split the items
        string[] sArray = sVector.Split(',');

        // store as a Vector3
        Vector3 result = new Vector3(
            float.Parse(sArray[0]),
            float.Parse(sArray[1]),
            float.Parse(sArray[2]));

        return result;
    }

}

