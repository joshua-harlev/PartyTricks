using UnityEngine.UIElements;

namespace Options {
    public interface IOptionsTab {
        void Initialize(VisualElement tabRoot);
        void SyncToSettings();
        void RegisterCallbacks();
        void Cleanup();
    }
}