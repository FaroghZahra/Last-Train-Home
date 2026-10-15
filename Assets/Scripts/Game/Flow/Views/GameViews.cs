using System;

namespace LastTrainHome.Game.Flow.Views
{
    /// <summary>The views the game is played on, handed to presenters as one unit.</summary>
    public sealed class GameViews
    {
        public GameViews(IRailMapView railMap, IRouteView route, ITitleScreenView title, INightCardView nightCard,
            IPlatformView platform, IPlanView plan, IRideView ride, ILogbookView logbook)
        {
            RailMap = railMap ?? throw new ArgumentNullException(nameof(railMap));
            Route = route ?? throw new ArgumentNullException(nameof(route));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            NightCard = nightCard ?? throw new ArgumentNullException(nameof(nightCard));
            Platform = platform ?? throw new ArgumentNullException(nameof(platform));
            Plan = plan ?? throw new ArgumentNullException(nameof(plan));
            Ride = ride ?? throw new ArgumentNullException(nameof(ride));
            Logbook = logbook ?? throw new ArgumentNullException(nameof(logbook));
        }

        public IRailMapView RailMap { get; }

        public IRouteView Route { get; }

        public ITitleScreenView Title { get; }

        public INightCardView NightCard { get; }

        public IPlatformView Platform { get; }

        public IPlanView Plan { get; }

        public IRideView Ride { get; }

        public ILogbookView Logbook { get; }
    }
}
