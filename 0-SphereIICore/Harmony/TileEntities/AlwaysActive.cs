using Harmony;
using System;
using System.Collections.Generic;

public class SphereII_TileEntityAlwaysActive
{
    [HarmonyPatch(typeof(TileEntity))]
    [HarmonyPatch("IsActive")]
    public class SphereII_TileEntity_ISActive
    {
        public static bool Postfix(bool __result, TileEntity __instance)
        {
            // if it's already active, don't check.
            if(__result)
                return __result;

            BlockValue block = GameManager.Instance.World.GetBlock(__instance.ToWorldPos());
            Block block2 = Block.list[block.type];
            bool isAlwaysActive = false;
            if(block2.Properties.Values.ContainsKey("AlwaysActive"))
            {
                isAlwaysActive = StringParsers.ParseBool(block2.Properties.Values["AlwaysActive"], 0, -1, true);
                if(isAlwaysActive)
                    return true;
            }

            return __result;
        }
    }


}