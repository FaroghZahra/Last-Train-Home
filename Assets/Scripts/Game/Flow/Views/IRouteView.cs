using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Flow.Views
{
    public interface IRouteView
    {
        void ShowRoute(IReadOnlyList<Vector3> points);

        void PlaceTrain(Vector3 position);
    }
}
