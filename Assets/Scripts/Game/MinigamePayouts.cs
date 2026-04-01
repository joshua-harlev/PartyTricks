using UnityEngine;

namespace Game {
    public static class MinigamePayouts {
        private static MinigamePayoutConfigSO payoutConfig;
        private static string ConfigPath = "Config/Minigame Payouts";

        public static int[] GetBaseFundsPerRank() {
            EnsurePayoutConfigLoaded();
            return payoutConfig.BaseFundsPerRank;
        }
        
        private static void EnsurePayoutConfigLoaded() {
            if (payoutConfig == null) {
                payoutConfig = Resources.Load<MinigamePayoutConfigSO>(ConfigPath);
            }
        }
    }
}