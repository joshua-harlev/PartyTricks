namespace Services {
    public interface ITinnitusFilterService {
        void SetEnabled(bool enabled);
        void SetFrequency(float frequency);
        void SetGain(float gainInDecibels);
        void PlayTestTone();
        void StopTestTone();
        bool IsPlayingTestTone { get; }
    }
}