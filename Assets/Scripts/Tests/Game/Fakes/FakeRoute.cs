using System.Collections.Generic;
using LastTrainHome.Game.Flow.Views;
using UnityEngine;

namespace LastTrainHome.Game.Tests.Fakes
{
    internal sealed class FakeRoute : IRouteView
    {
        public readonly List<Vector3> Points = new List<Vector3>();
        public Vector3 Train;

        public void ShowRoute(IReadOnlyList<Vector3> points)
        {
            Points.Clear();
            Points.AddRange(points);
        }

        public void PlaceTrain(Vector3 position) => Train = position;
    }
}
