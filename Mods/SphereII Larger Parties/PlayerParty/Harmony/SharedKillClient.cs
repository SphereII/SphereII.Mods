using System;
using HarmonyLib;
using UnityEngine;

namespace LargerParties.PlayerParty.Harmony {
    [HarmonyPatch(typeof(GameManager))]
    [HarmonyPatch(nameof(GameManager.SharedKillClient))]
    public class GameManagerSharedKillClient {
        public static bool Prefix(ref int _xp) {
            _xp = Math.Max(PlayerPartySettings.MinExp, _xp);
            return true;
        }
    }
}