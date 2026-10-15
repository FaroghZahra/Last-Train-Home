using System.Collections.Generic;
using UnityEngine;

namespace LastTrainHome.Game.Presentation
{
    /// <summary>
    /// The smooth curve a track is drawn along: uniform Catmull-Rom through its stations and bends. Design/route_tracks.py
    /// samples the same curve to keep tracks off the water, so change both together.
    /// </summary>
    public static class TrackCurve
    {
        public const int SamplesPerSegment = 12;

        /// <summary>Points along the curve through every control point, starting and ending exactly on the first and last.</summary>
        public static List<Vector3> Sample(IReadOnlyList<Vector3> controlPoints)
        {
            var result = new List<Vector3>();
            int count = controlPoints.Count;
            if (count < 3)
            {
                result.AddRange(controlPoints);
                return result;
            }

            result.Add(controlPoints[0]);
            for (int i = 0; i < count - 1; i++)
            {
                Vector3 p0 = controlPoints[Mathf.Max(i - 1, 0)];
                Vector3 p1 = controlPoints[i];
                Vector3 p2 = controlPoints[i + 1];
                Vector3 p3 = controlPoints[Mathf.Min(i + 2, count - 1)];
                for (int step = 1; step <= SamplesPerSegment; step++)
                {
                    float t = step / (float)SamplesPerSegment;
                    float t2 = t * t;
                    float t3 = t2 * t;
                    result.Add(0.5f * (2f * p1 + (p2 - p0) * t + (2f * p0 - 5f * p1 + 4f * p2 - p3) * t2
                                       + (-p0 + 3f * p1 - 3f * p2 + p3) * t3));
                }
            }

            return result;
        }
    }
}
