using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>
    /// Everything around the map while planning: the passengers' cards, the coach line, the clock, tonight's promise,
    /// the route so far and the buttons. The map itself is <see cref="IRailMapView"/>.
    /// </summary>
    public interface IPlanView
    {
        event Action UndoClicked;

        event Action ClearClicked;

        event Action DepartClicked;

        void Show(PlanModel model);

        /// <summary>A short message about a refused click that clears itself after a moment.</summary>
        void ShowRefusal(string message);

        void Hide();
    }
}
