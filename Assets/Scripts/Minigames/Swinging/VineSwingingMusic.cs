using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Minigames.Swinging {
    public class VineSwingingMusic : MonoBehaviour {
        [SerializeField] private EventReference musicEvent;
        private EventInstance musicInstance;

        private void Awake() {
            musicInstance = RuntimeManager.CreateInstance(musicEvent);
        }
        
        public void Play() => musicInstance.start();
        
        private void OnDisable() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
        }
        
        private void OnDestroy() {
            musicInstance.stop(STOP_MODE.IMMEDIATE);
            musicInstance.release();
        }
    }
}