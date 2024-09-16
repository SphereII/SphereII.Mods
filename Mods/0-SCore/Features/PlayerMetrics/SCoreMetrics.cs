namespace SCore.Features.PlayerMetrics {
    public class SCoreMetrics {
        public static void UpdateCVar(string cvarName, int newValue) {
            if (string.IsNullOrEmpty(cvarName)) return;
            var player = GameManager.Instance.World.GetPrimaryPlayer();
            if (player == null) return;
            var currentValue = player.Buffs.GetCustomVar(cvarName);
            if (currentValue > newValue) return;
            player.Buffs.SetCustomVar(cvarName, newValue);
        }
    }
}