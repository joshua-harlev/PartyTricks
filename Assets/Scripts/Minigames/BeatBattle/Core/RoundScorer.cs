using System;
using System.Collections.Generic;

namespace Minigames.BeatBattle.Core {
    public class RoundScorer {
        private int[] hitsByPlayer = new int[4];
        private int[] bonusesByPlayer = new int[4];
        private int[] totalScores = new int[4];
        private bool initialized;
        
        public event Action<int, int> ScoreChanged;

        public RoundScorer() {
            initialized = true;
            Array.Fill(hitsByPlayer, 0);
            Array.Fill(bonusesByPlayer, 0);
            Array.Fill(totalScores, 0);
        }
        
        public void RegisterHit(int playerIndex) {
            if (!initialized) return;
            hitsByPlayer[playerIndex]++;
            totalScores[playerIndex]++;
            ScoreChanged?.Invoke(playerIndex, totalScores[playerIndex]);
        }

        public int[] GetTotalScores() {
            if (!initialized) return null;
            return totalScores.Clone() as int[];
        }

        public void RegisterCreatorMissBonus(int creatorIndex) {
            if (!initialized) return;
            bonusesByPlayer[creatorIndex]++;
            totalScores[creatorIndex]++;
            ScoreChanged?.Invoke(creatorIndex, totalScores[creatorIndex]);
        }

        public int[] GetRankings() {
            if (!initialized) return null;
            var indexed = new List<(int index, int score)>();
            for (int i = 0; i < 4; i++) {
                indexed.Add((i, totalScores[i]));
            }
            
            indexed.Sort((a, b) => b.score.CompareTo(a.score));

            var rankings = new int[4];
            for (int i = 0; i < indexed.Count; i++) {
                if (i > 0 && indexed[i].score == indexed[i - 1].score) {
                    rankings[indexed[i].index] = rankings[indexed[i - 1].index];
                }
                else {
                    rankings[indexed[i].index] = i;
                }
            }
            
            return rankings;
        }
    }
}