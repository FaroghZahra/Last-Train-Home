using System;

namespace LastTrainHome.Game.Flow.Views
{
    public interface ISummaryView
    {
        event Action PlanAgainClicked;

        event Action NextNightClicked;

        /// <param name="canContinue">Whether to offer the next night.</param>
        void Show(string body, bool canContinue);

        void Hide();
    }
}
