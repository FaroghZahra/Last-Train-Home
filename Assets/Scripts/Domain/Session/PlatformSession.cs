using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Passengers;

namespace LastTrainHome.Domain.Session
{
    /// <summary>
    /// The platform before the train leaves: who the player has asked how they are, out of a limited number of
    /// questions. The promise belongs to the <see cref="NightSession"/>, since it is judged by the route.
    /// </summary>
    public sealed class PlatformSession
    {
        private readonly IReadOnlyList<Passenger> _passengers;
        private readonly int _asksAllowed;
        private readonly List<string> _asked = new List<string>();

        /// <param name="asksAllowed">Questions the player may ask tonight; negative for as many as they like.</param>
        public PlatformSession(IReadOnlyList<Passenger> passengers, int asksAllowed)
        {
            _passengers = passengers ?? throw new ArgumentNullException(nameof(passengers));
            _asksAllowed = asksAllowed;
        }

        public bool IsUnlimited => _asksAllowed < 0;

        /// <summary>Questions left tonight; meaningless when <see cref="IsUnlimited"/>.</summary>
        public int AsksLeft => IsUnlimited ? int.MaxValue : Math.Max(0, _asksAllowed - _asked.Count);

        /// <summary>Everyone asked so far, in the order they were asked.</summary>
        public IReadOnlyList<string> Asked => _asked;

        public bool HasAsked(string passengerName) => _asked.Contains(passengerName);

        public bool CanAsk(string passengerName) =>
            !HasAsked(passengerName) && AsksLeft > 0 && _passengers.Any(passenger => passenger.Name == passengerName);

        /// <summary>Asks a passenger how they are. False when they were already asked or no questions are left.</summary>
        public bool TryAsk(string passengerName)
        {
            if (!CanAsk(passengerName))
            {
                return false;
            }

            _asked.Add(passengerName);
            return true;
        }
    }
}
