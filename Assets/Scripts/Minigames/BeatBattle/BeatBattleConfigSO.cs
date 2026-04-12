using Minigames.BeatBattle.Core;
using UnityEngine;

[CreateAssetMenu(fileName = "Beat Battle Config", menuName = "Scriptable Objects/Beat Battle Config")]
public class BeatBattleConfigSO : ScriptableObject {
    [Header("Grid")] 
    [SerializeField] private float bpm = 120f;
    [SerializeField] private int gridSubdivision = 2;

    [Header("Gameplay")] 
    [SerializeField] private int maxNotesPerTurn = 5;
    [SerializeField] private float hitWindowInMs = 100f;
    [SerializeField] private float creationPhaseLengthInSeconds = 3f;
    [SerializeField] private float playbackPhaseLengthInSeconds = 3.5f;
    [SerializeField] private float transitionLengthInSeconds = 0.5f;

    public float CreationDurationInSeconds => creationPhaseLengthInSeconds;
    public float PlaybackDurationInSeconds => playbackPhaseLengthInSeconds;
    public float TransitionDurationInSeconds => transitionLengthInSeconds;
    
    public BeatBattleConfig GetConfig() {
        return new BeatBattleConfig(bpm, gridSubdivision, maxNotesPerTurn, hitWindowInMs);
    }
}
