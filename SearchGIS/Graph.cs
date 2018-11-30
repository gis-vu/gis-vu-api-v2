using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DTOs;
using Models;

namespace GIS.VU.API
{
    class Graph
    {
        private RouteFeature[] _routeFeature2s;
        private SearchOptionsDTO _searchOptions;


        public Graph(RouteFeature[] routeFeature2s, SearchOptionsDTO searchOptions)
        {
            _routeFeature2s = routeFeature2s;
            _searchOptions = searchOptions;
        }

        public List<RouteFeature> FindShortestPath(
            RouteFeature startFeature, 
            RouteFeature endFeature,
            List<RouteFeature> featuresToOverlap)
        {
            var previous = new Dictionary<RouteFeature, RouteFeature>();
            var distances = new Dictionary<RouteFeature, double>();
            var nodes = new List<RouteFeature>();

            List<RouteFeature> path = null;

            foreach (var vertex in _routeFeature2s)
            {
                if (vertex == startFeature)
                {
                    distances[vertex] = ApplySearchOptionsToGetLength(startFeature, featuresToOverlap);
                }
                else
                {
                    distances[vertex] = double.MaxValue;
                }

                nodes.Add(vertex);
            }

            while (nodes.Count != 0)
            {
                //Debug.WriteLine(nodes.Count());

                nodes.Sort((x, y) => Math.Sign(distances[x] - distances[y]));

                var smallest = nodes.First();
                nodes.Remove(smallest);

                if (smallest == endFeature)
                {
                    path = new List<RouteFeature>();
                    while (previous.ContainsKey(smallest))
                    {
                        path.Add(smallest);
                        smallest = previous[smallest];
                    }

                    break;
                }

                if (distances[smallest] == double.MaxValue)
                {
                    break;
                }

                foreach (var neighbor in smallest.Neighbours)
                {
                    var alt = distances[smallest] + ApplySearchOptionsToGetLength(neighbor, featuresToOverlap);
                    if (alt < distances[neighbor])
                    {
                        distances[neighbor] = alt;
                        previous[neighbor] = smallest;
                    }
                }
            }

            if (path == null) //no path 
                return null;

            path.Add(startFeature);

            path.Reverse();

            return path;
        }

        private double ApplySearchOptionsToGetLength(RouteFeature feature2, List<RouteFeature> featuresToOverlap)
        {

            var featureLength = feature2.Data.Properties["lenght"];

            return featureLength;
            //temp fix

            if (_searchOptions == null)
                return featureLength;

            var option = _searchOptions.PropertyImportance.FirstOrDefault(x =>
                feature2.Data.Properties.Any(y => y.Key == x.Property && y.Value == x.Value));

            if (option != null)
            {
                featureLength *= option.Importance;
            }


            if (featuresToOverlap != null && featuresToOverlap.Contains(feature2))
            {
                featureLength *= _searchOptions.TrackOverlapImportance;
            }

            foreach (var propertyValueImportance in _searchOptions.PropertyValueImportance)
            {
                if (feature2.Data.Properties[propertyValueImportance.Property] <= propertyValueImportance.Threshold)
                    featureLength *= propertyValueImportance.Importance;
            }


            return featureLength;
        }
    }
}
