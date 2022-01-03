using HarmonyLib;

//	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
//	Credits: Sirillion.
//	Tweaked: sphereii.

//	Adds an extra binding to hide buff background depending on a buff existing or not.
//	Difference: Vanilla has no binding for this, and as such every grid cell would get a background drawn regardless of it having a buff or not.

public class SMXhud_collecteditemlist_xuic
{
    [HarmonyPatch(typeof(XUiC_CollectedItemList))]
    [HarmonyPatch("SetYOffset")]

    public class SMXhudCollectedItemList
    {
        public static bool Prefix(ref bool __result, ref int _yOffset, ref int ___yOffset)
        {
            if (_yOffset != ___yOffset)
            {
                return false;
            }
            return false;
        }
    }
}