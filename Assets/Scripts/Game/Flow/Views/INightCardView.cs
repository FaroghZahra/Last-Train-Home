using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The card that opens each night: which night, whose it is, and the week so far.</summary>
    public interface INightCardView
    {
        event Action ContinueClicked;

        void Show(NightCardModel model);

        void Hide();
    }
}
