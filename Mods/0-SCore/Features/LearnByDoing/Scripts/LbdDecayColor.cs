using UnityEngine;

namespace SCore.Features.LearnByDoing.Scripts
{
    /// <summary>
    /// Shared colours for the Learn By Doing progress bars, exposed to XUi through the
    /// "color" binding on the three skill row controllers.
    ///
    /// These are Color32 (byte 0-255), not Color (float 0-1), and that distinction is the
    /// whole reason this class exists. CachedStringFormatterXuiRgbaColor takes a Color32,
    /// so passing a Color goes through Unity's implicit conversion, which is
    /// Mathf.Clamp01(c.x) * 255 per channel. A literal written as if it were bytes -
    /// new Color(0, 255, 54, 128) - therefore clamps every non-zero channel to 1 and
    /// arrives as opaque cyan (0, 255, 255, 255) rather than translucent green.
    ///
    /// Alpha stays at 128 so the bar reads as an overlay on the row beneath it, matching
    /// the literal the XML used before the binding was wired up.
    /// </summary>
    public static class LbdDecayColor
    {
        /// <summary>Decay counter at or below <see cref="WarningThreshold"/>: skill is being used.</summary>
        public static readonly Color32 Healthy = new Color32(0, 255, 54, 128);

        /// <summary>Idle long enough to be worth noticing, but not yet losing levels.</summary>
        public static readonly Color32 Warning = new Color32(255, 255, 0, 128);

        /// <summary>Idle long enough that decay is imminent or already happening.</summary>
        public static readonly Color32 Decaying = new Color32(255, 0, 0, 128);

        public const int WarningThreshold = 2;
        public const int DecayingThreshold = 4;

        /// <summary>
        /// Picks the bar colour for a decay counter. Kept here so the three row
        /// controllers - skill list entry, attribute level and perk level - cannot drift
        /// apart in either thresholds or colours.
        /// </summary>
        public static Color32 ForDecayCounter(float _decayCounter)
        {
            if (_decayCounter <= WarningThreshold) return Healthy;
            return _decayCounter <= DecayingThreshold ? Warning : Decaying;
        }
    }
}
