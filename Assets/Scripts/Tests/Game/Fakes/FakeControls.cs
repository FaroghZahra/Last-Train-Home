using System;
using LastTrainHome.Game.Flow.Views;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeControls : IControlsView
    {
        public string State;

        public event Action UndoClicked;

        public event Action ClearClicked;

        public event Action DepartClicked;

        public event Action FastForwardClicked;

        public void ShowPlanning(bool hasStops) => State = hasStops ? "planning" : "planning-empty";

        public void ShowTravelling(bool isFastForward) => State = isFastForward ? "travelling-fast" : "travelling";

        public void ShowFinished() => State = "finished";

        public void ClickUndo() => UndoClicked?.Invoke();

        public void ClickClear() => ClearClicked?.Invoke();

        public void ClickDepart() => DepartClicked?.Invoke();

        public void ClickFastForward() => FastForwardClicked?.Invoke();
    }
}
