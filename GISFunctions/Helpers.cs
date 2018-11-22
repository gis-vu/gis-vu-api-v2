using System;
using System.Collections.Generic;
using System.Linq;
using BAMCIS.GeoJSON;
using DTOs;

namespace GISFunctions
{
    public static class Helpers
    {

        public const double DistanceDiff = 10f / 10000000;


        public static bool AreClose(double[] first, double[] second)
        {
            if (GetDistance(first, second) < DistanceDiff)
                return true;

            return false;
        }

        private static double GetDistance(double[] first, double[] second)
        {
            return Math.Sqrt(Math.Pow(first.First() - second.First(), 2) + Math.Pow(first.Last() - second.Last(), 2));
        }

        public static bool AreClose(CustomPosition first, CustomPosition second)
        {
            if (GetDistance(new[] { first.Latitude, first.Longitude }, new[] { second.Latitude, second.Longitude }) < DistanceDiff)
                return true;

            return false;
        }

        public static double CalcualteDistanceToFeature(RouteFeature feature, Coordinate coordinate)
        {
            var lineSegments = SplitFeatureIntoLineSegments(feature);

            var distance = GetDistance(lineSegments.First(), coordinate);

            foreach (var c in lineSegments.Skip(1))
            {
                var newDistance = GetDistance(c, coordinate);

                if (newDistance < distance)
                    distance = newDistance;
            }

            return distance;
        }

        public static Tuple<CustomPosition, CustomPosition>[] SplitFeatureIntoLineSegments(RouteFeature feature)
        {
            var coords = feature.Feature.Coordinates;
            var lineSegments = new List<Tuple<CustomPosition, CustomPosition>>();

            var lastPosition = coords.First();

            foreach (var c in coords.Skip(1))
            {
                lineSegments.Add(new Tuple<CustomPosition, CustomPosition>(lastPosition, c));
                lastPosition = c;
            }

            return lineSegments.ToArray();
        }

        public static double GetDistance(Tuple<CustomPosition, CustomPosition> lineSegment, Coordinate point)
        {
            double x = point.Lat,
                y = point.Lng,
                x1 = lineSegment.Item1.Latitude,
                y1 = lineSegment.Item1.Longitude,
                x2 = lineSegment.Item2.Latitude,
                y2 = lineSegment.Item2.Longitude;

            var A = x - x1;
            var B = y - y1;
            var C = x2 - x1;
            var D = y2 - y1;

            var dot = A * C + B * D;
            var len_sq = C * C + D * D;
            double param = -1;
            if (len_sq != 0) //in case of 0 length line
                param = dot / len_sq;

            double xx, yy;

            if (param < 0)
            {
                xx = x1;
                yy = y1;
            }
            else if (param > 1)
            {
                xx = x2;
                yy = y2;
            }
            else
            {
                xx = x1 + param * C;
                yy = y1 + param * D;
            }

            var dx = x - xx;
            var dy = y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }
    }
}
