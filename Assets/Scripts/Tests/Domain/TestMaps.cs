using System.Linq;
using LastTrainHome.Domain.Clock;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Tests
{
    /// <summary>
    /// The Night 2 map and passengers from Assets/Design/Implementation_Plan.md §3. If the design changes,
    /// keep it in step with Data/Levels/Night2.json.
    /// </summary>
    internal static class TestMaps
    {
        public const string Central = "Central";
        public const string OldTown = "Old Town";
        public const string Market = "Market";
        public const string Harbour = "Harbour";
        public const string Hillside = "Hillside";

        public static readonly ClockTime Midnight = new ClockTime(0);
        public static readonly ClockTime Closing = new ClockTime(60);

        public static RailNetwork Night2Network() => new RailNetwork(
            new[] { Central, OldTown, Market, Harbour, Hillside },
            new[]
            {
                new Track(Central, OldTown, 10),
                new Track(Central, Market, 10),
                new Track(Market, Harbour, 10),
                new Track(Market, Hillside, 10),
                new Track(OldTown, Hillside, 20),
            });

        public static Passenger[] Night2Passengers() => new[]
        {
            new Passenger("Agnes", Harbour, new ClockTime(40)),
            new Passenger("Tomás", Hillside, new ClockTime(40)),
            new Passenger("Priya", OldTown, new ClockTime(30)),
        };

        public static Route Night2Route() => new Route(Night2Network(), Central, Midnight, Closing);

        public static NightDefinition Night2() => new NightDefinition("Night2", "Night 2", Central, Midnight, Closing,
            new[] { Central, OldTown, Market, Harbour, Hillside }.Select(id => new StationDefinition(id)).ToArray(),
            Night2Network().Tracks, Night2Passengers());
    }
}
