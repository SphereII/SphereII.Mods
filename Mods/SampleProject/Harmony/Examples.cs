using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Audio;
using HarmonyLib;
using UnityEngine;

namespace SampleProject.Harmony
{
    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SampleProjectPrefix
    {
        // The __instance is a reference to the class you are patching. 
        // To access a private field of the class, add it to the parameter line, adding three underscore (___) to the variable name.
        // To access a private field, and to change its value, pass it by reference by adding ref to it.
        private static bool Prefix(XUiC_MainMenu __instance, XUiC_SimpleButton ___btnNewGame, ref XUiC_SimpleButton ___btnContinueGame)
        { 
            Log.Out($"SampleProject Prefix Example: Am I opened? {__instance.IsOpen}");
            Log.Out($"btnNewGame's Label: {___btnNewGame.Label}");
            Log.Out("OnOpenPrefix(): I happen before the method starts.");

            // If I did not want the method we are patching to run at all, we would return false.
            return true;
        }
    }

    [HarmonyPatch(typeof(XUiC_MainMenu))]
    [HarmonyPatch("OnOpen")]
    public class SampleProjectPostfix
    {
        // A Postfix can have a return type of void, or it can have a return type of the method you are patching.
        private static void Postfix(XUiC_MainMenu __instance)
        {
            Log.Out($"SampleProject Postfix Example: Am I opened? {__instance.IsOpen}");
            Log.Out("OnOpenPostfix(): I happen after the method is done!");
        }
    }

  
    // If there's overloaded methods, you need to specify the parameter list. Here's one for Client.Play(), which is overloaded.
    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    // Target the Client.Play() which takes an int, a string, and a float as parameter.
    [HarmonyPatch(new[] { typeof(int), typeof(string), typeof(float) })]
    public class AudioClientPlay
    {
        // the parameter list must match vanilla, typos included!
        private static bool Prefix(int playOnEntityId, string soundGoupName, float _occlusion)
        {
            return true;
        }
    }

    [HarmonyPatch(typeof(Client))]
    [HarmonyPatch("Play")]
    [HarmonyPatch(new[] { typeof(Vector3), typeof(string), typeof(float), typeof(int) })]
    public class AudioClientPlay2
    {
        private static bool Prefix(Vector3 position, string soundGoupName)
        {
            return true;
        }
    }
}
