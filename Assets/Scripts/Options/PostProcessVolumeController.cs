using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PostProcessVolumeController : MonoBehaviour {
    [SerializeField] 
    private Volume volume;

    private Bloom bloom;
    private void Awake() {
        volume.profile.TryGet<Bloom>(out bloom);
        GameSettings.OnApplySettings += UpdateAll;
        UpdateAll();
    }

    private void UpdateAll() {
        UpdateBloom();
    }

    private void UpdateBloom() {
        if (bloom == null) return;
        bloom.active = GameSettings.Display.Bloom;
    }

    private void OnDestroy() {
        GameSettings.OnApplySettings -= UpdateAll;
    }
}
