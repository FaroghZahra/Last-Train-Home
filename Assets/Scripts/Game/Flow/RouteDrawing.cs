using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using LastTrainHome.Domain.Journey;
using LastTrainHome.Domain.Network;
using LastTrainHome.Domain.Nights;
using LastTrainHome.Domain.Passengers;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Flow
{
    /// <summary>
    /// Tonight's map and route on screen: builds the map from the level, draws the planned route along the drawn
    /// tracks, and says where the train is at any point of the journey. Shared by planning and the ride.
    /// </summary>
    public sealed class RouteDrawing
    {
        private readonly NightContext _night;
        private readonly IRailMapView _map;
        private readonly IRouteView _route;
        private readonly List<Vector3> _stopPoints = new List<Vector3>();
        private readonly List<IReadOnlyList<Vector3>> _legPaths = new List<IReadOnlyList<Vector3>>();
        private readonly List<Vector3> _routePath = new List<Vector3>();

        public RouteDrawing(NightContext night, IRailMapView map, IRouteView route)
        {
            _night = night ?? throw new ArgumentNullException(nameof(night));
            _map = map ?? throw new ArgumentNullException(nameof(map));
            _route = route ?? throw new ArgumentNullException(nameof(route));
        }

        /// <summary>Replaces the map on screen with tonight's, everyone waiting on the platform.</summary>
        public void BuildMap()
        {
            NightDefinition night = _night.Night;
            _map.Build(night.Stations.Select(ToMarker).ToArray(), night.Network.Tracks.Select(ToMarker).ToArray());
            ShowEveryoneOnThePlatform();
            Redraw();
        }

        /// <summary>Everyone where they board: on the platform at the start, or waiting at their pickup station.</summary>
        public void ShowEveryoneOnThePlatform()
        {
            _map.HidePassengers();
            foreach (Passenger passenger in _night.Night.Passengers)
            {
                _map.ShowPassenger(passenger.Name, passenger.OriginStationId ?? _night.Night.StartStationId);
            }
        }

        /// <summary>Only the passengers still waiting to be picked up; everyone at the start has boarded.</summary>
        public void ShowPickupsWaiting()
        {
            _map.HidePassengers();
            foreach (Passenger passenger in _night.Night.Passengers.Where(passenger => passenger.IsPickup))
            {
                _map.ShowPassenger(passenger.Name, passenger.OriginStationId);
            }
        }

        /// <summary>Draws the route as planned so far, with the train waiting at the start.</summary>
        public void Redraw()
        {
            var stops = _night.Session.Stops;
            _stopPoints.Clear();
            _legPaths.Clear();
            _routePath.Clear();
            _stopPoints.AddRange(stops.Select(stop => _map.GetStationPosition(stop.StationId)));
            _routePath.Add(_stopPoints[0]);
            for (int i = 1; i < stops.Count; i++)
            {
                IReadOnlyList<Vector3> leg = _map.GetTrackPath(stops[i - 1].StationId, stops[i].StationId);
                _legPaths.Add(leg);
                _routePath.AddRange(leg.Skip(1));
            }

            _route.ShowRoute(_routePath);
            _route.PlaceTrain(_stopPoints[0]);
        }

        /// <summary>
        /// At a stop, or partway along the drawn track between two. The train pulls away and brakes on screen; the
        /// clock still runs evenly, since only where the train is drawn is eased.
        /// </summary>
        public Vector3 TrainPosition(JourneyLeg leg) =>
            leg.FromStopIndex == leg.ToStopIndex
                ? _stopPoints[leg.FromStopIndex]
                : Polyline.PointAt(_legPaths[leg.FromStopIndex], Mathf.SmoothStep(0f, 1f, leg.Progress));

        private TrackMarker ToMarker(Track track)
        {
            bool isClosed = track.IsClosed || _night.Night.GetStation(track.StationA).IsClosed
                                           || _night.Night.GetStation(track.StationB).IsClosed;
            string label = isClosed
                ? _night.Ui.Get(UiKey.MapClosedTrack)
                : _night.Ui.Format(UiKey.MapMinutes, ("minutes", track.Minutes.ToString(CultureInfo.InvariantCulture)));
            return new TrackMarker(track.StationA, track.StationB, label,
                _night.Level.GetTrackBends(track.StationA, track.StationB), isClosed);
        }

        private StationMarker ToMarker(StationDefinition station)
        {
            StationPlacement placement = _night.Level.GetPlacement(station.Id);
            string destinationOf = _night.Night.Passengers
                .FirstOrDefault(passenger => passenger.DestinationStationId == station.Id)?.Name;
            string name = station.IsClosed
                ? _night.Ui.Format(UiKey.MapClosedStation, ("station", station.DisplayName))
                : station.DisplayName;
            return new StationMarker(station.Id, name, placement.Position, placement.LabelSide,
                station.Id == _night.Night.StartStationId, destinationOf, station.IsClosed);
        }
    }
}
