using DG.Tweening;
using Services;
using UnityEngine;

public class DireDodgingGameplayState : IDireDodgingState {
    private int[] playerPlaces;
    private int[] playerKills;
    private MinigameTimer timer;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private Camera gameCamera;
    private IPauseService pauseService;
    
    public DireDodgingGameplayState(MinigameTimer timer, PlayerCornerDisplay[] playerCornerDisplays, Camera camera) {
        this.timer = timer;
        gameCamera = camera;
        timer.OnTimerEnd += OnGameplayEnd;
        timer.OnHalfwayPointReached += OnHalfwayPointReached;
        this.playerCornerDisplays = playerCornerDisplays;
        pauseService = ServiceLocatorAccessor.GetService<IPauseService>();
    }

    private void OnHalfwayPointReached(int remainingTimeInSeconds) {
        timer.OnHalfwayPointReached -= OnHalfwayPointReached;
        DireDodgingMinigameManager.Instance.StartIncreasingIntensity(remainingTimeInSeconds);
        DireDodgingMinigameManager.Instance.SetMusicIntensity(2);
    }
    

    public void Enter() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Entered Gameplay State.", LogLevel.Verbose);
        DireDodgingMinigameManager.Instance.EnableAllPlayerInput();
        DireDodgingMinigameManager.Instance.StartPlayerShooting();
        DireDodgingMinigameManager.Instance.SetMusicIntensity(1);
        timer.StartTimer();
        playerPlaces = new[] { 1, 1, 1, 1 };
        playerKills = new[] { 0, 0, 0, 0 };
    }

    public void OnUpdate() { }

    public void HandlePlayerKill(int playerIndex) {
        playerKills[playerIndex]++;
        playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex]);
    }

    public void OnPlayerDeath(int playerIndex) {
        UpdateEliminations(playerIndex);
        gameCamera.DOShakePosition(duration: 0.1f, strength: 0.4f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
    }

    private void UpdateEliminations(int playerIndex) {
        playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex]);
    }
    

    private void OnGameplayEnd() {
        DeactivateGameplayTimer();
        playerPlaces = CalculatePlacesByKills(playerKills);
        UpdateAllDisplays();
        DireDodgingMinigameManager.Instance.FreezeAllPlayers();
        DireDodgingMinigameManager.Instance.ReturnAllProjectiles();
        TransitionToResultsAfterDelay();
    }

    private void TransitionToResultsAfterDelay() {
        pauseService.DoTimedPause(1f, () =>
        {
            DireDodgingMinigameManager.Instance.TransitionToResults(playerPlaces, playerKills); 
        });
    }

    private void DeactivateGameplayTimer() {
        timer.OnTimerEnd -= OnGameplayEnd;
        timer.StopIfRunning();
    }

    private int[] CalculatePlacesByKills(int[] kills) {
        int[] places = new int[4];
        var rankData = CreateRankDataArray(kills);
        SortRankDataByKills(rankData);
        AssignPlayerPlaces(places, rankData);
        return places;
    }

    private static void AssignPlayerPlaces(int[] places, (int playerIndex, int playerKills)[] rankData) {
        for (int i = 0; i < 4; i++) {
            places[rankData[i].playerIndex] = i + 1;
        }
    }

    private static void SortRankDataByKills((int playerIndex, int playerKills)[] rankData) {
        System.Array.Sort(rankData, (a, b) => b.playerKills.CompareTo(a.playerKills));
    }

    private static (int playerIndex, int playerKills)[] CreateRankDataArray(int[] kills) {
        var rankData = new (int playerIndex, int playerKills)[4];
        for (int i = 0; i < 4; i++) {
            rankData[i] = (i, kills[i]);
        }

        return rankData;
    }

    private void UpdateAllDisplays() {
        for (int i = 0; i < 4; i++) {
            playerCornerDisplays[i].UpdateEliminations(playerKills[i], playerPlaces[i]);
        }
    }

    public void Exit() {
        DebugLogger.Log(LogChannel.Systems, "Dire Dodging: Exited Gameplay State.", LogLevel.Verbose);
    }
}