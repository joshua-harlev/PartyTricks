using DG.Tweening;
using Services;
using UnityEngine;

// TODO: Refactor this, as players can respawn now, original intensity logic obsolete
public class DireDodgingGameplayState : IDireDodgingState {
    private int[] playerPlaces;
    private int[] playerKills;
    private MinigameTimer timer;
    private PlayerCornerDisplay[] playerCornerDisplays;
    private Camera gameCamera;
    // private bool gameShouldEnd => (numberOfAlivePlayers <= 1); Removed so game lasts the amount of seconds
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

    public void HandlePlayerDeath(int playerIndex) {
        UpdateEliminations(playerIndex);
        gameCamera.DOShakePosition(duration: 0.1f, strength: 0.4f, vibrato: 1, randomness: 90f, fadeOut: false).SetUpdate(true);
    }

    private void UpdateEliminations(int playerIndex) {
        playerCornerDisplays[playerIndex].UpdateEliminations(playerKills[playerIndex]);
    }
    

    private void OnGameplayEnd() {
        timer.OnTimerEnd -= OnGameplayEnd;
        timer.StopIfRunning();
    
        // Calculate final places based on kills
        playerPlaces = CalculatePlacesByKills(playerKills);
    
        UpdateAllDisplays();
        DireDodgingMinigameManager.Instance.FreezeAllPlayers();
        DireDodgingMinigameManager.Instance.ReturnAllProjectiles();
        pauseService.DoTimedPause(1f, () =>
        {
            DireDodgingMinigameManager.Instance.TransitionToResults(playerPlaces, playerKills); 
        });
    }

    private int[] CalculatePlacesByKills(int[] kills) {
        int[] places = new int[4];
    
        // Create array of (playerIndex, kills) pairs
        var rankData = new (int index, int kills)[4];
        for (int i = 0; i < 4; i++) {
            rankData[i] = (i, kills[i]);
        }
    
        // Sort by kills descending
        System.Array.Sort(rankData, (a, b) => b.kills.CompareTo(a.kills));
    
        // Assign places
        for (int i = 0; i < 4; i++) {
            places[rankData[i].index] = i + 1; // 1st, 2nd, 3rd, 4th
        }
    
        return places;
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