using System;
using System.Collections.Generic;
using LastTrainHome.Data;
using LastTrainHome.Domain;
using UnityEngine;

namespace LastTrainHome.World
{
    /// <summary>
    /// The scene's rail map. Lays out each night's stations and tracks by cloning inactive templates, and reports
    /// station clicks from a single place.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RailMap : MonoBehaviour
    {
        [SerializeField, Tooltip("Inactive child cloned for every station of a night.")]
        private StationView _stationTemplate;

        [SerializeField, Tooltip("Inactive child cloned for every track of a night. Its stations may be left empty.")]
        private TrackView _trackTemplate;

        private readonly Dictionary<string, StationView> _stationsById = new Dictionary<string, StationView>(StringComparer.Ordinal);
        private readonly List<TrackView> _tracks = new List<TrackView>();

        public event Action<StationView> StationClicked;

        /// <summary>Replaces the map on screen with this night's stations and tracks.</summary>
        public void Build(NightDefinition night)
        {
            if (night == null)
            {
                throw new ArgumentNullException(nameof(night));
            }

            Clear();

            foreach (StationDefinition definition in night.Stations)
            {
                StationView station = Instantiate(_stationTemplate, transform);
                station.transform.localPosition = definition.Position;
                station.Configure(definition.Id, definition.DisplayName, definition.LabelSide);
                station.Clicked += OnStationClicked;
                station.gameObject.SetActive(true);
                _stationsById.Add(definition.Id, station);
            }

            foreach (Track definition in night.Tracks)
            {
                TrackView track = Instantiate(_trackTemplate, transform);
                track.Connect(_stationsById[definition.StationA], _stationsById[definition.StationB], definition.Minutes);
                track.gameObject.SetActive(true);
                _tracks.Add(track);
            }
        }

        /// <exception cref="KeyNotFoundException">No station has this id.</exception>
        public StationView GetStation(string stationId)
        {
            if (stationId != null && _stationsById.TryGetValue(stationId, out StationView station))
            {
                return station;
            }

            throw new KeyNotFoundException($"Rail map '{name}' has no station '{stationId}'.");
        }

        private void Awake()
        {
            if (RequiredReferences.Check(this, (_stationTemplate, nameof(_stationTemplate)), (_trackTemplate, nameof(_trackTemplate))))
            {
                _stationTemplate.gameObject.SetActive(false);
                _trackTemplate.gameObject.SetActive(false);
            }
        }

        private void Clear()
        {
            foreach (StationView station in _stationsById.Values)
            {
                station.Clicked -= OnStationClicked;
                Destroy(station.gameObject);
            }

            foreach (TrackView track in _tracks)
            {
                Destroy(track.gameObject);
            }

            _stationsById.Clear();
            _tracks.Clear();
        }

        private void OnStationClicked(StationView station) => StationClicked?.Invoke(station);
    }
}
