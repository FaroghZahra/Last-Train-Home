using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>
    /// The ride: the map dimmed behind a clock pill, and a card for each arrival. Late and missed arrivals get the same
    /// card, shown in muted colours.
    /// </summary>
    public interface IRideView
    {
        event Action ContinueClicked;

        void Show();

        void ShowProgress(string now, string station, string progress);

        void ShowArrival(ArrivalModel model);

        /// <summary>Updates the countdown beside the Continue button; empty hides it.</summary>
        void ShowCountdown(string text);

        void HideArrival();

        void Hide();
    }
}
