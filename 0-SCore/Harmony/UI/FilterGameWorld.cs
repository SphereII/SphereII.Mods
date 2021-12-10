//using HarmonyLib;
//using System.Reflection;
//using UnityEngine;
//using DMT;
//using System;
//using System.Collections.Generic;

////[HarmonyPatch(typeof(PathAbstractions), MethodType.Constructor)]
//[HarmonyPatch(typeof(PathAbstractions))]
//[HarmonyPatch("GetAvailablePathsList")]
//public class SCoreFilterGameWorld
//{

//    public static List<PathAbstractions.AbstractedLocation> Postfix(List<PathAbstractions.AbstractedLocation> __result, PathAbstractions.SearchPaths _searchPaths)
//    {
//        List<PathAbstractions.AbstractedLocation> newList = new List<PathAbstractions.AbstractedLocation>();
//        foreach (var each in __result)
//        {
//            if (each.Name.Contains("Winter Project"))
//                newList.Add(each);
//        }

//        return newList;
//    }
//}

//[HarmonyPatch(typeof(XUiC_NewContinueGame))]
//[HarmonyPatch("updateWorlds")]
//public class SCoreUpdateWorlds
//{

//    public static void Postfix(ref XUiC_ComboBoxList<global::XUiC_NewContinueGame.LevelInfo> ___cbxWorldName)
//    {
//        XUiC_NewContinueGame.LevelInfo random = new global::XUiC_NewContinueGame.LevelInfo
//        {
//            RealName = "New Random World",
//            CustName = "New Random World",
//            Description = "Generate New Random World",
//            IsNewRwg = true
//        };
//        if (___cbxWorldName.Elements.Contains(random))
//        {
//            Debug.Log("Removing Random World");
//            ___cbxWorldName.Elements.Remove(random);
//        }

//    }
//}

