using System;

namespace TitleScreen {
    public interface ITitleScreenPhase {
        event Action OnPhaseComplete;
    }
}