/// <summary>
/// Gate for the four Harmony prefixes in this modlet - vp_FPCamera and vp_FPWeapon,
/// UpdateSwaying and UpdateBob. Returning true lets the original routine run (motion
/// happens); returning false skips it (motion suppressed).
/// </summary>
public class SwayUtilities {
    /// <summary>
    /// Set by the "weaponsway true/false" console command.
    ///
    /// SCore's WeaponCameraSway feature reads this same cvar with this same polarity:
    /// 1 suppresses sway, 0 restores it. That agreement is not optional. Both copies
    /// patch the same four methods, and Harmony skips the original if ANY prefix
    /// returns false, so opposite readings would leave the command unable to turn
    /// motion back on whenever SCore is also installed.
    ///
    /// Where the two deliberately differ is the default. SCore starts from vanilla
    /// sway and lets the cvar switch it off. This modlet exists to have it off from
    /// the start, so an absent cvar means no sway rather than sway.
    /// </summary>
    private const string SwayCVar = "$WeaponSway";

    public static bool CanSway(bool force = false) {
        if (force) return true;

        // Dev escape hatch: with the debugging modlet installed, leave motion alone.
        if (ModManager.ModLoaded("Z-SphereIIDebugging")) return true;

        if (GameManager.Instance.World == null) return false;

        // GetPrimaryPlayer is a plain field read, so calling it from four prefixes
        // every frame costs nothing worth caching around.
        var player = GameManager.Instance.World.GetPrimaryPlayer();
        if (player == null) return false;

        if (!player.Buffs.HasCustomVar(SwayCVar)) return false;
        return player.Buffs.GetCustomVar(SwayCVar) < 1f;
    }
}