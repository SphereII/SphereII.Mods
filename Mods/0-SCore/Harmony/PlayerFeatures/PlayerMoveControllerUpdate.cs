using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using HarmonyLib;
using UnityEngine;

namespace Harmony.PlayerFeatures {

    /**
     * SCorePlayerMoveController_Update
     *
     * This class includes a Harmony patches to the EntityPlayer Local to allow skipping of buffs that contain the name "buffcutscene" by pressing space or escape.
     *
     * This was used in the Winter Project 2019 to skip the opening cutscene, which was applied through a buff.
     */
    [HarmonyPatch(typeof(PlayerMoveController))]
    [HarmonyPatch("Update")]
    public class PlayerMoveControllerUpdate {
        // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
        private static bool Prefix(PlayerMoveController __instance, EntityPlayerLocal ___entityPlayerLocal) {
            if (__instance.playerInput.Jump.IsPressed || __instance.playerInput.Menu.IsPressed)
                foreach (var buff in ___entityPlayerLocal.Buffs.ActiveBuffs)
                    if (buff.BuffName.ToLower().Contains("buffcutscene"))
                        ___entityPlayerLocal.Buffs.RemoveBuff(buff.BuffName);
            return true;
        }

        private static readonly string AdvancedFeatureClass = "AdvancedNPCFeatures";
        private static readonly string AdvancedEnemyNPCsFeature = "AdvancedEnemyNPCs";

        private static bool _initialized = false;
        private static bool _enabled = false;

        /// <summary>
        /// Disables the "Press E to interact..." prompt if the NPC is an enemy.
        /// This is only needed if some enemy NPCs use EntityAliveSDX.
        /// </summary>
        /// <param name="___entityPlayerLocal"></param>
        /// <param name="___strTextLabelPointingTo"></param>
        public static void Postfix(
            EntityPlayerLocal ___entityPlayerLocal,
            ref string ___strTextLabelPointingTo) {
            if (!_initialized)
                Initialize();

            if (!_enabled)
                return;

            if (!___entityPlayerLocal.IsAlive())
                return;

            var hitInfo = ___entityPlayerLocal.HitInfo;

            // This is how the code determines if it's an entity that can be interacted with
            if (!hitInfo.bHitValid ||
                !hitInfo.tag.StartsWith("E_") ||
                hitInfo.hit.distanceSq >= Constants.cCollectItemDistance * Constants.cCollectItemDistance)
                return;

            var rootTransform = GameUtils.GetHitRootTransform(hitInfo.tag, hitInfo.transform);
            if (rootTransform == null)
                return;

            var entity = rootTransform.GetComponent<Entity>();
            if (entity is not EntityNPC npc || !npc.IsAlive())
                return;

            
            // This is how the code determines if it's a trader/EntityAliveSDX (vs. a drone)
            if (GameManager.Instance.World.GetTileEntity(npc.entityId) is not TileEntityTrader)
                return;


            // At this point we know it's a trader or EntityAliveSDX and that the "Press E..."
            // prompt is to be shown; now determine if it's an enemy, and if so, hide the prompt
            if (ShouldTalk(___entityPlayerLocal, npc))
                return;

            ___strTextLabelPointingTo = string.Empty;
            XUiC_InteractionPrompt.SetText(___entityPlayerLocal.PlayerUI, null);
        }

        private static void Initialize() {
            _initialized = true;

            _enabled = Configuration.CheckFeatureStatus(
                AdvancedFeatureClass,
                AdvancedEnemyNPCsFeature);

            AdvLogging.DisplayLog(
                AdvancedFeatureClass,
                $"{AdvancedEnemyNPCsFeature} {(_enabled ? "en" : "dis")}abled");
        }

        /// <summary>
        /// This is a specialized method, which basically returns false if the NPC is an enemy
        /// of the player. We already know the entity types, that neither entity is dead or null,
        /// and that a player can't have a leader, so we don't need to do all of the checks in
        /// <see cref="EntityTargetingUtilities.IsEnemy(global::EntityAlive, Entity)"/>.
        /// <param name="player"></param>
        /// <param name="npc"></param>
        /// <returns></returns>
        private static bool ShouldTalk(EntityPlayerLocal player, EntityNPC npc) {
            if (EntityTargetingUtilities.IsAlly(npc, player))
                return true;

            if (EntityTargetingUtilities.IsFightingFollowers(player, npc))
                return false;

            return !EntityTargetingUtilities.IsEnemyByFaction(npc, player);
        }
    }
}