using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Network;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>The map on screen. Lays out a night's stations and tracks by cloning inactive child templates.</summary>
    [DisallowMultipleComponent]
    public sealed class RailMapView : MonoBehaviour, IRailMapView
    {
        [SerializeField, Tooltip("Inactive child cloned for every station of a night.")]
        private StationView _stationTemplate;

        [SerializeField, Tooltip("Inactive child cloned for every track of a night.")]
        private TrackView _trackTemplate;

        private readonly Dictionary<string, StationView> _stations = new Dictionary<string, StationView>(StringComparer.Ordinal);
        private readonly List<TrackView> _tracks = new List<TrackView>();

        public event Action<string> StationClicked;

        public void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<Track> tracks)
        {
            Clear();

            foreach (StationMarker marker in stations)
            {
                StationView station = Instantiate(_stationTemplate, transform);
                station.transform.localPosition = marker.Position;
                station.Configure(marker.Id, marker.DisplayName, marker.LabelSide);
                station.Clicked += OnStationClicked;
                station.gameObject.SetActive(true);
                _stations.Add(marker.Id, station);
            }

            foreach (Track track in tracks)
            {
                TrackView view = Instantiate(_trackTemplate, transform);
                view.Draw($"{track.StationA} - {track.StationB}",
                    GetStationPosition(track.StationA), GetStationPosition(track.StationB), track.Minutes);
                view.gameObject.SetActive(true);
                _tracks.Add(view);
            }
        }

        public Vector3 GetStationPosition(string stationId) => GetStation(stationId).transform.position;

        public void FlashRefused(string stationId) => GetStation(stationId).FlashRefused();

        private void Awake()
        {
            _stationTemplate.gameObject.SetActive(false);
            _trackTemplate.gameObject.SetActive(false);
        }

        private StationView GetStation(string stationId) =>
            stationId != null && _stations.TryGetValue(stationId, out StationView station)
                ? station
                : throw new KeyNotFoundException($"The map has no station '{stationId}'.");

        private void Clear()
        {
            foreach (StationView station in _stations.Values)
            {
                station.Clicked -= OnStationClicked;
                Destroy(station.gameObject);
            }

            foreach (TrackView track in _tracks)
            {
                Destroy(track.gameObject);
            }

            _stations.Clear();
            _tracks.Clear();
        }

        private void OnStationClicked(string stationId) => StationClicked?.Invoke(stationId);
    }
}
