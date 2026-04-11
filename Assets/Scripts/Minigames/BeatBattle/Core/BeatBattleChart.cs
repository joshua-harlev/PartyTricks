using System.Collections.Generic;

namespace Minigames.BeatBattle.Core {
    public class BeatBattleChart {
        public IReadOnlyList<ChartNote> Notes { get; }

        public BeatBattleChart(List<ChartNote> notes) {
            Notes = notes.AsReadOnly();
        }
    }
}