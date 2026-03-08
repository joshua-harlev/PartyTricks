using UnityEngine;

[CreateAssetMenu(fileName = "VineSwingingAIConfig", menuName = "Scriptable Objects/Vine Swinging AI Config")]
[System.Serializable]
public class VineSwingingAIConfigSO : ScriptableObject {
    [Range(0, 1)]
    public double MissChance = 0.2;
    [Range(0, 1)]
    public double FallChance = 0.3;
    [Range(0, 1)]
    public float MaxPhaseOffset = 0.6f;
    [Range(0, 1)]
    public float MinReleaseThreshold = 0.3f;
    [Range(0, 1)]
    public float ReleaseThresholdRange = 0.3f;
    [Range(0, 1)]
    public float MinPhaseOffset = 0.2f;
}