using System;

namespace LastTrainHome.Domain
{
    /// <summary>The rules-relevant part of a passenger: where they're going and by when.</summary>
    public sealed class Passenger
    {
        public Passenger(string name, string destinationStationId, ClockTime deadline)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Passenger name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(destinationStationId))
            {
                throw new ArgumentException($"{name} needs a destination.", nameof(destinationStationId));
            }

            Name = name;
            DestinationStationId = destinationStationId;
            Deadline = deadline;
        }

        public string Name { get; }

        public string DestinationStationId { get; }

        public ClockTime Deadline { get; }
    }
}
