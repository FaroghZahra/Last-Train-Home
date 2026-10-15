using System;
using LastTrainHome.Domain.Clock;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>
    /// Someone on tonight's train: where they're going, by when, and what they told the player. Most board at the start
    /// station; someone with an <see cref="OriginStationId"/> waits there until the train picks them up.
    /// </summary>
    public sealed class Passenger
    {
        /// <param name="originStationId">Where they board, or null for the start station.</param>
        public Passenger(string name, string destinationStationId, ClockTime deadline, PassengerStory story = null,
            string originStationId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Passenger name is required.", nameof(name));
            }

            if (string.IsNullOrWhiteSpace(destinationStationId))
            {
                throw new ArgumentException($"{name} needs a destination.", nameof(destinationStationId));
            }

            if (string.Equals(originStationId, destinationStationId, StringComparison.Ordinal))
            {
                throw new ArgumentException($"{name} boards and gets off at the same station.", nameof(originStationId));
            }

            Name = name;
            DestinationStationId = destinationStationId;
            Deadline = deadline;
            Story = story ?? PassengerStory.Empty;
            OriginStationId = string.IsNullOrWhiteSpace(originStationId) ? null : originStationId;
        }

        public string Name { get; }

        public string DestinationStationId { get; }

        public ClockTime Deadline { get; }

        public PassengerStory Story { get; }

        /// <summary>Where they wait to be picked up, or null when they board at the start station.</summary>
        public string OriginStationId { get; }

        public bool IsPickup => OriginStationId != null;
    }
}
