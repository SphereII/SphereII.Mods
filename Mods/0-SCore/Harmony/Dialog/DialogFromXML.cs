using HarmonyLib;
using System.Xml.Linq;

/**
 * SphereII_DialogFromXML_Extensions
 *
 * This class includes a Harmony patches to allow loading up extra custom dialog elements
 *
 * Usage:
 *   <action type="AddCVar, Mods" id="quest_Samara_Diary" value="1" operator="set" />
 *   <requirement type="HasBuffSDX, Mods" value="buffCursedSamaraMorgan" requirementtype="Hide" Hash="Requirement_-101666296" />
 *   <requirement type="HasCVarSDX, Mods" value="1" requirementtype="Hide" operator="GTE" id="quest_Samara_Diary" Hash="Requirement_-2138114132" />
 *   <requirement type="HasBuffSDX, Mods" value="buffBadAttitude" match="not" requirementtype="Hide" Hash="Requirement_-1230867493" />
  */
public class SphereII_DialogFromXML_Extensions
{
    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch(nameof(DialogFromXml.ParseRequirement))]
    public class SphereII__DialogFromXML_ParseRequirement
    {
        static void Postfix(BaseDialogRequirement __result, XElement e)
        {
            if (__result is IDialogOperator dialogOperatorRequirement &&
                e.HasAttribute("operator"))
            {
                dialogOperatorRequirement.Operator = e.GetAttribute("operator");
            }
        }
    }

    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch(nameof(DialogFromXml.ParseAction))]
    public class SphereII__DialogFromXML_ParseAction
    {
        static void Postfix(BaseDialogAction __result, XElement e)
        {
            if (__result is IDialogOperator dialogOperatorAction &&
                e.HasAttribute("operator"))
            {
                dialogOperatorAction.Operator = e.GetAttribute("operator");
            }
        }
    }

    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch(nameof(DialogFromXml.ParseResponse))]
    public class SphereII__DialogFromXML_ParseResponse
    {
        static void Postfix(ref Dialog dialog, XElement e)
        {
            if (dialog is IDialogOperator dialogOperatorResponse &&
                e.HasAttribute("operator"))
            {
                dialogOperatorResponse.Operator = e.GetAttribute("operator");
            }
        }
    }


}