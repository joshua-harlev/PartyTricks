using System.Collections.Generic;
using FMODUnity;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace Services {
    public class MenuSoundService : MonoBehaviour, IMenuSoundService {
        private MenuSoundConfigSO config;
        private readonly HashSet<IPanel> registeredPanels = new();

        // Prefab/GO names
        private static readonly HashSet<string> ExcludedDocumentNames = new()
        {
            "GameBoardDisplay"
        };
        
        public void Initialize(MenuSoundConfigSO soundConfig) {
            config = soundConfig;
            SceneManager.sceneLoaded += OnSceneLoaded;
            RegisterAllDocumentsInScene();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
            RegisterAllDocumentsInScene();
        }

        private void RegisterAllDocumentsInScene() {
            foreach (var document in FindObjectsByType<UIDocument>(FindObjectsSortMode.None)) {
                if (ExcludedDocumentNames.Contains(document.gameObject.name)) continue;
                RegisterUIDocument(document);
            }
        }

        private void RegisterUIDocument(UIDocument document) {
            var root = document.rootVisualElement;
            if (root?.panel == null) return;

            var panel = root.panel;
            if (!registeredPanels.Add(panel)) return;

            var visualTree = panel.visualTree;
            visualTree.RegisterCallback<FocusInEvent>(OnFocusIn, TrickleDown.TrickleDown);
            visualTree.RegisterCallback<PointerOverEvent>(OnPointerOver, TrickleDown.TrickleDown);
            visualTree.RegisterCallback<ClickEvent>(OnClick, TrickleDown.TrickleDown);
            visualTree.RegisterCallback<NavigationCancelEvent>(OnCancel, TrickleDown.TrickleDown);
            visualTree.RegisterCallback<ChangeEvent<bool>>(OnToggleChanged, TrickleDown.TrickleDown);
            visualTree.RegisterCallback<ChangeEvent<float>>(OnSliderChanged, TrickleDown.TrickleDown);
        }

        private void OnFocusIn(FocusInEvent focusEvent) {
            if (focusEvent.target is Button or DropdownField) {
                PlaySound(config.HighlightSound);
            } else if (focusEvent.target is VisualElement visualElement && visualElement.ClassListContains("unity-base-dropdown__item")) {
                PlaySound(config.HighlightSound);
            }
        }
        
        private void OnPointerOver(PointerOverEvent pointerEvent) {
            if (pointerEvent.target is Button or DropdownField) {
                PlaySound(config.HighlightSound);
            } else if (pointerEvent.target is VisualElement visualElement && visualElement.ClassListContains("unity-base-dropdown__item")) {
                PlaySound(config.HighlightSound);
            }
        }

        private void OnClick(ClickEvent clickEvent) {
            if (clickEvent.target is Button) {
                PlaySound(config.SelectSound);
            }
        }
        
        private void OnCancel(NavigationCancelEvent navigationCancelEvent) {
            PlaySound(config.CancelSound);
        }

        private void OnToggleChanged(ChangeEvent<bool> changeEvent) {
            if (changeEvent.target is not Toggle) return;
            PlaySound(changeEvent.newValue ? config.ToggleOnSound : config.ToggleOffSound);
        }

        private void OnSliderChanged(ChangeEvent<float> changeEvent) {
            if (changeEvent.target is not Slider) return;
            PlaySound(config.SliderDragSound);
        }
        
        private void PlaySound(EventReference sound) {
            if (!sound.IsNull) {
                RuntimeManager.PlayOneShot(sound);
            } else {
                DebugLogger.Log(LogChannel.Audio, "Sound clip was null in MenuSoundService.", LogLevel.Warning);
            }
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}