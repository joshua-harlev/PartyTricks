using System;
using System.Collections;
using TitleScreen;
using UnityEngine;
using UnityEngine.UIElements;

public class SplashScreen : MonoBehaviour, ITitleScreenPhase {
    [SerializeField] private UIDocument SplashScreenDocument;
    [SerializeField] private float SecondsBetweenLogos;
    
    private VisualElement fmodParent;
    private VisualElement gameLogoParent;
    
    public event Action OnPhaseComplete;

    private void Awake() {
        fmodParent = SplashScreenDocument.rootVisualElement.Q("FMODPanel");
        gameLogoParent = SplashScreenDocument.rootVisualElement.Q("GameLogoPanel");
    }

    private void OnEnable() {
        StartCoroutine(Play());
    }

    private IEnumerator Play() {
        HideGameLogo();
        ShowFMOD();
        yield return new WaitForSeconds(SecondsBetweenLogos);
        HideFMOD();
        ShowGameLogo();
        yield return new WaitForSeconds(SecondsBetweenLogos);
        OnPhaseComplete?.Invoke();
    }

    private void ShowFMOD() {
        fmodParent.style.display = DisplayStyle.Flex;
    }
    
    private void ShowGameLogo() {
        gameLogoParent.style.display = DisplayStyle.Flex;
    }
    
    private void HideFMOD() {
        fmodParent.style.display = DisplayStyle.None;
    }
    
    private void HideGameLogo() {
        gameLogoParent.style.display = DisplayStyle.None;
    }
}
