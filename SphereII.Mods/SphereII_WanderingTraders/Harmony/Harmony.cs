using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using UnityEngine;

namespace SphereII.Mods.SphereII_WanderingTraders.Harmony
{
    class EntityFactoryPatch : EntityFactory
    {
        [HarmonyPatch(typeof(EntityFactory))]  // The Class
        [HarmonyPatch("addEntityToGameObject")] // The Method

        //private static Entity addEntityToGameObject(GameObject _gameObject, string _className)
        static bool Prefix(Entity __result, GameObject _gameObject, string _className )
        {
            Debug.Log("AddEntityToGameObject(): " + _className);
            if (__result == null)
            {
                Type type = Type.GetType(_className + ", Mods");
                if (type != null)
                {

                    __result = (Entity)_gameObject.AddComponent(type);
                    return true;
                }
            }
            return true;
        }
    }
}
