using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Platform;

[HarmonyPatch(typeof(PlayerMoveController))]
[HarmonyPatch("Update")]
public class PlayerMoveControllerUpdateNPCv2 {
    /*
     * This patch is originally intended to change   string text = null; into something different.
     * Since our custom NPCs aren't necessarily are a type of EntityTrader, we want to pre-set the text value
     * if we are looking at an EntityAliveV2.
     */
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator generator, MethodBase method) {
        // To Find the index, uncomment and follow the instructions in this method:
        // ILUtilities.DisplayLocalVariables(method);

        // This index is not the index of the instruction list, which would include a listing of all the opcodes / operands,
        // rather, this is the 32nd local variable defined. We don't know what it is, or where it's at in the instructions,
        // but the variableInformation will contain the operand fingerprint
        var index = 31;
        var variableInformation = ILUtilities.FindLocalVariable(method, index);

        // Grab all the instructions
        var codes = new List<CodeInstruction>(instructions);

        // startIndex is -1, so on the first loop of the first instruction, it'll be 0
        // In the end, we want the instruction right before the opcode/operand we are looking for
        var startIndex = -1;
        foreach (var t in codes)
        {
            // This opcode + operand is equivalent to
            //  string text = null;
            if (t.opcode == OpCodes.Stloc_S && t.operand.ToString() == variableInformation)
            {
                // We found our line, so let's break out, keeping the startIndex to the previous line, which is what we want to change
                break;
            }

            startIndex++;
        }

        if (startIndex > 0 && startIndex < codes.Count)
        {
            // Replace the null from the "= null" into "= CheckText()". The SymbolExtensions sorts out the full path to the method.
            var checkText = SymbolExtensions.GetMethodInfo(() => SetInitialText());
            codes[startIndex] = new CodeInstruction(OpCodes.Call, checkText);
        }

        return codes.AsEnumerable();
    }

    // This handles displaying the radial dial of available chat commands. 
    // Since the Radial dial needs a TileEntity, even if it's just to use it's position, we want to use entityAliveV2's GetTileEntity() short cut.
    // By default, this will return SCore's TileEntityAoE, which is just a limited Tile Entity that meets the basic requirements.
    private static void Postfix() {
        var localPlayerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
        if (localPlayerUI == null) return;
        var entityPlayerLocal = localPlayerUI.entityPlayer;
        if (!entityPlayerLocal.IsAlive()) return;
        
        if (localPlayerUI.windowManager.IsInputActive() && localPlayerUI.windowManager.IsFullHUDDisabled() ) return;

        // Check to see if the button is currently pressed, and was held down. This is to notify the system that you want
        // to see the radial
        var isPressed = localPlayerUI.playerInput.Activate.IsPressed ||
                         localPlayerUI.playerInput.PermanentActions.Activate.IsPressed;
        var wasPressed = localPlayerUI.playerInput.Activate.WasPressed ||
                         localPlayerUI.playerInput.PermanentActions.Activate.WasPressed;
        if (!isPressed || !wasPressed) return;

        var entityAliveV2 = CheckForEntityAlive();
        if (entityAliveV2 == null) return;

        var tileEntity = entityAliveV2.GetTileEntity();
        if (tileEntity == null) return;

        entityPlayerLocal.AimingGun = false;
        localPlayerUI.xui.RadialWindow.Open();
        localPlayerUI.xui.RadialWindow.SetCurrentEntityData(entityAliveV2.world, entityAliveV2, tileEntity, entityPlayerLocal);

    }

    private static EntityAliveV2 CheckForEntityAlive() {
        var localPlayerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
        if (localPlayerUI == null) return null;
        var entityPlayerLocal = localPlayerUI.entityPlayer;
        if (!entityPlayerLocal.IsAlive()) return null;

        var hitInfo = entityPlayerLocal.HitInfo;
        if (!hitInfo.bHitValid ||
            !hitInfo.tag.StartsWith("E_") ||
            hitInfo.hit.distanceSq >= Constants.cCollectItemDistance * Constants.cCollectItemDistance)
            return null;

        var rootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
        if (rootTransform == null)
            return null;

        var entity = rootTransform.GetComponent<Entity>();
        if (entity is not EntityAliveV2 entityAliveV2 || !entityAliveV2.IsAlive())
            return null;
        return entityAliveV2;
    }
    private static string SetInitialText() {
        var localPlayerUI = LocalPlayerUI.GetUIForPrimaryPlayer();
        if (localPlayerUI == null) return null;
        var entityPlayerLocal = localPlayerUI.entityPlayer;
        if (!entityPlayerLocal.IsAlive())
            return null;

        var entityAliveV2 = CheckForEntityAlive();
        if (entityAliveV2 == null) return null;
/*
            var talkTag = FastTags.Parse("talkable");
            if (!npc.HasAnyTags(talkTag)) return null;
            if (string.IsNullOrEmpty(npc.NPCInfo.DialogID)) return null;
  */
        var text10 =
            localPlayerUI.playerInput.Activate.GetBindingXuiMarkupString() + localPlayerUI.playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
        var text11 = Localization.Get(entityAliveV2.EntityName);
        var text = string.Format(Localization.Get("npcTooltipTalk"), text10, text11);
        return text;
    }
}