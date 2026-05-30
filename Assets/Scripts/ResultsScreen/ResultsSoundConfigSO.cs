using FMODUnity;
using UnityEngine;

namespace ResultsScreen {
    [CreateAssetMenu(fileName = "ResultsSoundConfigSO", menuName = "Scriptable Objects/ResultsSoundConfigSO")]
    public class ResultsSoundConfigSO : ScriptableObject {
        public EventReference ReturnSound;
        public EventReference ResultsMusic;
        public EventReference GrowSound;
        public EventReference SwapSound;
        public EventReference WhooshSound;
    }
}
