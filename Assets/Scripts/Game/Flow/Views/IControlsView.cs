using System;

namespace LastTrainHome.Game.Flow.Views
{
    public interface IControlsView
    {
        event Action UndoClicked;

        event Action ClearClicked;

        event Action DepartClicked;

        event Action FastForwardClicked;

        void ShowPlanning(bool hasStops);

        void ShowTravelling(bool isFastForward);

        void ShowFinished();
    }
}
