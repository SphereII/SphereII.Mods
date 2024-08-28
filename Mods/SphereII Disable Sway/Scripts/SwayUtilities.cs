    public class SwayUtilities {
            public static bool CanSway(bool force = false) {
                if (ModManager.ModLoaded("Z-SphereIIDebugging")) return true;
                return false;
            }
    }
