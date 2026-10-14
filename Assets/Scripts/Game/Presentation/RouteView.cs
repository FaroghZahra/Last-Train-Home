using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>Draws the planned route and positions the train.</summary>
    [DisallowMultipleComponent]
    public sealed class RouteView : MonoBehaviour, IRouteView
    {
        [SerializeField] private LineRenderer _routeLine;
        [SerializeField] private Transform _train;

        public void ShowRoute(IReadOnlyList<Vector3> points)
        {
            _routeLine.positionCount = points.Count;
            for (int i = 0; i < points.Count; i++)
            {
                _routeLine.SetPosition(i, points[i]);
            }
        }

        public void PlaceTrain(Vector3 position) => _train.position = position;
    }
}
