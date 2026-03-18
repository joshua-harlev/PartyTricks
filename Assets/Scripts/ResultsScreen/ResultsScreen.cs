using System;
using System.Collections;
using System.Collections.Generic;
using ResultsScreen.Core;
using Services;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using FMODUnity;
using Image = UnityEngine.UI.Image;

namespace ResultsScreen {
    public class ResultsScreen : MonoBehaviour {
        [SerializeField] private GameObject SuspensePanel;
        [SerializeField] private TMP_Text WinnerLabel;
        [SerializeField] private TMP_Text WinnerNumberLabel;
        [SerializeField] private Image BackgroundImage;
        [SerializeField] private PlacesScreenPanel PlacesScreenPanel;
        [SerializeField] private EventReference returnSound;
        private int playerWinnerIndex;
        private int[] playerFunds;
        private Button mainMenuButton;
        private IPlayerService playerService;
        private bool canReturnToMainMenu = false;
        private bool isTie;
        private readonly List<InputAction> subscribedActions = new();

        private void Awake() {
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        }

        public void ReturnToMainMenu() {
            if (canReturnToMainMenu) {
                RuntimeManager.PlayOneShot(returnSound);
                ResetProfiles();
                SceneManager.LoadScene("MainMenu");
            }

            canReturnToMainMenu = false;
        }

        private void ResetProfiles() {
            PlayerSlot[] playerSlots = playerService.PlayerSlots as PlayerSlot[];
            foreach (var playerSlot in playerSlots) {
                playerSlot.Profile.Reset();
            }
        }

        private void Start() {
            playerFunds = new int[4] { 0, 0, 0, 0 };
            WinnerLabel.text = "The winner is";
            GetWinner();
            StartCoroutine(WaitAndDisplayWinner());
        }

        private void GetWinner() {
            for (int i = 0; i < 4; i++) {
                playerFunds[i] = playerService.GetPlayerProfile(i).Wallet.GetCurrentFunds();
            }
            
            PlacesEntry[] entries = PlacesCalculator.CalculatePlaces(playerFunds);
            isTie = (entries[1].Rank == 0);

            if (!isTie) {
                playerWinnerIndex = entries[0].PlayerIndex;
            }
        }

        private IEnumerator WaitAndDisplayWinner() {
            yield return new WaitForSeconds(0.5f);
            WinnerLabel.text = "The winner is.";
            yield return new WaitForSeconds(1.3f);
            WinnerLabel.text = "The winner is..";
            yield return new WaitForSeconds(1.5f);
            WinnerLabel.text = "The winner is...";
            yield return new WaitForSeconds(2.5f);
            DisplayWinner();
            yield return new WaitForSeconds(3.5f);
            HideSuspensePanel();
            PlacesScreenPanel.ShowStatic(playerFunds);
            canReturnToMainMenu = true;
            SubscribeToPlayerSubmitActions();
        }

        private void SubscribeToPlayerSubmitActions() {
            foreach (var playerSlot in playerService.PlayerSlots) {
                PlayerInput playerInput = playerSlot.PlayerInput;
                if (playerInput == null) continue;
                InputAction action = playerInput.actions.FindAction("UI/Submit");
                if (action == null) continue;
                action.performed += OnSubmitPerformed;
                subscribedActions.Add(action);
            }
        }

        private void OnSubmitPerformed(InputAction.CallbackContext context) {
            ReturnToMainMenu();
        }

        private void OnDestroy() {
            foreach (var action in subscribedActions) {
                action.performed -= OnSubmitPerformed;
            }
        }

        private void HideSuspensePanel() {
            BackgroundImage.color = Color.white;
            SuspensePanel.SetActive(false);
        }

        private void DisplayWinner() {
            if (isTie) {
                WinnerNumberLabel.text = "tied?";
                BackgroundImage.color = Color.black;
            } else {
                WinnerNumberLabel.text = "Player " + (playerWinnerIndex + 1) + "!";
                Color backgroundColor = playerWinnerIndex switch
                {
                    0 => new Color(153 / 256f, 0, 0, 1),
                    1 => new Color(66 / 256f, 99 / 256f, 217 / 256f, 1),
                    2 => new Color(140 / 256f, 25 / 256f, 212 / 256f, 1),
                    3 => new Color(10 / 256f, 121 / 256f, 41 / 256f, 1),
                    _ => default
                };
            BackgroundImage.color = backgroundColor;
            }
            WinnerNumberLabel.color = Color.white;
            WinnerLabel.color = Color.white;
        }
    }
}