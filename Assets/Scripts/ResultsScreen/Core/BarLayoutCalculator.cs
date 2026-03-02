namespace ResultsScreen.Core {
    public static class BarLayoutCalculator {
        public static float[] ComputeBarHeights(PlacesEntry[] entries, float maxBarHeight, float minBarHeight) {
            float[] barHeights = new float[entries.Length];
            int leaderFunds = entries[0].Funds;
            if (leaderFunds == 0) {
                // assuming the game hasn't changed to allow negatives, everyone is tied
                for (int i = 0; i < entries.Length; i++) {
                    barHeights[i] = maxBarHeight;
                }
                return barHeights;
            }
            for(int i = 0; i<entries.Length; i++) {
                if (entries[i].Funds == leaderFunds) {
                    barHeights[i] = maxBarHeight;
                } else {
                    barHeights[i] =
                        minBarHeight + ((float)entries[i].Funds / leaderFunds) * (maxBarHeight - minBarHeight);
                }
            }
            return barHeights;
        }
    }
}