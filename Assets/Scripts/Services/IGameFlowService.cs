using System;
using System.Collections.Generic;
using Minigames;

namespace Services {
    public interface IGameFlowService {
        void StartGame();
        void OnShopEnd();

        (MinigameType minigameType, bool IsDouble) GetCurrentRoundDefinition();
        List<(MinigameType minigameType, bool isDouble)> GetUpcomingMinigameList();
        List<(MinigameType minigameType, bool isDouble)> GetCompletedMinigameList();

        bool ShouldShowPlacesScreen();
        int[] GetPreviousRoundFunds();
    }
}