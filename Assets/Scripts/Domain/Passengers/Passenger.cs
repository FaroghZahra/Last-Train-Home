using System;
using LastTrainHome.Domain.Clock;

namespace LastTrainHome.Domain.Passengers
{
    /// <summary>Someone on tonight's train: where they're going, by when, and what they told the player.</summary>
    public sealed class Passenger
    {
        public Passenger(string name, string destinationStationId, ClockTime deadline, PassengerStory story = null)
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
            Story = story ?? PassengerStory.Empty;
        }

        public string Name { get; }

        public string DestinationStationId { get; }

        public ClockTime Deadline { get; }

        public PassengerStory Story { get; }
    }
}
