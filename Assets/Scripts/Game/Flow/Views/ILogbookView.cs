using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The logbook at the end of a night: what happened to each passenger, the cab, and the week ahead.</summary>
    public interface ILogbookView
    {
        event Action NextClicked;

        void Show(LogbookModel model);

        void Hide();
    }
}
