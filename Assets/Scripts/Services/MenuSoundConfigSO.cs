using FMODUnity;
using UnityEngine;

[CreateAssetMenu(fileName = "MenuSoundConfigSO", menuName = "Scriptable Objects/MenuSoundConfigSO")]
public class MenuSoundConfigSO : ScriptableObject {
    public EventReference HighlightSound;
    public EventReference SelectSound;
    public EventReference CancelSound;
    public EventReference ToggleOnSound;
    public EventReference ToggleOffSound;
    public EventReference SliderDragSound;
}
