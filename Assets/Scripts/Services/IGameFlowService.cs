using System.Collections.Generic;
using Minigames;

namespace Services {
    public interface IGameFlowService {
        void StartGame();
        void OnShopEnd();
        void MarkSplashScreenShown();

        (MinigameType minigameType, bool IsDouble) GetCurrentRoundDefinition();
        List<(MinigameType minigameType, bool isDouble)> GetUpcomingMinigameList();
        List<(MinigameType minigameType, bool isDouble)> GetCompletedMinigameList();

        bool ShouldShowPlacesScreen();
        bool ShouldShowSplashScreen();
        
        int[] GetPreviousRoundFunds();
    }
}