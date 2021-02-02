using HarmonyLib;
using System.Xml;

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
    [HarmonyPatch("ParseRequirement")]
    public class SphereII__DialogFromXML_ParseRequirement
    {
        static void Postfix(BaseDialogRequirement __result, XmlElement e)
        {
            if (__result is DialogRequirementHasCVarSDX)
            {
                if (e.HasAttribute("operator"))
                    (__result as DialogRequirementHasCVarSDX).strOperator = e.GetAttribute("operator");
            }

            if (__result is DialogRequirementHasBuffSDX)
            {
                if (e.HasAttribute("match"))
                    (__result as DialogRequirementHasBuffSDX).strMatch = e.GetAttribute("match");
            }

            if (__result is DialogRequirementFactionValue)
            {
                if (e.HasAttribute("operator"))
                    (__result as DialogRequirementFactionValue).strOperator = e.GetAttribute("operator");
            }
        }
    }
    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch("ParseAction")]
    public class SphereII__DialogFromXML_ParseAction
    {
        static void Postfix(BaseDialogAction __result, XmlElement e)
        {
            if (__result is DialogActionAddCVar)
            {
                if (e.HasAttribute("operator"))
                    (__result as DialogActionAddCVar).strOperator = e.GetAttribute("operator");
            }

        }
    }


}