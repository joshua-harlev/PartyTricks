using System;
using UnityEngine;

namespace Options {
    public static partial class GameSettings {
        public static event Action OnApplySettings;

        public static void Load() {
            Display.Load();
            Sound.Load();
            Gameplay.Load();
            Accessibility.Load();
            Misc.Load();
        }

        public static void Save() {
            Display.Save();
            Sound.Save();
            Gameplay.Save();
            Accessibility.Save();
            Misc.Save();
            PlayerPrefs.Save();
        }

        public static void Apply(bool applyDisplay = true) {
            Display.Apply(applyDisplay);
            Sound.Apply();
            Gameplay.Apply();
            Accessibility.Apply();
            Misc.Apply();
            OnApplySettings?.Invoke();
        }
    }
}

