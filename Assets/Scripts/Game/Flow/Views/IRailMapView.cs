using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    public interface IRailMapView
    {
        /// <summary>Raised with the station's id.</summary>
        event Action<string> StationClicked;

        /// <summary>Replaces whatever map is on screen.</summary>
        void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<TrackMarker> tracks);

        Vector3 GetStationPosition(string stationId);

        /// <summary>The track as drawn, in world space, from one station's position to the other's.</summary>
        IReadOnlyList<Vector3> GetTrackPath(string fromStationId, string toStationId);

        /// <summary>Makes these stations glow as the ones the train can go to next; the rest stop glowing.</summary>
        void HighlightStations(IReadOnlyCollection<string> stationIds);

        /// <summary>Briefly shows that a click on this station was refused.</summary>
        void FlashRefused(string stationId);

        /// <summary>Stands a passenger's figure at a station, beside any already there.</summary>
        void ShowPassenger(string passengerName, string stationId);

        /// <summary>Takes one passenger's figure off the map, e.g. when the train picks them up.</summary>
        void HidePassenger(string passengerName);

        /// <summary>Takes every passenger figure off the map.</summary>
        void HidePassengers();
    }
}
