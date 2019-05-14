using DMT;
using Harmony;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class SphereII__EntityMoveHelper
{
    public class SphereII_EntityMNoveHelperTweaks : IHarmony
    {
        public void Start()
        {
            Debug.Log(" Loading Patch: " + GetType().ToString());
            var harmony = HarmonyInstance.Create(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof(EntityMoveHelper))]
    [HarmonyPatch("CheckBlocked")]
    public class SphereII_EntityMoveHelperCheckBlock
    {
        static bool Prefix(EntityMoveHelper __instance, Vector3 pos, Vector3 endPos, int baseY)
        {
            __instance.IsBlocked = false;
            BlockValue myBlock = GameManager.Instance.World.GetBlock( new Vector3i(endPos ));
            if(myBlock.Block.FilterTags.Contains("ftraps"))
            {
                Debug.Log("HarmonyPatch: CheckedBlock - Trap detected");
                __instance.IsBlocked = true;
                return false;
            }
            return true;

        }
    }
}