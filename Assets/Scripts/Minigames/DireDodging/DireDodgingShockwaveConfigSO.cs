using FMODUnity;
using UnityEngine;

namespace Minigames.DireDodging {
    [CreateAssetMenu(fileName = "DireDodgingShockwaveConfigSO",
        menuName = "Scriptable Objects/Dire Dodging Shockwave Config")]
    public class DireDodgingShockwaveConfigSO : ScriptableObject {
        [Header("Timing")]
        public float BaseCooldownInSeconds = 8.5f;
        public float CooldownReductionPerStackInSeconds = 1.5f;
        public float MinCooldownInSeconds = 4f;
        public float WarningDurationInSeconds = 3.5f;
        public float HoldDurationInSeconds = 1.0f;

        [Header("Effect")]
        public float BaseStunDurationInSeconds = 1.5f;
        public float StunDurationIncreasePerStackInSeconds = 0.5f;

        [Header("Visual")]
        public float RingExpansionDurationInSeconds = 0.4f;

        [Header("Sound")]
        public EventReference ChargeSound;
        public EventReference FireSound;
    }
}