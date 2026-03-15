using UnityEngine;

namespace Minigames.DireDodging {
    public struct DireDodgingShockwaveData {
        public float CooldownDurationInSeconds;
        public float WarningDurationInSeconds;
        public float StunDurationInSeconds;
        public float HoldDurationInSeconds;
        public float Timer;
        public DireDodgingShockwave.ShockwaveState State;

        public static DireDodgingShockwaveData Create(DireDodgingShockwaveConfigSO config, int stackCount = 1) {
            float cooldownReduction = config.CooldownReductionPerStackInSeconds * (stackCount - 1);
            float cooldownTime = Mathf.Max(config.MinCooldownInSeconds, config.BaseCooldownInSeconds - cooldownReduction);
            float stunIncreaseTime = config.StunDurationIncreasePerStackInSeconds * (stackCount - 1);
            return new DireDodgingShockwaveData
            {
                CooldownDurationInSeconds = cooldownTime,
                WarningDurationInSeconds = config.WarningDurationInSeconds,
                StunDurationInSeconds = config.BaseStunDurationInSeconds + stunIncreaseTime,
                HoldDurationInSeconds = config.HoldDurationInSeconds,
                Timer = cooldownTime,
                State = DireDodgingShockwave.ShockwaveState.Charging,
            };
        }
    }
}