using System;
using System.Collections.Generic;
using System.Linq;
using Game;
using Services;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour, IGameFlowService {
    [SerializeField] private MinigameConfigSO config;
    [SerializeField] private int initialGameLength = 5;
    [SerializeField] private GameObject gameBoardDisplayPrefab;
    [SerializeField] private GameBoardPresetSO gameBoardPreset;
    [SerializeField] private GameObject tutorialPrefab;
    private HashSet<string> shownTutorialScenes = new();
    private string currentMinigameSceneName;
    private string[] sceneOverrides;
    
    private GameBoardGenerator boardGenerator;
    private GameBoardDisplay currentBoardDisplay;
    private IEconomyService economyService;
    private int currentRoundIndex = -1;
    private List<(MinigameType minigameType, bool IsDouble)> gameBoard;
    private bool GameIsOver => currentRoundIndex >= gameBoard.Count;
    
    private IPlayerService playerService;
    private int[] previousRoundFunds;
    private bool shouldShowPlacesScreen;
    
    public bool HasActiveSession => gameBoard != null && gameBoard.Count > 0;
    
    private void Awake() {
        economyService = ServiceLocatorAccessor.GetService<IEconomyService>();
        playerService = ServiceLocatorAccessor.GetService<IPlayerService>();
        boardGenerator = new GameBoardGenerator(initialGameLength);
    }

    public void StartGame() {
        if (GameSettings.Gameplay.UsePresetBoard && gameBoardPreset != null && gameBoardPreset.RoundList.Count != 0) {
            List<GameBoardPresetSO.RoundEntry> roundList = gameBoardPreset.RoundList;
            var boardToGenerate = new List<(MinigameType minigameType, bool IsDouble)>();
            sceneOverrides = new string[roundList.Count];
            for (int i = 0; i < roundList.Count; i++) {
                boardToGenerate.Add((roundList[i].MinigameType, roundList[i].IsDouble));
                if (!string.IsNullOrWhiteSpace(roundList[i].SceneName)) sceneOverrides[i] = roundList[i].SceneName;
            }
            boardGenerator.GenerateSpecificBoard(boardToGenerate.ToArray());
        } else {
            boardGenerator.GenerateRandomBoard();
            sceneOverrides = null;
        }
        gameBoard = boardGenerator.GameBoard;
        currentRoundIndex = 0;
        shownTutorialScenes.Clear();
        DebugLogger.Log(LogChannel.Systems, $"Game started. Board generated with {gameBoard.Count} rounds.");
        ShowGameBoardDisplay();
    }

    private void ShowGameBoardDisplay() {
        if (gameBoardDisplayPrefab == null) {
            Debug.LogError("No game board display prefab assigned");
            return;
        }
        GameObject displayObject = Instantiate(gameBoardDisplayPrefab);
        currentBoardDisplay = displayObject.GetComponent<GameBoardDisplay>();
        if (currentBoardDisplay != null) {
            currentBoardDisplay.OnContinue += HandleBoardDisplayFinished;
            currentBoardDisplay.ShowBoard(gameBoard);
        }
        else {
            Debug.LogError("GameBoardDisplay component missing from prefab.");
        }
    }

    private void HandleBoardDisplayFinished() {
        if (currentBoardDisplay != null) {
            currentBoardDisplay.OnContinue -= HandleBoardDisplayFinished;
            currentBoardDisplay = null;
        }

        if (GameSettings.Gameplay.ShowFirstShop) {
            TransitionToShop();
        }
        else {
            TransitionToMinigame();
        }
    }

    public void OnShopEnd() {
        DebugLogger.Log(LogChannel.Systems, "Shop phase over. Transitioning to next minigame.");
        StartNextRound();
    }

    private void StartNextRound() {
        TransitionToMinigame();
    }

    private void TransitionToShop() {
        SceneTransition.LoadScene("Shop");
    }

    private void TransitionToMinigame() {
        var nextRound = gameBoard[currentRoundIndex];
        MinigameType minigameType = nextRound.minigameType;
        string sceneName = null;
        if (sceneOverrides != null) {
            if (sceneOverrides[currentRoundIndex] != null) {
                sceneName = sceneOverrides[currentRoundIndex];
            }
        }
        if(sceneName == null) sceneName = config.GetRandomSceneName(minigameType);
        
        if (string.IsNullOrEmpty(sceneName)) {
            Debug.LogError($"Failed to load scene for type {minigameType}. Stopping.");
            return;
        }

        currentMinigameSceneName = sceneName;
        SceneTransition.LoadScene(sceneName);
        DebugLogger.Log(LogChannel.Systems, $"Loading minigame: {minigameType} (Scene: {sceneName}). Double Round: {nextRound.IsDouble}");
        SceneManager.sceneLoaded += OnMinigameSceneLoaded;
    }

    private void OnMinigameSceneLoaded(Scene scene, LoadSceneMode mode) {
        SceneManager.sceneLoaded -= OnMinigameSceneLoaded;
        var minigameManager = FindMinigameManager();
        if(minigameManager == null) {
            Debug.LogError($"GameFlowManager: Couldn't find minigame manager for scene {scene.name}");
            return;
        }
        
        minigameManager.OnMinigameFinished += ProcessMinigameResults;
        var currentRoundDefinition = GetCurrentRoundDefinition();

        if(GameSettings.Gameplay.ShowTutorials) {
            string tutorialText = config.GetTutorialText(currentMinigameSceneName);
            if (!string.IsNullOrWhiteSpace(tutorialText) && !shownTutorialScenes.Contains(currentMinigameSceneName)) {
                shownTutorialScenes.Add(currentMinigameSceneName);
                ShowTutorial(tutorialText, () => minigameManager.Initialize(currentRoundDefinition.IsDouble));
            } else {
                minigameManager.Initialize(currentRoundDefinition.IsDouble);
            }
        }
        else {
            minigameManager.Initialize(currentRoundDefinition.IsDouble);
        }
        
    }

    private void ShowTutorial(string tutorialText, Action onDismissed) {
        var tutorialObject = Instantiate(tutorialPrefab);
        var display = tutorialObject.GetComponent<MinigameTutorialDisplay>();
        display.OnDismissed += onDismissed;
        display.Show(tutorialText);
    }

    private static IMinigameManager FindMinigameManager() {
        IMinigameManager minigameManager = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IMinigameManager>().FirstOrDefault();
        return minigameManager;
    }

    public void ProcessMinigameResults(PlayerMinigameResult[] results) {
        var minigameManager = FindMinigameManager();
        if (minigameManager != null) {
            minigameManager.OnMinigameFinished -= ProcessMinigameResults;
        }

        SnapshotFunds();
        economyService.ApplyRewards(results);

        DebugLogger.Log(LogChannel.Systems, "Minigame finished, results processed. Transitioning back to shop.");
        currentRoundIndex++;
        if (GameIsOver) {
            EndGame();
        } else {
            shouldShowPlacesScreen = true;
            TransitionToShop();
        }
    }

    private void SnapshotFunds() {
        previousRoundFunds = new int[4];
        for (int i = 0; i < 4; i++) {
            PlayerProfile profile = playerService.GetPlayerProfile(i);
            if (profile != null) {
                previousRoundFunds[i] = profile.Wallet.GetCurrentFunds();
            } else {
                previousRoundFunds[i] = 0;
            }
        }
    }

    private void EndGame() {
        DebugLogger.Log(LogChannel.Systems, "Ending game.");
        SceneTransition.LoadScene("Results");
    }

    public (MinigameType minigameType, bool IsDouble) GetCurrentRoundDefinition() {
        if (currentRoundIndex >= 0 && currentRoundIndex < gameBoard.Count) {
            return gameBoard[currentRoundIndex];
        }

        return (MinigameType.Unknown, false);
    }

    public List<(MinigameType minigameType, bool isDouble)> GetCompletedMinigameList() {
        List<(MinigameType, bool)> completedMinigames = new();
        for (int i = 0; i < currentRoundIndex; i++) {
            completedMinigames.Add(gameBoard[i]);
        }
        return completedMinigames;
    }

    public bool ShouldShowPlacesScreen() {
        bool returnValue = shouldShowPlacesScreen;
        shouldShowPlacesScreen = false;
        return returnValue;
    }

    public int[] GetPreviousRoundFunds() {
        return previousRoundFunds;
    }

    public List<(MinigameType minigameType, bool isDouble)> GetUpcomingMinigameList() {
        List<(MinigameType, bool)> upcomingMinigames = new();
        for (int i = currentRoundIndex; i < gameBoard.Count; i++) {
            if (i == -1) continue;
            upcomingMinigames.Add(gameBoard[i]);
        }
        return upcomingMinigames;
    }

    #if UNITY_EDITOR
    public void StartGameForColdStart(MinigameType type, bool isShop, bool isResults) {
        if (isResults) return;
        
        if (isShop) {
            boardGenerator.GenerateRandomBoard();
            sceneOverrides = null;
            gameBoard = boardGenerator.GameBoard;
            currentRoundIndex = 0;
            return;
        }
        
        gameBoard = new List<(MinigameType minigameType, bool IsDouble)> { (type, IsDouble: false) };
        currentRoundIndex = 0;

        var minigameManager = FindMinigameManager();
        if(minigameManager == null) {
            Debug.LogError($"GameFlowManager: Couldn't find minigame manager");
            return;
        }
        
        minigameManager.OnMinigameFinished += ProcessMinigameResults;
        minigameManager.Initialize(false);
    }
    #endif
}