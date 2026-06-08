using BreathTower.Data;
using UnityEngine;

namespace BreathTower.Core
{
    /// <summary>
    /// Thin haptics wrapper replacing expo-haptics. Honors the user's
    /// "Haptic Feedback" setting. On Android it uses the device vibrator;
    /// on other platforms it is a no-op.
    /// </summary>
    public static class Haptics
    {
        public static void Light()
        {
            if (!SaveSystem.Data.settings.vibrationEnabled) return;
            Vibrate();
        }

        public static void Success()
        {
            if (!SaveSystem.Data.settings.vibrationEnabled) return;
            Vibrate();
        }

        private static void Vibrate()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }
    }
}
