namespace ResultsScreen.Core {
    public class PlacesAnimationState {
        public bool HasPreviousState { get; private set; }
        public PlacesEntry[] PreviousEntries { get; private set; }
        public float[] PreviousHeights { get; private set; }

        public void Record(PlacesEntry[] entries, float[] heights) {
            PreviousEntries = entries;
            PreviousHeights = heights;
            HasPreviousState = true;
        }

        public void Clear() {
            PreviousEntries = null;
            PreviousHeights = null;
            HasPreviousState = false;
        }
    }
}