using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "MinigameConfigSO", menuName = "Scriptable Objects/Minigame Configuration")]
public class MinigameConfigSO : ScriptableObject
{
    [Serializable]
    public class SceneTutorialEntry {
        public string SceneName;
        [TextArea(2, 5)]
        public string TutorialText;
    }
    
    [Serializable]
    public class MinigameSceneMapping {
        public MinigameType Type;
        [Tooltip("List of scene names available for minigame type")]
        public List<string> SceneNames;
    }
    
    [SerializeField]
    private List<MinigameSceneMapping> configurations = new List<MinigameSceneMapping>();
    [SerializeField]
    private List<SceneTutorialEntry> sceneTutorials = new List<SceneTutorialEntry>();

    public string GetRandomSceneName(MinigameType type) {
        var minigameMapping = configurations.Find(c => c.Type == type);
        if (minigameMapping == null || minigameMapping.SceneNames.Count == 0) {
            Debug.LogError($"MinigameConfigSO: No scene names were found for minigame type {type}.");
            return null;
        }
        return minigameMapping.SceneNames[Random.Range(0, minigameMapping.SceneNames.Count)];
    }

    public string GetTutorialText(string sceneName) {
        var entry = sceneTutorials.Find(s => s.SceneName == sceneName);
        return entry?.TutorialText;
    }

    public MinigameType GetTypeForMinigame(string sceneName) {
        MinigameType type = MinigameType.Unknown;
        foreach (var minigameSceneMapping in configurations) {
            if (minigameSceneMapping.SceneNames.Contains(sceneName) &&
                minigameSceneMapping.Type != MinigameType.Final) {
                return minigameSceneMapping.Type;
            }
        }

        return type;
    }
}
