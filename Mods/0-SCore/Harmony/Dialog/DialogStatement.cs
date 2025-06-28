using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Harmony.Dialog
{
    public class DialogStatementPatches
    {
        /*
         * This patch is necessary to allow actions to be added to a statement. The game already supports actions to statements,
         * but the GetRespones() doesn't create a full response for the [ Next ] lines. It just returns a pre-generated " [Next]"
         * to pass the NextStatementID. Instead, we'll loop around and grab the full reference to it.
         */
        [HarmonyPatch(typeof(DialogStatement))]
        [HarmonyPatch("GetResponses")]
        public class DialogStatementGetResponses
        {
            public static void Postfix(DialogStatement __instance, ref List<BaseResponseEntry> __result)
            {
                if ( __result == null) return;
                // If this isn't a generated Next, skip it.
                if (__result.Count >= 0) return;

                
                var response = __result[0].Response;
                var statementID = response.NextStatementID;
                
                // Not a next statement? skip.
                if (response.Text != "[" + Localization.Get("xuiNext") + "]") return;

                // Find the actual statement and copy its actions over to the returned response.
                foreach (var t in __instance.OwnerDialog.Statements)
                {
                    if (t.NextStatementID != statementID) continue;
                    __result[0].Response.Actions = t.Actions;
                    break;
                }
            
            }
        } 
    }
}