using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LastTrainHome.Game.Config;
using LastTrainHome.Game.Content;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The map on screen. Lays out a night's stations, tracks and passenger figures by cloning inactive child templates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RailMapView : MonoBehaviour, IRailMapView
    {
        [SerializeField, Tooltip("Inactive child cloned for every station of a night.")]
        private StationView _stationTemplate;

        [SerializeField, Tooltip("Inactive child cloned for every track of a night.")]
        private TrackView _trackTemplate;

        [SerializeField, Tooltip("Inactive child cloned for every passenger figure.")]
        private SpriteRenderer _figureTemplate;

        [SerializeField] private MapArt _art;
        [SerializeField] private PassengerArt _passengerArt;

        [SerializeField, Tooltip("Where the first figure stands from its station's pin: x is the distance to the side, " +
                                 "y the height.")]
        private Vector2 _figureOffset = new Vector2(0.62f, -0.32f);

        [SerializeField, Tooltip("Gap between figures standing at the same station.")]
        private float _figureSpacing = 0.3f;

        [SerializeField, Min(0.01f)] private float _figurePopSeconds = 0.25f;

        [SerializeField, Tooltip("Pin of the station the train leaves from.")]
        private Color _startPinColor = new Color(0.24f, 0.25f, 0.3f);

        [SerializeField, Tooltip("Pin of a station nobody is heading for; destinations take their passenger's colour.")]
        private Color _stationPinColor = new Color(0.96f, 0.93f, 0.84f);

        [SerializeField, Tooltip("Pin of a station that's closed tonight.")]
        private Color _closedPinColor = new Color(0.6f, 0.56f, 0.5f);

        private readonly Dictionary<string, StationView> _stations = new Dictionary<string, StationView>(StringComparer.Ordinal);
        private readonly List<TrackView> _tracks = new List<TrackView>();
        private readonly Dictionary<(string From, string To), Vector3[]> _trackPaths = new Dictionary<(string, string), Vector3[]>();
        private readonly Dictionary<string, SpriteRenderer> _figures = new Dictionary<string, SpriteRenderer>(StringComparer.Ordinal);
        private readonly Dictionary<string, List<string>> _figuresAtStation = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        private readonly Dictionary<string, LabelSide> _labelSides = new Dictionary<string, LabelSide>(StringComparer.Ordinal);

        public event Action<string> StationClicked;

        public void Build(IReadOnlyList<StationMarker> stations, IReadOnlyList<TrackMarker> tracks)
        {
            Clear();

            foreach (StationMarker marker in stations)
            {
                StationView station = Instantiate(_stationTemplate, transform);
                station.transform.localPosition = marker.Position;
                station.gameObject.SetActive(true); // before Configure: the label can only measure its text while active
                station.Configure(marker.Id, marker.DisplayName, marker.LabelSide, _art.GetBuilding(marker.Id), PinColor(marker),
                    marker.IsClosed);
                station.Clicked += OnStationClicked;
                _stations.Add(marker.Id, station);
                _labelSides.Add(marker.Id, marker.LabelSide);
            }

            foreach (TrackMarker track in tracks)
            {
                var controlPoints = new List<Vector3> { GetStationPosition(track.StationA) };
                foreach (Vector2 bend in track.Bends)
                {
                    controlPoints.Add(transform.TransformPoint(bend));
                }

                controlPoints.Add(GetStationPosition(track.StationB));
                Vector3[] path = TrackCurve.Sample(controlPoints).ToArray();
                Vector3[] reversed = path.Reverse().ToArray();
                _trackPaths[(track.StationA, track.StationB)] = path;
                _trackPaths[(track.StationB, track.StationA)] = reversed;

                TrackView view = Instantiate(_trackTemplate, transform);
                view.Draw($"{track.StationA} - {track.StationB}", path, track.Label, track.IsClosed);
                view.gameObject.SetActive(true);
                _tracks.Add(view);
            }
        }

        public Vector3 GetStationPosition(string stationId) => GetStation(stationId).transform.position;

        public IReadOnlyList<Vector3> GetTrackPath(string fromStationId, string toStationId) =>
            _trackPaths.TryGetValue((fromStationId, toStationId), out Vector3[] path)
                ? path
                : throw new KeyNotFoundException($"The map has no track from '{fromStationId}' to '{toStationId}'.");

        public void HighlightStations(IReadOnlyCollection<string> stationIds)
        {
            foreach (KeyValuePair<string, StationView> station in _stations)
            {
                station.Value.SetHighlighted(stationIds.Contains(station.Key));
            }
        }

        public void FlashRefused(string stationId) => GetStation(stationId).FlashRefused();

        public void ShowPassenger(string passengerName, string stationId)
        {
            Sprite sprite = _passengerArt.GetFigure(passengerName);
            if (sprite == null)
            {
                return;
            }

            Vector3 pin = GetStationPosition(stationId);
            if (!_figuresAtStation.TryGetValue(stationId, out List<string> standing))
            {
                standing = new List<string>();
                _figuresAtStation.Add(stationId, standing);
            }

            if (!_figures.TryGetValue(passengerName, out SpriteRenderer figure))
            {
                figure = Instantiate(_figureTemplate, transform);
                figure.name = passengerName;
                figure.sprite = sprite;
                _figures.Add(passengerName, figure);
            }

            // Figures line up away from the pin on the side facing the middle of the map, so the side panels never
            // cover them, unless the station's name is on that side. Each stands a little further back so they overlap.
            int index = standing.Count;
            standing.Add(passengerName);
            float side = FigureSide(stationId, pin);
            figure.transform.position = pin + new Vector3(side * (_figureOffset.x + index * _figureSpacing), _figureOffset.y + index * 0.04f, 0f);
            figure.sortingOrder = _figureTemplate.sortingOrder - index;
            figure.gameObject.SetActive(true);
            StartCoroutine(PopIn(figure.transform));
        }

        public void HidePassenger(string passengerName)
        {
            if (_figures.TryGetValue(passengerName, out SpriteRenderer figure))
            {
                figure.gameObject.SetActive(false);
            }

            foreach (List<string> standing in _figuresAtStation.Values)
            {
                standing.Remove(passengerName);
            }
        }

        public void HidePassengers()
        {
            foreach (SpriteRenderer figure in _figures.Values)
            {
                figure.gameObject.SetActive(false);
            }

            _figuresAtStation.Clear();
        }

        private void Awake()
        {
            _stationTemplate.gameObject.SetActive(false);
            _trackTemplate.gameObject.SetActive(false);
            _figureTemplate.gameObject.SetActive(false);
        }

        private IEnumerator PopIn(Transform figure)
        {
            for (float elapsed = 0f; elapsed < _figurePopSeconds; elapsed += Time.deltaTime)
            {
                float t = elapsed / _figurePopSeconds;
                // Overshoots slightly before settling, like a little hop off the train.
                figure.localScale = Vector3.one * ((1f + 0.25f * Mathf.Sin(t * Mathf.PI)) * Mathf.Min(1f, t * 2f));
                yield return null;
            }

            figure.localScale = Vector3.one;
        }

        /// <summary>+1 for the right of the pin, -1 for the left.</summary>
        private float FigureSide(string stationId, Vector3 pin) => _labelSides[stationId] switch
        {
            LabelSide.Right => -1f,
            LabelSide.Left => 1f,
            _ => pin.x < transform.position.x ? 1f : -1f,
        };

        private Color PinColor(StationMarker marker) =>
            marker.IsClosed ? _closedPinColor
            : marker.IsStart ? _startPinColor
            : marker.DestinationOf != null ? _passengerArt.GetColor(marker.DestinationOf)
            : _stationPinColor;

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

            foreach (SpriteRenderer figure in _figures.Values)
            {
                Destroy(figure.gameObject);
            }

            _figures.Clear();
            _figuresAtStation.Clear();

            _stations.Clear();
            _labelSides.Clear();
            _tracks.Clear();
            _trackPaths.Clear();
        }

        private void OnStationClicked(string stationId) => StationClicked?.Invoke(stationId);
    }
}
