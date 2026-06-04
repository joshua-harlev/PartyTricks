using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Minigames {
    public class Bets : MonoBehaviour {
        [SerializeField] 
        private BetCard[] betCards;
        private BetPlayerManager playerManager;
        public int AllowedBettingDurationInSeconds = 10;
        [SerializeField] 
        private MinigameTimer MinigameTimer;
        [FormerlySerializedAs("playerCornerDisplays")] [SerializeField]
        private PlayerCornerDisplay[] PlayerCornerDisplays;
        private IGamblingMinigame gamblingMinigameManager;

        private void Awake() {
            gamblingMinigameManager = GetComponent<IGamblingMinigame>();
            if (gamblingMinigameManager == null) {
                UnityEngine.Debug.LogError("Error: IGamblingMinigame not found on Bets GameObject");
            }

            if (MinigameTimer == null) {
                UnityEngine.Debug.LogError("Error: MinigameTimer not found on Bets GameObject");
            }
            MinigameTimer.Initialize(AllowedBettingDurationInSeconds);
        }

        private void Start() {
            if (gamblingMinigameManager == null) return;
            InitializeComponents();
            StartBets();
        }

        private void InitializeComponents() {
            playerManager = new BetPlayerManager(betCards, PlayerCornerDisplays);
            MinigameTimer.OnTimerEnd += OnBetTimerEnd;
        }

        private void StartBets() {
            playerManager.InitializePlayers();
            MinigameTimer.StartTimer();
        }

        private void OnBetTimerEnd() {
            UnityEngine.Debug.Log("Timer over!");
            playerManager.LockAllSelectors();

            Dictionary<int, int> bets = playerManager.GetPlayerBets();

            StartCoroutine(WaitAndContinue(bets));
        }

        private IEnumerator WaitAndContinue(Dictionary<int, int> bets) {
            yield return new WaitForSeconds(3);
            if (gamblingMinigameManager.OnBetTimerEnd != null) {
                gamblingMinigameManager.OnBetTimerEnd.Invoke(bets);
            }
            else {
                UnityEngine.Debug.LogError("OnBetTimerEnd event is not set up correctly.");
            }
        }

        public void Reset() {
            MinigameTimer.StopIfRunning();
            MinigameTimer.StartTimer();
            playerManager.EnableAllSelectors();
        }
    
        public void UnlockAISelectors() {
            playerManager.UnlockAISelectors();
        }
    

        private void OnDestroy() {
            if (MinigameTimer != null) {
                MinigameTimer.OnTimerEnd -= OnBetTimerEnd;
            }
            playerManager?.Cleanup();
        }
    }
}
