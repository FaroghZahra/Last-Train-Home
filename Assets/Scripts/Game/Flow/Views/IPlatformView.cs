using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The platform: tonight's passengers, one card each, to ask and to promise before the map appears.</summary>
    public interface IPlatformView
    {
        /// <summary>Raised with the passenger's name.</summary>
        event Action<string> AskClicked;

        /// <summary>Raised with the passenger's name.</summary>
        event Action<string> PromiseClicked;

        event Action ContinueClicked;

        void Show(PlatformModel model);

        void Hide();
    }
}
