using HarmonyLib;

namespace Harmony.NPCFeatures
{
    /**
     * AN NPC'S NOCKED ARROW CAN BE TAKEN WITH E.
     *
     * ProjectileMoveScript.TryCollect checks only itemProjectile.IsSticky before handing the
     * player an arrow and destroying the GameObject. It never asks whether the projectile has
     * actually been fired and come to rest, so it will collect any arrow object it is called on.
     *
     * The exposure is not theoretical. ItemActionLauncher.instantiateProjectile adds the
     * ProjectileMoveScript to the arrow model while that model is still parented to the holder's
     * right hand, well before Fire() ever runs - so a nocked arrow is a live, collectable
     * ProjectileMoveScript sitting in state Idle with ProjectileID == InvalidID.
     * PlayerMoveController's activation raycast then does GetComponentInChildren on whatever it
     * hit and calls TryCollect with no state check of its own.
     *
     * A player's own nocked arrow is safe only by accident: the player model sits on layer 24,
     * outside the E raycast's mask. SetModelLayer is a no-op on both EntityAliveSDX and
     * EntityAliveSDXV4, so an SDX NPC's in-hand arrow stays reachable - and taking it gives the
     * player a free arrow while the NPC keeps its loaded round. Free ammunition, repeatable.
     *
     * THE FIX: restore the invariant TryCollect always assumed - a projectile is collectable only
     * once it has stuck in the world and been registered with the ProjectileManager. Both halves
     * are set together, on both stick paths in checkCollision: ProjectileID is assigned from
     * ProjectileManager.AddProjectileItem and SetState(State.Sticky) immediately follows. A
     * nocked arrow has neither; an in-flight one (Active) has neither; a spent or destroyed one
     * (Dead / StickyDestroyed) has no valid id. Every legitimately collectable arrow passes.
     *
     * This closes the hole for both NPC generations, not just V4, since it fixes the collection
     * side rather than the entity side.
     *
     * Ungated on purpose. This is a correctness fix, not a feature: for every projectile stock
     * meant to be collectable the prefix is a no-op and the original runs untouched.
     *
     * Residue worth knowing about: the "press E to pick up <item>" prompt is built by
     * PlayerMoveController.TryHandleProjectile regardless of what TryCollect returns, so a player
     * looking at an NPC's nocked arrow still sees the prompt - pressing E now simply does
     * nothing. Suppressing the prompt too would mean patching a local function nested inside
     * PlayerMoveController, which is not worth the fragility for a cosmetic wart.
     */
    public class ProjectileCollectStateCheck
    {
        [HarmonyPatch(typeof(ProjectileMoveScript))]
        [HarmonyPatch("TryCollect")]
        public class ProjectileMoveScriptTryCollect
        {
            public static bool Prefix(ProjectileMoveScript __instance, ref bool __result)
            {
                // Stuck in the world AND registered with the ProjectileManager. Anything else -
                // a nocked arrow, one still in flight, one already spent - is not collectable.
                if (__instance.state == ProjectileMoveScript.State.Sticky &&
                    __instance.ProjectileID != ProjectileMoveScript.InvalidID)
                    return true;

                // Report "not collected" the same way stock reports a non-sticky projectile: no
                // tooltip, no sound, no side effects. The caller already handles a false return.
                __result = false;
                return false;
            }
        }
    }
}
