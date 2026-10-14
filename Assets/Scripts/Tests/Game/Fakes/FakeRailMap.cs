using System;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Domain.Network;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeRailMap : IRailMapView
    {
        public readonly List<StationMarker> Stations = new List<StationMarker>();
        public readonly List<string> Flashed = new List<string>();
        public int TrackCount;

        public event Action<string> StationClicked;

        public void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<Track> tracks)
        {
            Stations.Clear();
            Stations.AddRange(stations);
            TrackCount = tracks.Count;
        }

        public Vector3 GetStationPosition(string stationId) => Stations.Single(station => station.Id == stationId).Position;

        public void FlashRefused(string stationId) => Flashed.Add(stationId);

        public void Click(string stationId) => StationClicked?.Invoke(stationId);
    }
}
