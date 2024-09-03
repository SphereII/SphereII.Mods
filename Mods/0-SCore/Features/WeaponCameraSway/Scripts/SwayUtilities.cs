    public class SwayUtilities {
            public static bool CanSway(bool force = false) {
                if (force)
                    return true;
                if (GameManager.Instance.World == null) return true;
                var player = GameManager.Instance.World.GetPrimaryPlayer();
                if (player == null) return true;
                if (!player.Buffs.HasCustomVar("$WeaponSway")) return true;
                var sway = player.Buffs.GetCustomVar("$WeaponSway");
                return sway < 1f;
            }

    }
