using HarmonyLib;

/// <summary>
/// Wires the two SCore entries appended to the vanilla ESC menu column - SCore Utilities
/// and NPC Settings - in Config/XUi_InGame/windows.xml.
///
/// This is the one part XML cannot do. XUiC_InGameMenuWindow.Init looks up exactly ten
/// buttons by name - btnOptions, btnSandboxSettings, btnExit and so on - and attaches a
/// C# handler to each. A button inserted from XML is never in that list, so it renders
/// and highlights but does nothing on press. 0-Help Screens hangs its own ESC menu button
/// off the same hook for the same reason.
///
/// This used to be a window controller instead: ingameMenuSCore was a second window in
/// the ingameMenu group, floating off to the right with its own two-cell grid, and being
/// its controller was what gave the buttons their handlers. Folding the buttons into the
/// vanilla grid costs that, so the wiring moves here.
/// </summary>
[HarmonyPatch(typeof(XUiC_InGameMenuWindow))]
[HarmonyPatch(nameof(XUiC_InGameMenuWindow.Init))]
public class XUiC_SCoreInGameMenu
{
    /// <summary>Names match the buttons inserted in Config/XUi_InGame/windows.xml.</summary>
    private const string OptionsButtonName = "btnSCoreOptions";

    private const string CompanionsButtonName = "btnNPCView";

    public static void Postfix(XUiC_InGameMenuWindow __instance)
    {
        Wire(__instance, OptionsButtonName, () => XUiC_SCoreUtilities.ID);
        Wire(__instance, CompanionsButtonName, () => XUiC_SCoreCompanionList.ID);
    }

    /// <summary>
    /// The window group id is read at press time, not here: XUiC_SCoreUtilities and
    /// XUiC_SCoreCompanionList each set their static ID in their own Init, and nothing
    /// guarantees those ran before this patch does.
    /// </summary>
    private static void Wire(XUiC_InGameMenuWindow menu, string buttonName, System.Func<string> windowGroup)
    {
        // Unlike vanilla's own lookups, this one is guarded. If our windows.xml patch did
        // not apply - a game update moving the ESC menu, or a load-order accident - the
        // button simply is not there, and the ESC menu must still work.
        var holder = menu.GetChildById(buttonName);
        if (holder == null)
        {
            Log.Warning($"[SCore] '{buttonName}' not found in the ESC menu; that entry will " +
                        "not be available. Check that Config/XUi_InGame/windows.xml applied.");
            return;
        }

        var button = holder.GetChildByType<XUiC_SimpleButton>();
        if (button == null)
        {
            Log.Warning($"[SCore] '{buttonName}' has no XUiC_SimpleButton child.");
            return;
        }

        button.OnPressed += (_sender, _mouseButton) =>
        {
            var id = windowGroup();
            if (string.IsNullOrEmpty(id))
            {
                Log.Warning($"[SCore] '{buttonName}' pressed but its window group is not registered yet.");
                return;
            }

            // Same two steps vanilla uses for Sandbox Settings: drop the ESC menu, then
            // open ours modally. Opening without closing leaves the menu underneath and
            // its buttons still clickable.
            var windowManager = menu.xui.playerUI.windowManager;
            windowManager.Close(menu.windowGroup);
            windowManager.Open(id, true);
        };
    }
}
