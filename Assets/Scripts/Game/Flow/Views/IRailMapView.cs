using System;
using System.Collections.Generic;
using LastTrainHome.Domain.Network;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    public interface IRailMapView
    {
        /// <summary>Raised with the station's id.</summary>
        event Action<string> StationClicked;

        /// <summary>Replaces whatever map is on screen.</summary>
        void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<Track> tracks);

        Vector3 GetStationPosition(string stationId);

        /// <summary>Briefly shows that a click on this station was refused.</summary>
        void FlashRefused(string stationId);
    }
}
