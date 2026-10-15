using System;
using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// Draws the planned route as a glowing line with dashes running along it, and moves the train. The train is a
    /// chain of cars: the first goes where it's placed, the rest follow the exact path it took, so every car stays on
    /// the rails through curves. Cars turn with the track, chug while running, and the first puffs steam.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RouteView : MonoBehaviour, IRouteView
    {
        // A train placement further than this from the last one is a jump (a new plan), not travel.
        private const float MaxTravelStep = 0.5f;

        [SerializeField, Tooltip("The route itself; its material tiles dashes along the line (Texture Mode: Tile).")]
        private LineRenderer _routeLine;

        [SerializeField, Tooltip("A wider, soft line under the route.")]
        private LineRenderer _routeGlow;

        [SerializeField, Tooltip("Locomotive first. Each car's sprite is drawn facing right.")]
        private TrainCar[] _cars = Array.Empty<TrainCar>();

        [SerializeField, Min(0f), Tooltip("Distance along the track between the centres of neighbouring cars.")]
        private float _carSpacing = 0.55f;

        [SerializeField, Tooltip("Steam from the locomotive's chimney; placed for a right-facing train.")]
        private ParticleSystem _smoke;

        [SerializeField, Tooltip("On for side-view sprites: they flip to face left and only lean on slopes, instead of " +
                                 "turning upside down. Off for top-down sprites, which turn freely with the track.")]
        private bool _keepUpright;

        [SerializeField, Range(0f, 90f), Tooltip("With Keep Upright on, the steepest a car may lean.")]
        private float _maxLean = 20f;

        [SerializeField, Min(0f), Tooltip("How quickly cars turn to follow the track.")]
        private float _turnSharpness = 14f;

        [SerializeField, Min(0f), Tooltip("How high the bodies bob while running, in world units.")]
        private float _bobHeight = 0.01f;

        [SerializeField, Min(0f), Tooltip("Bobs per second while running.")]
        private float _bobRate = 3.5f;

        [SerializeField, Min(0f), Tooltip("Steam and bobbing carry on this long after the train last moved.")]
        private float _runningLingerSeconds = 0.15f;

        [SerializeField, Tooltip("How fast the dashes run along the route, in texture tiles per second.")]
        private float _dashSpeed = 0.5f;

        // Where the locomotive has been, newest last; the cars behind are placed along it.
        private readonly List<Vector3> _trail = new List<Vector3>();
        private Material _dashMaterial;
        private Vector3 _smokeOffset;
        private float _lastMovedAt = float.NegativeInfinity;

        public void ShowRoute(IReadOnlyList<Vector3> points)
        {
            _routeLine.positionCount = points.Count;
            _routeGlow.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                _routeLine.SetPosition(i, points[i]);
                _routeGlow.SetPosition(i, points[i]);
            }
        }

        public void PlaceTrain(Vector3 position)
        {
            Vector3 last = _trail.Count > 0 ? _trail[_trail.Count - 1] : position;
            float step = Vector3.Distance(position, last);
            if (_trail.Count == 0 || step > MaxTravelStep)
            {
                // A new plan put the train back at the start: line it up level, facing right, cars behind.
                _trail.Clear();
                _trail.Add(position + Vector3.left * (_carSpacing * _cars.Length + 1f));
                _trail.Add(position);
                PlaceCars(snap: true);
                return;
            }

            if (step < 1e-5f)
            {
                return;
            }

            // Doubling back from a station: a real train would run its engine round; ours lines the cars up behind
            // the locomotive again, facing the new way, rather than driving through its own carriage.
            Vector3 direction = (position - last) / step;
            Vector3 heading = last - PointBehind(0.1f);
            bool reversing = heading.sqrMagnitude > 1e-8f && Vector3.Dot(direction, heading.normalized) < -0.3f;
            if (reversing)
            {
                _trail.Clear();
                _trail.Add(last - direction * (_carSpacing * _cars.Length + 1f));
                _trail.Add(last);
            }

            _trail.Add(position);
            TrimTrail();
            _lastMovedAt = Time.time;
            if (reversing)
            {
                PlaceCars(snap: true);
            }
        }

        private void Awake()
        {
            // An instance, so scrolling it doesn't modify the material asset.
            _dashMaterial = _routeLine.material;
            _smokeOffset = _smoke.transform.localPosition;
        }

        private void Update()
        {
            _dashMaterial.mainTextureOffset = new Vector2(-Time.time * _dashSpeed % 1f, 0f);

            if (_trail.Count > 0)
            {
                PlaceCars(snap: false);
            }

            bool running = Time.time - _lastMovedAt < _runningLingerSeconds;
            for (int i = 0; i < _cars.Length; i++)
            {
                // Cars bob out of step, like separate carriages over the joints.
                float bob = running ? Mathf.Abs(Mathf.Sin((Time.time * _bobRate + i * 0.37f) * Mathf.PI)) * _bobHeight : 0f;
                _cars[i].Body.localPosition = new Vector3(0f, bob, 0f);
            }

            ParticleSystem.EmissionModule emission = _smoke.emission;
            emission.enabled = running;
        }

        private void OnDestroy() => Destroy(_dashMaterial);

        private void PlaceCars(bool snap)
        {
            float turn = snap ? 1f : 1f - Mathf.Exp(-_turnSharpness * Time.deltaTime);
            for (int i = 0; i < _cars.Length; i++)
            {
                float distance = i * _carSpacing;
                Vector3 centre = PointBehind(distance);
                Vector3 along = centre - PointBehind(distance + 0.1f);
                TrainCar car = _cars[i];
                car.Root.position = centre;
                if (along.sqrMagnitude < 1e-8f)
                {
                    continue;
                }

                float angle = Mathf.Atan2(along.y, along.x) * Mathf.Rad2Deg;
                if (_keepUpright)
                {
                    // Near-vertical track keeps the current facing, so the sprite doesn't flicker.
                    if (Mathf.Abs(along.x) > 0.2f * along.magnitude)
                    {
                        car.Sprite.flipX = along.x < 0f;
                    }

                    // How far to turn the sprite from its resting facing, limited to a lean.
                    angle = Mathf.Clamp(Mathf.DeltaAngle(car.Sprite.flipX ? 180f : 0f, angle), -_maxLean, _maxLean);
                }

                car.Root.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpAngle(car.Root.localEulerAngles.z, angle, turn));
            }

            bool mirrored = _keepUpright && _cars.Length > 0 && _cars[0].Sprite.flipX;
            _smoke.transform.localPosition = new Vector3(mirrored ? -_smokeOffset.x : _smokeOffset.x, _smokeOffset.y, _smokeOffset.z);
        }

        /// <summary>The point on the locomotive's trail this far behind it.</summary>
        private Vector3 PointBehind(float distance)
        {
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                float segment = Vector3.Distance(_trail[i], _trail[i - 1]);
                if (distance <= segment)
                {
                    return Vector3.Lerp(_trail[i], _trail[i - 1], segment > 0f ? distance / segment : 0f);
                }

                distance -= segment;
            }

            return _trail[0];
        }

        // Keeps just enough trail for the last car, plus a little for its heading.
        private void TrimTrail()
        {
            float needed = _carSpacing * _cars.Length + 1f;
            float length = 0f;
            for (int i = _trail.Count - 1; i > 0; i--)
            {
                length += Vector3.Distance(_trail[i], _trail[i - 1]);
                if (length > needed)
                {
                    _trail.RemoveRange(0, i - 1);
                    return;
                }
            }
        }

        [Serializable]
        private struct TrainCar
        {
            [Tooltip("Placed on the track and turned with it.")]
            public Transform Root;

            [Tooltip("Child of the root that bobs while running.")]
            public Transform Body;

            public SpriteRenderer Sprite;
        }
    }
}
