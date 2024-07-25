using System.Reflection;
using HarmonyLib;
using UnityEngine;

public static class ILUtilities {

    // Debug to print off a list of all fields and local variables.
    /* The index may change for each release.
        Example Output of this method will show, in order, all the local variable types, and their index.
        The IL itself does not contain the name of the variables, as that type of meta data isn't actually available.
        
        Instead, you need to use the index of the local variable. This is done by reviewing the output of the below method:
        the Index of the variable is within the ( ).  You will have to review the entire list of methods, then compare back to the C# decompiled
        class to find the right one. In the example above, I want to find a string called "text". I know that there's a few other local variables
        of various types right above the one I want, so I use those as an anchor to find correct index.
        Example output:
         <snip>
            EntityItem :: EntityItem (28)
            BlockValue :: BlockValue (29)
            ProjectileMoveScript :: ProjectileMoveScript (30)
            ThrownWeaponMoveScript :: ThrownWeaponMoveScript (31)
            System.String :: System.String (32) // This is the one we want
            <snip>

         C# Equivalent
            EntityItem entityItem = null;
            BlockValue blockValue = BlockValue.Air;
            ProjectileMoveScript projectileMoveScript = null;
            ThrownWeaponMoveScript thrownWeaponMoveScript = null;
            string text = null;  // Because it matches here.
    */
    public static void DisplayLocalVariables(MethodBase method) {
        var body = method.GetMethodBody();
        if (body == null)
        {
            Debug.Log($"No Body in Method: {method.Name}");
            return;
        }
        Debug.Log($"Start Method: {method.Name}");
        Debug.Log("========================");
        foreach (var field in body.LocalVariables)
        {
            Debug.Log($"{field} :: {field.LocalIndex}");
        }
        Debug.Log("========================");
        Debug.Log($"End Method: {method.Name}");
    }
    public static string FindLocalVariable( MethodBase method, int index) {
        var body = method.GetMethodBody();
        foreach (var field in body.LocalVariables)
        {
            if (field.LocalIndex == index)
            {
                return field.ToString();
            }
                
        }

        return string.Empty;
    }
}