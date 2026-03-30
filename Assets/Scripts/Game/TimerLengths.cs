using UnityEngine;

namespace Game {
    public static class TimerLengths {
        private static TimerLengthsSO lengthsConfig;
        private static string ConfigPath = "Config/Timer Lengths";
        private enum TimerLengthOptions {
            LessTime,
            Default,
            MoreTime
        }

        private static TimerLengthOptions GetTimerLength() {
            return GameSettings.TimerLengths switch
            {
                0 => TimerLengthOptions.LessTime,
                1 => TimerLengthOptions.Default,
                2 => TimerLengthOptions.MoreTime,
                _ => TimerLengthOptions.Default
            };
        }
        
        public static float GetShopTimerLengthInSeconds() {
            EnsureTimerLengthsConfigLoaded();
            TimerLengthOptions timerLengthOption = GetTimerLength();
            switch (timerLengthOption) {
                case TimerLengthOptions.LessTime:
                    return lengthsConfig.ShopTimerLength_LessTime;
                case TimerLengthOptions.MoreTime:
                    return lengthsConfig.ShopTimerLength_MoreTime;
                default:
                case TimerLengthOptions.Default:
                    return lengthsConfig.ShopTimerLength_Default;
            }
        }

        public static float GetMinigameTimerLengthInSeconds() {
            EnsureTimerLengthsConfigLoaded();
            TimerLengthOptions timerLengthOption = GetTimerLength();
            switch (timerLengthOption) {
                case TimerLengthOptions.LessTime:
                    return lengthsConfig.MinigameTimerLength_LessTime;
                case TimerLengthOptions.MoreTime:
                    return lengthsConfig.MinigameTimerLength_MoreTime;
                default:
                case TimerLengthOptions.Default:
                    return lengthsConfig.MinigameTimerLength_Default;
            }
        }

        private static void EnsureTimerLengthsConfigLoaded() {
            if (lengthsConfig == null) lengthsConfig = Resources.Load<TimerLengthsSO>(ConfigPath);
        }
    }
}