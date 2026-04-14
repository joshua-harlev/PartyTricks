using System;
using System.Collections.Generic;
using Minigames.BeatBattle.Core;

namespace Minigames.BeatBattle.States {
    public class BeatBattlePlaybackState : IBeatBattleGameState {
        private readonly BeatBattleLaneView[] playerLanes;
        private readonly BeatBattleMinigameManager manager;
        private readonly BeatBattleChart chart;
        private readonly int creatorIndex;
        private readonly int[] playerIndices; // non-creators
        private readonly ChartPlayer[] chartPlayers;
        private int phaseStartTimeInMs;
        private Action<int, float>[] noteHitHandlers;
        
        public BeatBattlePlaybackState(BeatBattleMinigameManager manager, BeatBattleChart beatBattleChart) {
            this.manager = manager;
            this.chart = beatBattleChart;
            this.creatorIndex = manager.RoundState.CurrentCreatorIndex;
            
            
            var indices = new List<int>();
            for (int i = 0; i < 4; i++) {
                if(i != creatorIndex) indices.Add(i);
            }
            playerIndices = indices.ToArray();

            playerLanes = new BeatBattleLaneView[3];
            for (int i = 0; i < 3; i++) {
                playerLanes[i] = manager.GetLaneView(playerIndices[i]);
            }
            
            var config = manager.ConfigSO.GetConfig();
            chartPlayers = new ChartPlayer[3];
            for (int i = 0; i < 3; i++) {
                chartPlayers[i] = new ChartPlayer(chart, config);
            }
        }

        public void Enter() {
            noteHitHandlers = new Action<int, float>[3];
            phaseStartTimeInMs = manager.GetTimelinePositionInMs();
            manager.InvokePlaybackPhaseStarted(creatorIndex, chart);
            
            float playbackDuration = manager.ConfigSO.PlaybackDurationInSeconds;
            float gridSlotDuration = manager.ConfigSO.GetConfig().GridSlotDuration;
            for (int i = 0; i < 3; i++) {
                playerLanes[i].BeginPlayback(chart, playbackDuration, gridSlotDuration);
            }
            

            for (int i = 0; i < 3; i++) {
                int playerIndex = playerIndices[i];
                int capturedIndex = i;
                noteHitHandlers[i] = (noteIndex, offsetInMs) =>
                {
                    manager.InvokePlayerHit(playerIndex, noteIndex, offsetInMs);
                    playerLanes[capturedIndex].OnNoteHit(noteIndex);
                };
                chartPlayers[i].NoteHit += noteHitHandlers[i];
            }
        }

        public void OnUpdate() {
            float elapsedTimeInSeconds = (manager.GetTimelinePositionInMs() - phaseStartTimeInMs) / 1000f;

            if (elapsedTimeInSeconds >= manager.ConfigSO.PlaybackDurationInSeconds) {
                ScoreRound();
                manager.ChangeState(new BeatBattleTransitionState(manager));
                return;
            }

            for (int i = 0; i < 3; i++) {
                var input = manager.GetInputHandler(playerIndices[i]);
                if (input.SelectIsPressed()) {
                    chartPlayers[i].ProcessInput(elapsedTimeInSeconds, NoteType.A);
                } else if (input.CancelIsPressed()) {
                    chartPlayers[i].ProcessInput(elapsedTimeInSeconds, NoteType.B);
                }
                playerLanes[i].UpdateScroll(elapsedTimeInSeconds);
            }
        }

        public void Exit() {
            for (int i = 0; i < 3; i++) {
                chartPlayers[i].NoteHit -= noteHitHandlers[i];
                playerLanes[i].ClearNotes();
            }
        }

        private void ScoreRound() {
            for (int i = 0; i < 3; i++) {
                int missCount = chartPlayers[i].GetMissedNoteCount();
                int hitCount = chart.Notes.Count - missCount;

                for (int j = 0; j < hitCount; j++) {
                    manager.Scorer.RegisterHit(playerIndices[i]);
                }

                for (int k = 0; k < missCount; k++) {
                    manager.Scorer.RegisterCreatorMissBonus(creatorIndex);
                }
            }
            manager.InvokeRoundEnded(manager.RoundState.CurrentRoundIndex, manager.Scorer.GetTotalScores());
        }
    }
}