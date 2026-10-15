using System.Collections.Generic;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>What the planning screen shows around the map.</summary>
    public sealed class PlanModel
    {
        public string NightLabel { get; set; }

        public string ClockLabel { get; set; }

        /// <summary>Empty hides the coach line.</summary>
        public string Coach { get; set; }

        public IReadOnlyList<PlanCardModel> Cards { get; set; }

        public bool HasPromise { get; set; }

        public string PromiseHeader { get; set; }

        public string PromiseLine { get; set; }

        public string PromiseStatus { get; set; }

        public bool PromiseKept { get; set; }

        public string RouteHeader { get; set; }

        public IReadOnlyList<PlanStopModel> Stops { get; set; }

        public string UndoLabel { get; set; }

        public string ClearLabel { get; set; }

        /// <summary>Whether Undo and Clear do anything.</summary>
        public bool CanEdit { get; set; }

        public string DepartLabel { get; set; }

        /// <summary>Who waits if the train leaves now.</summary>
        public string DepartDetail { get; set; }

        public bool CanDepart { get; set; }
    }
}
