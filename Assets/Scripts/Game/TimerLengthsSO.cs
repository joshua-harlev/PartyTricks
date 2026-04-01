using UnityEngine;

[CreateAssetMenu(fileName = "Timer Lengths Config", menuName = "Scriptable Objects/Timer Lengths Config")]
public class TimerLengthsSO : ScriptableObject {
    [Header("Shop Timer Lengths")]
    public float ShopTimerLength_LessTime = 20;
    public float ShopTimerLength_Default = 30;
    public float ShopTimerLength_MoreTime = 45;
    
    [Header("Minigame Timer Lengths")]
    public float MinigameTimerLength_LessTime = 25;
    public float MinigameTimerLength_Default = 30;
    public float MinigameTimerLength_MoreTime = 40;
    
    [Header("Countdown Lengths")]
    public int CountdownTimerLength_Default = 3;
    public int CountdownTimerLength_MoreTime = 10;
}
