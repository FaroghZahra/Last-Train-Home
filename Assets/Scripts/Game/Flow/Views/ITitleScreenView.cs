using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The title screen: the game's name, what the week is about, and the button that starts it.</summary>
    public interface ITitleScreenView
    {
        event Action BeginClicked;

        void Show(TitleScreenModel model);

        void Hide();
    }
}
