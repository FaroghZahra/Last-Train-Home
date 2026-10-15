using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Domain.Routing;

namespace LastTrainHome.Domain.Session
{
    /// <summary>
    /// One play of a night: promise someone, plan a route, depart, finish, and optionally plan again.
    /// Route edits are only allowed while planning; calling a step out of order is a programming error and throws.
    /// </summary>
    public sealed class NightSession
    {
        private readonly Route _route;
        private PassengerOutcome[] _outcomes;

        public NightSession(NightDefinition night)
        {
            Night = night ?? throw new ArgumentNullException(nameof(night));
            _route = new Route(night.Network, night.StartStationId, night.Departure, night.Closing);
            _route.Changed += OnRouteChanged;
            _outcomes = Evaluate();
        }

        /// <summary>Raised after every change to <see cref="Stops"/>, with <see cref="Outcomes"/> already updated.</summary>
        public event Action RouteChanged;

        public event Action<NightPhase> PhaseChanged;

        public NightDefinition Night { get; }

        public NightPhase Phase { get; private set; } = NightPhase.Planning;

        /// <summary>Stops in travel order. The first stop is always the start station.</summary>
        public IReadOnlyList<RouteStop> Stops => _route.Stops;

        public RouteStop LastStop => _route.LastStop;

        public bool HasLeftStart => _route.HasLeftStart;

        public bool CanDepart => Phase == NightPhase.Planning && _route.HasLeftStart;

        /// <summary>Who the player has promised to get there on time, or null.</summary>
        public string PromisedPassenger { get; private set; }

        /// <summary>What the current route means for each passenger, in the night's passenger order.</summary>
        public IReadOnlyList<PassengerOutcome> Outcomes => _outcomes;

        /// <summary>Makes (or, with null, withdraws) tonight's promise. It can change until the train departs.</summary>
        /// <exception cref="ArgumentException">They aren't riding tonight.</exception>
        /// <exception cref="InvalidOperationException">Not planning.</exception>
        public void Promise(string passengerName)
        {
            RequirePhase(NightPhase.Planning);
            if (passengerName != null && Night.Passengers.All(passenger => passenger.Name != passengerName))
            {
                throw new ArgumentException($"'{passengerName}' isn't riding tonight.", nameof(passengerName));
            }

            PromisedPassenger = passengerName;
        }

        /// <summary>How the current route treats one passenger.</summary>
        /// <exception cref="ArgumentException">They aren't riding tonight.</exception>
        public PassengerOutcome GetOutcome(string passengerName)
        {
            for (int i = 0; i < Night.Passengers.Count; i++)
            {
                if (Night.Passengers[i].Name == passengerName)
                {
                    return _outcomes[i];
                }
            }

            throw new ArgumentException($"'{passengerName}' isn't riding tonight.", nameof(passengerName));
        }

        /// <exception cref="InvalidOperationException">Not planning.</exception>
        public AddStopResult TryAddStop(string stationId)
        {
            RequirePhase(NightPhase.Planning);
            return _route.TryAddStop(stationId);
        }

        /// <summary>Removes the most recent stop. Returns false when the route is already just the start.</summary>
        /// <exception cref="InvalidOperationException">Not planning.</exception>
        public bool TryUndo()
        {
            RequirePhase(NightPhase.Planning);
            return _route.TryRemoveLastStop();
        }

        /// <exception cref="InvalidOperationException">Not planning.</exception>
        public void Clear()
        {
            RequirePhase(NightPhase.Planning);
            _route.Reset();
        }

        /// <summary>Locks the route. The outcomes are now what will happen.</summary>
        /// <exception cref="InvalidOperationException"><see cref="CanDepart"/> is false.</exception>
        public void Depart()
        {
            if (!CanDepart)
            {
                throw new InvalidOperationException("The train can only depart while planning a route that leaves the start.");
            }

            SetPhase(NightPhase.Travelling);
        }

        /// <exception cref="InvalidOperationException">The train isn't travelling.</exception>
        public NightResult Finish()
        {
            RequirePhase(NightPhase.Travelling);
            SetPhase(NightPhase.Finished);
            return new NightResult(Night.Id, _route.Stops.ToArray(), _outcomes,
                Night.Passengers.Select(passenger => passenger.Name).ToArray(), PromisedPassenger);
        }

        /// <summary>Starts planning again from the start station.</summary>
        /// <exception cref="InvalidOperationException">The night isn't finished.</exception>
        public void Replan()
        {
            RequirePhase(NightPhase.Finished);
            SetPhase(NightPhase.Planning);
            _route.Reset();
        }

        private void OnRouteChanged()
        {
            _outcomes = Evaluate();
            RouteChanged?.Invoke();
        }

        private PassengerOutcome[] Evaluate() => OutcomeEvaluator.EvaluateAll(Night.Passengers, _route.Stops);

        private void SetPhase(NightPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        private void RequirePhase(NightPhase expected)
        {
            if (Phase != expected)
            {
                throw new InvalidOperationException($"Only allowed while {expected}; the night is {Phase}.");
            }
        }
    }
}
