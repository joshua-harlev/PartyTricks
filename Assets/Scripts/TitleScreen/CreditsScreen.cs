using UnityEngine;
using UnityEngine.UIElements;

namespace TitleScreen {
    public class CreditsScreen : MonoBehaviour {
        [SerializeField] private UIDocument creditsDocument;
        [SerializeField] private CreditsScreenData creditsData;
        
        private VisualElement root;
        private Button okayButton;

        public void Awake() {
            root = creditsDocument.rootVisualElement;
            root.style.display = DisplayStyle.None;
            
            SetUpCreditsList();
            Hide();
        }

        private void Start() {
            okayButton = root.Q<Button>("Okay_Button");
            okayButton.clicked += Hide;
        }

        private void SetUpCreditsList() {
            var listView = root.Q<MultiColumnListView>("CreditsList");
            listView.itemsSource = creditsData.CreditsEntries;

            var roleColumn = listView.columns["role"];
            var namesColumn = listView.columns["names"];

            roleColumn.makeCell = () => new Label
            {
                style =
                {
                    height = Length.Percent(100),
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.MiddleCenter,
                    paddingTop = 3,
                    paddingBottom = 3,
                }
            };
            namesColumn.makeCell = () => new Label
            {
                style =
                {
                    height = Length.Percent(100),
                    whiteSpace = WhiteSpace.Normal,
                    unityTextAlign = TextAnchor.MiddleLeft,
                    paddingTop = 3,
                    paddingBottom = 3,
                }
            };

            roleColumn.bindCell = (element, rowIndex) =>
            {
                ((Label)element).text = creditsData.CreditsEntries[rowIndex].Role;
            };

            namesColumn.bindCell = (element, rowIndex) =>
            {
                var names = creditsData.CreditsEntries[rowIndex].Names;
                ((Label)element).text = names != null ? string.Join(", ", names) : string.Empty;
            };
        }

        public void Hide() {
            root.style.display = DisplayStyle.None;
        }
        
        public void Show() {
            root.style.display = DisplayStyle.Flex;
        }

        private void OnDestroy() {
            if (okayButton != null) okayButton.clicked -= Hide;
        }
    }
}