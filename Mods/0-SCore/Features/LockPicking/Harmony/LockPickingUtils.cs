using Platform;

namespace Features.LockPicking {
    public class LockPickingUtils {
        public static bool CheckForMiniGame(EntityAlive entityAlive) {
            if (entityAlive == null) return false;

            if (entityAlive.Buffs.HasCustomVar("LegacyLockPick") && entityAlive.Buffs.GetCustomVar("LegacyLockPick") > 0)
                return false;

            if (entityAlive.Buffs.HasCustomVar("MiniGameLockPick") && entityAlive.Buffs.GetCustomVar("MiniGameLockPick") > 0)
                return true;

            // If they have a controller, skip the mini game
            if (PlatformManager.NativePlatform.Input.CurrentInputStyle == PlayerInputManager.InputStyle.Keyboard)
                return true;
            
            return false;
        }
    }
}