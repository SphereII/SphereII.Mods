using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using HarmonyLib;
using UnityEngine;

public class DialogExtendsPatches
{
    [HarmonyPatch(typeof(DialogFromXml))]
    [HarmonyPatch(nameof(DialogFromXml.ParseDialog))]
    public class DialogFromXmlParseDialog
    {
        static void Postfix(ref Dialog __result, XElement e)
        {
            if (!e.HasAttribute("extends")) return;
            var extends = e.GetAttribute("extends");
            if (string.IsNullOrEmpty(extends))
            {
                Debug.Log("Dialog Extends is empty");
                return;
            }
            __result.Phases ??= new List<DialogPhase>();
            __result.Statements ??= new List<DialogStatement>();
            __result.Responses ??= new List<DialogResponse>();
            __result.QuestEntryList ??= new List<QuestEntry>();

            Debug.Log("Extending. Original Dialog is: " + __result.ID + " Extending: " + extends);
            Debug.Log("Dialog Statement Counts: " + __result.Statements.Count + " " + __result.Phases.Count + " " + __result.Responses.Count);
            foreach (var dialogNode in extends.Split(','))
            {

                if (Dialog.DialogList.TryGetValue(dialogNode, out Dialog sourceDialog))
                {
                    Debug.Log($"{sourceDialog.ID} Statement Counts: " + sourceDialog.Statements.Count + " " + sourceDialog.Phases.Count + " " + sourceDialog.Responses.Count);

                    // Copy elements from the source dialog into the new one.
                    // Existing Add methods handle overwriting/adding logic.
                    foreach (var phase in sourceDialog.Phases)
                    {
                        __result.Phases.Add(phase); // Assumes AddPhase handles ID check & addition/overwrite
                    }


                    foreach (var response in sourceDialog.Responses)
                    {
                        __result.Responses.Add(response); // Assumes AddResponse handles ID check & addition/overwrite
                    }
                    MergeDialogStatements(ref __result, sourceDialog);
                }

           
            }
            Debug.Log("Dialog Merge Complete Statement Counts: " + __result.Statements.Count + " " +
                      __result.Phases.Count + " " + __result.Responses.Count);
        }
        
    }

    private static void MergeDialogStatements(ref Dialog __result, Dialog dialog)
    {
        foreach (var statement in dialog.Statements)
        {
            var originalStatement = __result.Statements.Find(x => x.ID == statement.ID);
            if (originalStatement == null) // If we don't have the statement, just add it.
            {
                 __result.Statements.Add(statement);
                continue;
            }

            // We have an existing statement, so let's begin the merge.
            foreach (var entry in statement.ResponseEntries)
            {
                var original = originalStatement.ResponseEntries
                    .Find(x => x.ID == entry.ID);
                if (original == null)
                {
                    originalStatement.ResponseEntries.Add(entry);
                }
            }
            // Sort the responses so done is always last.
            originalStatement.ResponseEntries = originalStatement.ResponseEntries
                .OrderBy(item => item.ID == "done") // false comes before true
                .ToList();
     
        }
    }
}
