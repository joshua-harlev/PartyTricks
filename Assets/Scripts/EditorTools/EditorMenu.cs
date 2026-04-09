using UnityEditor;
using UnityEditor.SceneManagement;

namespace EditorTools {
    public static class EditorMenu {
        private const string MainMenuScenePath = "Assets/Scenes/MainMenu.unity";

        [MenuItem("Party Tricks/Play From Main Menu %#m")] // (ctrl/cmd+shift+m)
        private static void PlayFromMainMenu() {
            if (EditorApplication.isPlaying) {
                EditorApplication.isPlaying = false;
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);
            EditorApplication.isPlaying = true;
        }
        
        [MenuItem("Party Tricks/Play From Main Menu %#m", true)]
        private static bool PlayFromMainMenuValidate() => !EditorApplication.isPlaying;
    }
}