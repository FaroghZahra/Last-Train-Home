using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Flow
{
    /// <summary>Positions along a path of straight segments, measured by distance travelled.</summary>
    public static class Polyline
    {
        /// <param name="progress">0 at the first point, 1 at the last; clamped.</param>
        public static Vector3 PointAt(IReadOnlyList<Vector3> points, float progress)
        {
            if (points == null || points.Count == 0)
            {
                throw new ArgumentException("A path needs at least one point.", nameof(points));
            }

            float remaining = Mathf.Clamp01(progress) * Length(points);
            for (int i = 1; i < points.Count; i++)
            {
                float segment = Vector3.Distance(points[i - 1], points[i]);
                if (remaining <= segment && segment > 0f)
                {
                    return Vector3.Lerp(points[i - 1], points[i], remaining / segment);
                }

                remaining -= segment;
            }

            return points[points.Count - 1];
        }

        public static float Length(IReadOnlyList<Vector3> points)
        {
            float length = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                length += Vector3.Distance(points[i - 1], points[i]);
            }

            return length;
        }
    }
}
