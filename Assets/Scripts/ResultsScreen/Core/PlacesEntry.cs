namespace ResultsScreen.Core {
    public readonly struct PlacesEntry {
        public readonly int PlayerIndex;
        public readonly int Funds;
        public readonly int Rank;

        public PlacesEntry(int playerIndex, int funds, int rank) {
            PlayerIndex = playerIndex;
            Funds = funds;
            Rank = rank;
        }
    }
}