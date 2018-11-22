using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using BAMCIS.GeoJSON;
using DTOs;
using GISFunctions;
using Microsoft.EntityFrameworkCore.Internal;
using ReadMyGIS;

namespace GIS.VU.API
{
    public class RouteSearchEngine
    {
        private readonly RouteFeature[] _routeFeatures;

        public RouteSearchEngine(string path)
        {

            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(path, FileMode.Open))
            {
                _routeFeatures = (RouteFeature[]) formatter.Deserialize(fileStream);
            }

        }

        public RouteSearchResponse FindRoute(RouteSearchRequest request)
        {
            var startFeature = FindClosetFeature(request.Start);
            var endFeature = FindClosetFeature(request.End);

            var pointFeature = request.Point == null ? null : FindClosetFeature(request.Point);

            if (pointFeature == null)
            {
                //var g1 = new Graph(_routeFeatures, null);
                //var path = g1.FindShortestPath(startFeature, endFeature, null);

                //if (path == null)
                 //   return new RouteSearchResponse(Array.Empty<Route>());

               // var route1 = PathToRoute(path);

                var g2 = new Graph(_routeFeatures, request.SearchOptions);
                var path2 = g2.FindShortestPath(startFeature, endFeature, null);

                var route2 = PathToRoute(path2);

                return new RouteSearchResponse(new[] { route2 });
            }
            else
            {
                //var g1 = new Graph(_routeFeatures, null);
                //var path1 = g1.FindShortestPath(startFeature, pointFeature, null);

                //if (path1 == null)
                //    return new RouteSearchResponse(Array.Empty<Route>());

                //var path2 = g1.FindShortestPath(pointFeature, endFeature, null);
                //if (path2 == null)
                //    return new RouteSearchResponse(Array.Empty<Route>());

                //var route1 = PathToRoute(path1);
                //var route2 = PathToRoute(path2);
                //var r1 = MergeTwoRoutes(route1, route2);

                var g2 = new Graph(_routeFeatures, request.SearchOptions);
                var path3 = g2.FindShortestPath(startFeature, pointFeature, null);
                var path4 = g2.FindShortestPath(pointFeature, endFeature, path3);            

                var route3 = PathToRoute(path3);
                var route4 = PathToRoute(path4);
                var r = MergeTwoRoutes(route3, route4);

                return new RouteSearchResponse(new[] { r  });
            }
        }

        private Route MergeTwoRoutes(Route route1, Route route2)
        {
            var data =new RouteData()
            {
                Type = route1.Data.Type
            };
            var info = new RouteInfo()
            {
                Length = route1.Info.Length + route2.Info.Length
            };

            var result = new Route()
            {
                Data = data,
                Info = info
            };

            var coordinates = new List<double[]>();

            if (Enumerable.SequenceEqual(route1.Data.Coordinates.Last() , route2.Data.Coordinates.First()))
            {
                coordinates.AddRange(route1.Data.Coordinates);
                coordinates.AddRange(route2.Data.Coordinates);
            }
            else if (Enumerable.SequenceEqual(route1.Data.Coordinates.Last() , route2.Data.Coordinates.Last()))
            {
                
                coordinates.AddRange(route2.Data.Coordinates);
                coordinates.Reverse();
                coordinates.InsertRange(0, route1.Data.Coordinates);
            }
            else if (Enumerable.SequenceEqual(route1.Data.Coordinates.First() , route2.Data.Coordinates.First()))
            {
                coordinates.AddRange(route1.Data.Coordinates);
                coordinates.Reverse();
                coordinates.AddRange(route2.Data.Coordinates);
            }
            else if (Enumerable.SequenceEqual(route1.Data.Coordinates.First(), route2.Data.Coordinates.Last()))
            {
                coordinates.AddRange(route2.Data.Coordinates);
                coordinates.AddRange(route1.Data.Coordinates);
            }
           
            
            else
            {
                coordinates.AddRange(route1.Data.Coordinates);

                route2.Data.Coordinates = route2.Data.Coordinates.Reverse().ToArray();

                for (int i = 0; i < route2.Data.Coordinates.Length; i++)
                {
                    if(coordinates.Any(x=> Enumerable.SequenceEqual(x, route2.Data.Coordinates[i])))
                        continue;

                    route2.Data.Coordinates = route2.Data.Coordinates.Skip(i == 0? 0 : i-1).ToArray();

                 
                    return MergeTwoRoutes(route1, route2);
                }

            }


            result.Data.Coordinates = coordinates.ToArray();
            result.Info.Length =  Math.Round(GeoJsonFileReader.CalculateLength(result.Data.Coordinates),2);
            return result;
        }

        private Route PathToRoute(List<RouteFeature> path)
        {
            //var a = path.FirstOrDefault(x => x.Feature.Properties["osm_id"] == "195164433");

            return new Route
            {
                Data = new RouteData
                {
                    Type = "LineString",
                    Coordinates = SorthPath(path.Select(x =>
                         x.Feature.Coordinates.Select(y => new[] {y.Longitude, y.Latitude})
                        .ToArray()).ToArray())
                },
                Info = new RouteInfo
                {
                    Length = Math.Round(path.Sum(x => (double)x.Feature.Properties["length"]), 2)
                }
            };
        }

        private double[][] SorthPath(IList<double[][]> path)
        {
            var tempPat = new List<double[][]>(path);

            var lastSubPath = tempPat.First();
            tempPat.Remove(lastSubPath);

            IEnumerable<double[]> coordinates = lastSubPath;


            while (tempPat.Any())
            {
                var newLastSubPath = tempPat.FirstOrDefault(x => Helpers.AreClose(x.First(),coordinates.First()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = newLastSubPath.Reverse().Concat(coordinates);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => Helpers.AreClose(x.First() , coordinates.Last()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = coordinates.Concat(newLastSubPath);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => Helpers.AreClose(x.Last() , coordinates.First()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = newLastSubPath.Concat(coordinates);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => Helpers.AreClose(x.Last(), coordinates.Last()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = coordinates.Concat(newLastSubPath.Reverse());

                    continue;
                }

                throw new Exception("Something went wrong at path sorting");
            }

            //foreach (var f in path.Skip(1))
            //    if (AreClose(last, f.First()))
            //    {
            //        coordinates.AddRange(f);
            //        last = f.Last();
            //    }
            //    else if (AreClose(last, f.Last())) //apversti
            //    {
            //        coordinates.AddRange(f.Reverse());
            //        last = f.First();
            //    }
            //    else //jei reikia deti i prieki
            //    {
            //        coordinates = AreClose(coordinates.First(), f.Last()) ? f.Concat(coordinates.ToArray()).ToList() : f.Reverse().Concat(coordinates.ToArray()).ToList();
            //    }

            return coordinates.ToArray();
        }

      

       

        private RouteFeature FindClosetFeature(Coordinate coordinate)
        {
            var closet = _routeFeatures.First();
            var dist = Helpers.CalcualteDistanceToFeature(closet, coordinate);

            foreach (var f in _routeFeatures.Skip(1))
            {
                var newDistance = Helpers.CalcualteDistanceToFeature(f, coordinate);

                if (newDistance < dist)
                {
                    dist = newDistance;
                    closet = f;
                }
            }

            return closet;
        }       
    }
}