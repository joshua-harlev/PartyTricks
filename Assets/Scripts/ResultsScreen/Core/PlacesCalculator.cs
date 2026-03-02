using System.Collections.Generic;

namespace ResultsScreen.Core {
    public static class PlacesCalculator {
        public static PlacesEntry[] CalculatePlaces(int[] funds) {
            var players = new List<(int index, int funds)>();
            for (int i = 0; i < funds.Length; i++) {
                players.Add((i, funds[i]));
            }
            
            players.Sort((a, b) =>
            {
                int comparison = b.funds.CompareTo(a.funds);
                if (comparison != 0) return comparison; 
                return a.index.CompareTo(b.index);
            });
            
            var entries = new PlacesEntry[players.Count];
            for (int i = 0; i < players.Count; i++) {
                int rank;
                if (i > 0 && players[i].funds == players[i - 1].funds) {
                    rank = entries[i - 1].Rank;
                } else {
                    rank = i;
                }
                entries[i] = new PlacesEntry(players[i].index, players[i].funds, rank);
            }
            
            return entries;
        }
    }
}