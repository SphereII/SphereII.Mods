using HarmonyLib;
using System.Xml;
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
    [HarmonyPatch("ParseRequirement")]
    public class SphereII__DialogFromXML_ParseRequirement
    {
        static void Postfix(BaseDialogRequirement __result, XElement e)
        {
            if (__result is DialogRequirementHasCVarSDX)
            {
                if (e.HasAttribute("operator"))
                    (__result as DialogRequirementHasCVarSDX).strOperator = e.GetAttribute("operator");
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
        static void Postfix(BaseDialogAction __result, XElement e)
        {
            if (__result is DialogActionAddCVar)
            {
                if (e.HasAttribute("operator"))
                    (__result as DialogActionAddCVar).strOperator = e.GetAttribute("operator");
            }

        }
    }


    // Extends
    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch("ParseDialog")]
    public class SphereII__DialogFromXML_ParseDialog
    {
        static void Postfix(ref Dialog __result, XElement e)
        {
            if (e.HasAttribute("extends"))
            {
                var dialogNode = e.GetAttribute("extends");
                if (string.IsNullOrEmpty(dialogNode)) return;

                if (Dialog.DialogList.TryGetValue(dialogNode, out var dialog))
                {
                    //foreach (var statement in dialog.Statements)
                    //{
                    //    var originalStatement = __result.Statements.Find(x => x.ID == statement.ID);
                    //    if (originalStatement == null) // If we don't have the statement, just add it.
                    //    {
                    //        __result.Statements.Add(statement);
                    //        continue;
                    //    }
                    //    // We have an existing statement, so let's begin the merge.
                    //    foreach (var entry in statement.GetResponses())
                    //    {
                    //        var original = __result.Statements.Find(x => x.ID == entry.ID);
                    //        if (original == null)
                    //            originalStatement.ResponseEntries.Add(entry);
                    //    }

                    //    foreach (var entry in statement.Actions)
                    //    {
                    //        var original = __result.Statements.Find(x => x.ID == entry.ID);
                    //        if (original == null)
                    //            originalStatement.Actions.Add(entry);
                    //    }
                    //}
                    __result.Statements.AddRange(dialog.Statements);
                    __result.Responses.AddRange(dialog.Responses);
                    __result.Phases.AddRange(dialog.Phases);
                    __result.QuestEntryList.AddRange(dialog.QuestEntryList);
                }
            }

        }
    }


}