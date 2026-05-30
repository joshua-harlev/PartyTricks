using System.Collections;
using Debug;
using FMOD;
using FMODUnity;
using UnityEngine;

namespace Services {
    public class TinnitusFilterService : MonoBehaviour, ITinnitusFilterService {
        private DSP notchProcessor;
        private DSP oscillatorProcessor;
        private ChannelGroup masterChannelGroup;
        private bool initialized;

        // width of filter cut -> larger values are smaller cuts
        private const float Q_FACTOR = 6.0f;
        
        public bool IsPlayingTestTone { get; private set; }

        private IEnumerator Start() {
            yield return new WaitUntil(() => RuntimeManager.HasBankLoaded("Master"));
            Initialize();
        }

        public void Initialize() {
            FMOD.System coreSystem = RuntimeManager.CoreSystem;

            RESULT result = RuntimeManager.GetBus("bus:/").getChannelGroup(out masterChannelGroup);
            if (result != RESULT.OK) {
                DebugLogger.Log(LogChannel.Audio, $"TinnitusFilterService could not get master channel group: {result}");
                return;
            }

            CreateNotchProcessor(coreSystem);
            AddDSPToHead(notchProcessor);
            notchProcessor.setBypass(!GameSettings.Accessibility.TinnitusFilterEnabled);
            
            CreateOscillatorProcessor(coreSystem);
            AddDSPToHead(oscillatorProcessor);
            oscillatorProcessor.setBypass(true);

            initialized = true;
        }

        private void AddDSPToHead(DSP processor) {
            masterChannelGroup.addDSP(CHANNELCONTROL_DSP_INDEX.HEAD, processor);
        }

        private void CreateOscillatorProcessor(FMOD.System coreSystem) {
            coreSystem.createDSPByType(DSP_TYPE.OSCILLATOR, out oscillatorProcessor);
            oscillatorProcessor.setParameterInt((int)DSP_OSCILLATOR.TYPE, 0); //0 is sine wave
            oscillatorProcessor.setParameterFloat((int)DSP_OSCILLATOR.RATE, GameSettings.Accessibility.TinnitusFilterFrequency);
        }

        private void CreateNotchProcessor(FMOD.System coreSystem) {
            coreSystem.createDSPByType(DSP_TYPE.MULTIBAND_EQ, out notchProcessor);
            notchProcessor.setParameterInt((int)DSP_MULTIBAND_EQ.A_FILTER, (int)DSP_MULTIBAND_EQ_FILTER_TYPE.NOTCH);
            notchProcessor.setParameterFloat((int)DSP_MULTIBAND_EQ.A_FREQUENCY,
                GameSettings.Accessibility.TinnitusFilterFrequency);
            notchProcessor.setParameterFloat((int)DSP_MULTIBAND_EQ.A_Q, Q_FACTOR);
            notchProcessor.setParameterFloat((int)DSP_MULTIBAND_EQ.A_GAIN,
                GameSettings.Accessibility.TinnitusFilterGain);
        }

        public void SetEnabled(bool enabled) {
            if (!initialized) return;
            notchProcessor.setBypass(!enabled);
        }

        public void SetFrequency(float frequencyInHz) {
            if (!initialized) return;
            notchProcessor.setParameterFloat((int)DSP_MULTIBAND_EQ.A_FREQUENCY, frequencyInHz);
            oscillatorProcessor.setParameterFloat((int)DSP_OSCILLATOR.RATE, frequencyInHz);
        }

        public void SetGain(float gainInDecibels) {
            if (!initialized) return;
            notchProcessor.setParameterFloat((int)DSP_MULTIBAND_EQ.A_GAIN, gainInDecibels);
        }

        public void PlayTestTone() {
            if (!initialized) return;
            oscillatorProcessor.setParameterFloat((int)DSP_OSCILLATOR.RATE,
                GameSettings.Accessibility.TinnitusFilterFrequency);
            oscillatorProcessor.setBypass(false);
            IsPlayingTestTone = true;
        }

        public void StopTestTone() {
            if (!initialized) return;
            oscillatorProcessor.setBypass(true);
            IsPlayingTestTone = false;
        }

        private void OnDestroy() {
            if (!initialized) return;
            StopTestTone();
            masterChannelGroup.removeDSP(notchProcessor);
            masterChannelGroup.removeDSP(oscillatorProcessor);
            notchProcessor.release();
            oscillatorProcessor.release();
        }
    }
}