using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeRailMap : IRailMapView
    {
        public readonly List<StationMarker> Stations = new List<StationMarker>();
        public readonly List<string> Flashed = new List<string>();
        public readonly List<string> Highlighted = new List<string>();
        public readonly List<TrackMarker> Tracks = new List<TrackMarker>();
        public readonly Dictionary<string, string> PassengerStations = new Dictionary<string, string>();
        public int TrackCount;

        public event Action<string> StationClicked;

        public void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<TrackMarker> tracks)
        {
            Stations.Clear();
            Stations.AddRange(stations);
            TrackCount = tracks.Count;
            Tracks.Clear();
            Tracks.AddRange(tracks);
        }

        public void HighlightStations(IReadOnlyCollection<string> stationIds)
        {
            Highlighted.Clear();
            Highlighted.AddRange(stationIds);
        }

        public Vector3 GetStationPosition(string stationId) => Stations.Single(station => station.Id == stationId).Position;

        public IReadOnlyList<Vector3> GetTrackPath(string fromStationId, string toStationId) =>
            new[] { GetStationPosition(fromStationId), GetStationPosition(toStationId) };

        public void FlashRefused(string stationId) => Flashed.Add(stationId);

        public void ShowPassenger(string passengerName, string stationId) => PassengerStations[passengerName] = stationId;

        public void HidePassenger(string passengerName) => PassengerStations.Remove(passengerName);

        public void HidePassengers() => PassengerStations.Clear();

        public void Click(string stationId) => StationClicked?.Invoke(stationId);
    }
}
