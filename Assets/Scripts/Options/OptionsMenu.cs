using System.Collections.Generic;
using Options;
using UnityEngine;
using UnityEngine.UIElements;

public class OptionsMenu : MonoBehaviour
  {
      [SerializeField] private UIDocument optionsDocument;

      private VisualElement root;
      private Button okayButton;
      
      private DisplayTabHandler displayTab;
      private List<IOptionsTab> tabs;
      
      private void Awake()
      {
          root = optionsDocument.rootVisualElement;
          root.style.display = DisplayStyle.None;
          GameSettings.Load();
          GameSettings.Apply();
      }

      private void Start()
      {
          okayButton = root.Q<Button>("Okay_Button");

          displayTab = new DisplayTabHandler();
          tabs = new List<IOptionsTab>
          {
              displayTab,
              new SoundTabHandler(),
              new MiscTabHandler(),
              new AccessibilityTabHandler(),
              new GameplayTabHandler()
          };

          foreach (var tab in tabs) {
              tab.Initialize(root);
              tab.SyncToSettings();
              tab.RegisterCallbacks();
          }
          
          okayButton.clicked += OnOkay;
      }
      
      private void OnOkay()
      {
          GameSettings.Save();
          GameSettings.Apply(displayTab.DisplayOptionChanged);
          root.style.display = DisplayStyle.None;
      }

      public void Show()
      {
          foreach (var tab in tabs) {
              tab.SyncToSettings();
          }
          root.style.display = DisplayStyle.Flex;
      }

      private void OnDestroy()
      {
          if (okayButton != null) okayButton.clicked -= OnOkay;
      }

      public void Hide() {
          root.style.display = DisplayStyle.None;
      }
  }
