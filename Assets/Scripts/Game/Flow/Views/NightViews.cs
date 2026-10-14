using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The views a night is played on, handed to presenters as one unit.</summary>
    public sealed class NightViews
    {
        public NightViews(IRailMapView railMap, IRouteView route, IClockView clock, IHintView hint,
            IPassengerListView passengers, IArrivalLogView arrivalLog, ISummaryView summary, IControlsView controls)
        {
            RailMap = railMap ?? throw new ArgumentNullException(nameof(railMap));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Clock = clock ?? throw new ArgumentNullException(nameof(clock));
            Hint = hint ?? throw new ArgumentNullException(nameof(hint));
            Passengers = passengers ?? throw new ArgumentNullException(nameof(passengers));
            ArrivalLog = arrivalLog ?? throw new ArgumentNullException(nameof(arrivalLog));
            Summary = summary ?? throw new ArgumentNullException(nameof(summary));
            Controls = controls ?? throw new ArgumentNullException(nameof(controls));
        }

        public IRailMapView RailMap { get; }

        public IRouteView Route { get; }

        public IClockView Clock { get; }

        public IHintView Hint { get; }

        public IPassengerListView Passengers { get; }

        public IArrivalLogView ArrivalLog { get; }

        public ISummaryView Summary { get; }

        public IControlsView Controls { get; }
    }
}
