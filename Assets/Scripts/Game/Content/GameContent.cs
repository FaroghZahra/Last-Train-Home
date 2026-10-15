using System;
using System.Collections.Generic;

namespace LastTrainHome.Game.Content
{
    /// <summary>Everything the game reads from data: the week, the interface wording and the levels in play order.</summary>
    public sealed class GameContent
    {
        public GameContent(Week week, UiText ui, IReadOnlyList<Level> levels)
        {
            Week = week ?? throw new ArgumentNullException(nameof(week));
            Ui = ui ?? throw new ArgumentNullException(nameof(ui));
            Levels = levels ?? throw new ArgumentNullException(nameof(levels));
        }

        public Week Week { get; }

        public UiText Ui { get; }

        public IReadOnlyList<Level> Levels { get; }
    }
}
