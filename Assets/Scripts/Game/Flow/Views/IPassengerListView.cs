using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    public interface IPassengerListView
    {
        /// <summary>One row per passenger, replacing the rows on screen.</summary>
        void ShowRows(IReadOnlyList<string> rows);
    }
}
