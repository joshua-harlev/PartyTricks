using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Services;
using TitleScreen;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

namespace Input.ControllerConnection {
    public class ControllerConnectionSystem : MonoBehaviour, ITitleScreenPhase {
        public event Action OnPhaseComplete;
        
        private static readonly Color[] ColorTemplateList =
        {
            Color.red,
            Color.orange,
            Color.darkBlue,
            Color.darkMagenta,
            Color.seaGreen,
            Color.paleVioletRed,
            Color.powderBlue
        };

        private List<Color> randomColorList;
        private List<PlayerSelector> playerSelectors;
        private IPlayerService playerService;
        private IPauseService pauseService;
        private bool phaseComplete;
        private bool countdownIsActive;
        
        [SerializeField] private GameObject PlayerSelectorPrefab;
        [SerializeField] private ReadyZone ReadyZone;
        [SerializeField] private Player[] PlayerBodies;
        [SerializeField] private CountdownTimer CountdownTimer;
        [SerializeField] private TMP_Text CountdownLabel;
        [SerializeField] private int ReadyCountdownLengthInSeconds = 5;
        private Player[] bodiesBySlot;

        private void Awake() {
            playerSelectors = new List<PlayerSelector>();
            playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
            pauseService = ServiceLocatorAccessor.GetService<IPauseService>();
            ShuffleColors();
            bodiesBySlot = new Player[playerService.PlayerSlots.Count];
            foreach (Player player in PlayerBodies) {
                bodiesBySlot[player.SlotIndex] = player;
            }
        }

        private void OnEnable() {
            pauseService.DisablePause();
            ReleaseAllHumanSeats();
            playerService.OnInputConnected += SpawnSelectorFor;
            foreach (var input in playerService.UnassignedInputs.ToArray()) {
                SpawnSelectorFor(input);
            }

            CountdownTimer.OnTick += OnCountdownTimerTick;
            CountdownTimer.OnTimerEnd += OnCountdownTimerEnd;
            CountdownTimer.OnReset += OnCountdownTimerReset;
            phaseComplete = false;
            countdownIsActive = false;
        }

        private void OnCountdownTimerTick(int seconds) {
            CountdownLabel.text = seconds.ToString();
        }

        private void OnCountdownTimerEnd() {
            if (phaseComplete) return;
            phaseComplete = true;
            OnPhaseComplete?.Invoke();
        }

        private void OnCountdownTimerReset() {
            CountdownLabel.text = "-";
        }

        private void Update() {
            if (phaseComplete) return;
            bool ready = EnoughPlayersAreReady();
            if (ready == countdownIsActive) return;

            countdownIsActive = ready;
            if (ready) {
                CountdownLabel.gameObject.SetActive(true);
                CountdownTimer.StartTimer(ReadyCountdownLengthInSeconds);
            }
            else {
                CountdownTimer.Reset();
            }
        }

        private void ReleaseAllHumanSeats() {
            for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
                if (!playerService.PlayerSlots[i].IsAI) playerService.TryReleaseSlot(i);
            }
        }

        private void SpawnSelectorFor(PlayerInput input) {
            Color pointerColor = GetRandomUnusedColor();
            GameObject playerSelectorGameObject = Instantiate(PlayerSelectorPrefab);
            PlayerSelector playerSelector = playerSelectorGameObject.GetComponent<PlayerSelector>();
            playerSelectors.Add(playerSelector);
            playerSelector.Initialize(pointerColor, input, input.GetComponent<IDirectionalTwoButtonInputHandler>(), playerService);
        }

        private void ShuffleColors() {
            randomColorList = ColorTemplateList.OrderBy(_ => Random.value).ToList();
        }

        private Color GetRandomUnusedColor() {
            if (randomColorList.Count <= 0) {
                Debug.LogError("Ran out of random colors for the controller connection system :(");
                return Color.black;
            }
            
            Color selectedColor = randomColorList[0];
            randomColorList.RemoveAt(0);
            
            return selectedColor;
        }

        private bool EnoughPlayersAreReady() {
            int numberSeated = 0;
            for (int i = 0; i < playerService.PlayerSlots.Count; i++) {
                if (playerService.PlayerSlots[i].IsAI) continue;
                numberSeated++;
                Player player = bodiesBySlot[i];
                if (player == null) return false;
                if(!ReadyZone.Contains(player.transform.position)) return false;
            }

            return numberSeated > 0;
        }

        private void OnDisable() {
            playerService.OnInputConnected -= SpawnSelectorFor;
            CountdownTimer.OnTick -= OnCountdownTimerTick;
            CountdownTimer.OnTimerEnd -= OnCountdownTimerEnd;
            CountdownTimer.OnReset -= OnCountdownTimerReset;
            
            foreach (var selector in playerSelectors) {
                if (selector == null) continue;
                selector.Disable();
                Destroy(selector.gameObject);
            }
            playerSelectors.Clear();
            
            playerService.DestroyUnassignedInputs();
            pauseService.EnablePause();
            ShuffleColors();
        }
    }
}
