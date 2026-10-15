using System;
using UnityEngine;

namespace LastTrainHome.Game.Config
{
    /// <summary>The building drawn for each station. A station with no entry is drawn as its pin alone.</summary>
    [CreateAssetMenu(fileName = "MapArt", menuName = "Last Train Home/Map Art")]
    public sealed class MapArt : ScriptableObject
    {
        [SerializeField] private StationArt[] _stations = Array.Empty<StationArt>();

        /// <summary>The building for a station id, or null when the station has none.</summary>
        public Sprite GetBuilding(string stationId)
        {
            foreach (StationArt station in _stations)
            {
                if (string.Equals(station.StationId, stationId, StringComparison.Ordinal))
                {
                    return station.Building;
                }
            }

            return null;
        }

        [Serializable]
        private struct StationArt
        {
            [Tooltip("Matches the station id in the level files.")]
            public string StationId;

            public Sprite Building;
        }
    }
}
