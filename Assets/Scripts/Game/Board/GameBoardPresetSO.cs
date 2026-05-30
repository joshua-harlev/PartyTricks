using System;
using System.Collections.Generic;
using Minigames;
using UnityEngine;

namespace Game.Board {
    [CreateAssetMenu(fileName = "GameBoardPresetSO", menuName = "Scriptable Objects/Game Board Preset")]
    public class GameBoardPresetSO : ScriptableObject
    {
        [Serializable]
        public class RoundEntry {
            [SerializeField] private MinigameType minigameType;
            [SerializeField] private bool isDouble;
            [Tooltip("Optional. Leave blank for a random scene.")]
            [SerializeField] private string sceneName;
        
        
            public MinigameType MinigameType => minigameType;
            public bool IsDouble => isDouble;
            public string SceneName => sceneName;

            public RoundEntry(MinigameType minigameType, bool isDouble, string sceneName = null) {
                this.minigameType = minigameType;
                this.isDouble = isDouble;
                this.sceneName = sceneName;
            }
        }
    
        [SerializeField] private List<RoundEntry> roundList;
        public List<RoundEntry> RoundList => roundList;
    }
}
