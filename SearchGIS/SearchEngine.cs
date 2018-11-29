using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DTOs;
using GIS.VU.API;
using Helpers;
using LoadGIS;
using Models;

namespace SearchGIS
{
    public class SearchEngine
    {
        private ILoader Loader { get; }

        public SearchEngine(ILoader loader)
        {
            Loader = loader;
        }

        public RouteSearchResponseDTO FindRoute(RouteSearchRequestDTO request)
        {
            var loadedData = Loader.Load(
                new PointPosition()
                {
                    Latitude = request.Start.Lat,
                    Longitude = request.Start.Lng
                },
                new PointPosition()
                {
                    Latitude = request.End.Lat,
                    Longitude = request.End.Lng
                }, 
                request.Points.Select(x => new PointPosition()
                {
                    Latitude = x.Lat,
                    Longitude = x.Lng
                }).ToArray()
               );

            //var pointFeature = request.Points.Length == 0 ? null : FindClosetFeature(request.Points.FirstOrDefault());

            if (request.Points.Length == 0)
            {
                //var g1 = new Graph(_routeFeature2s, null);
                //var path = g1.FindShortestPath(startFeature2, endFeature2, null);

                //if (path == null)
                //   return new RouteSearchResponse(Array.Empty<Route>());

                // var route1 = PathToRoute(path);

                var g2 = new Graph(loadedData.AllFeatures, request.SearchOptions);
                var path2 = g2.FindShortestPath(loadedData.StartFeature, loadedData.EndFeature, null);

                var route2 = PathToRoute(path2);

                return new RouteSearchResponseDTO(new[] {route2});
            }
            else
            {
                //var g1 = new Graph(_routeFeature2s, null);
                //var path1 = g1.FindShortestPath(startFeature2, pointFeature, null);

                //if (path1 == null)
                //    return new RouteSearchResponse(Array.Empty<Route>());

                //var path2 = g1.FindShortestPath(pointFeature, endFeature2, null);
                //if (path2 == null)
                //    return new RouteSearchResponse(Array.Empty<Route>());

                //var route1 = PathToRoute(path1);
                //var route2 = PathToRoute(path2);
                //var r1 = MergeTwoRoutes(route1, route2);

                var g2 = new Graph(loadedData.AllFeatures, request.SearchOptions);

                var data1 = FeaturesToGeojsonHelper.ToGeojson(new double[][][] {loadedData.StartFeature.Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray()});
                var data2 = FeaturesToGeojsonHelper.ToGeojson(new double[][][] {loadedData.IntermediateFeatures.First().Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray()});

                var path3 = g2.FindShortestPath(loadedData.StartFeature, loadedData.IntermediateFeatures.First(), null);
                var route3 = PathToRoute(path3);

                for (var i=1; i < loadedData.IntermediateFeatures.Length; i++)
                {
                    var path4 = g2.FindShortestPath(loadedData.IntermediateFeatures[i-1], loadedData.IntermediateFeatures[i], null);
                    var route4 = PathToRoute(path4);
                    route3 = MergeTwoRoutes(route3, route4);
                }

                var path5 = g2.FindShortestPath(loadedData.IntermediateFeatures.Last(), loadedData.EndFeature, null);
                var route5 = PathToRoute(path5);
                route3 = MergeTwoRoutes(route3, route5);


                return new RouteSearchResponseDTO(new[] {route3});
            }
        }

        private RouteDTO MergeTwoRoutes(RouteDTO route1, RouteDTO route2)
        {
            var data = new RouteDataDTO
            {
                Type = route1.Data.Type
            };
            var info = new RouteInfoDTO
            {
                Length = route1.Info.Length + route2.Info.Length
            };

            var result = new RouteDTO
            {
                Data = data,
                Info = info
            };

            var coordinates = new List<double[]>();

            if (route1.Data.Coordinates.Last().SequenceEqual(route2.Data.Coordinates.First()))
            {
                coordinates.AddRange(route1.Data.Coordinates);
                coordinates.AddRange(route2.Data.Coordinates);
            }
            else if (route1.Data.Coordinates.Last().SequenceEqual(route2.Data.Coordinates.Last()))
            {
                coordinates.AddRange(route2.Data.Coordinates);
                coordinates.Reverse();
                coordinates.InsertRange(0, route1.Data.Coordinates);
            }
            else if (route1.Data.Coordinates.First().SequenceEqual(route2.Data.Coordinates.First()))
            {
                coordinates.AddRange(route1.Data.Coordinates);
                coordinates.Reverse();
                coordinates.AddRange(route2.Data.Coordinates);
            }
            else if (route1.Data.Coordinates.First().SequenceEqual(route2.Data.Coordinates.Last()))
            {
                coordinates.AddRange(route2.Data.Coordinates);
                coordinates.AddRange(route1.Data.Coordinates);
            }


            else
            {
                throw new Exception("WIP");
                coordinates.AddRange(route1.Data.Coordinates);

                //route2.Data.Coordinates = route2.Data.Coordinates.Reverse().ToArray();

                //for (var i = 0; i < route2.Data.Coordinates.Length; i++)
                //{
                //    if (coordinates.Any(x => x.SequenceEqual(route2.Data.Coordinates[i])))
                //        continue;

                //    route2.Data.Coordinates = route2.Data.Coordinates.Skip(i == 0 ? 0 : i - 1).ToArray();


                //    return MergeTwoRoutes(route1, route2);
                //}
            }


            result.Data.Coordinates = coordinates.ToArray();
            result.Info.Length = 0;
            ///Math.Round(GeoJsonFileReader.CalculateLength(result.Data.Coordinates),2);
            return result;
        }

        private RouteDTO PathToRoute(List<RouteFeature> path)
        {
            //var a = path.FirstOrDefault(x => x.Feature.Properties["osm_id"] == "195164433");

            return new RouteDTO
            {
                Data = new RouteDataDTO
                {
                    Type = "LineString",
                    Coordinates = SorthPath(path.Select(x =>
                        x.Data.Coordinates.Select(y => new[] {y.Longitude, y.Latitude})
                            .ToArray()).ToArray())
                },
                Info = new RouteInfoDTO
                {
                    Length = Math.Round(path.Sum(x => (double) x.Data.Properties["lenght"]), 2)
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
                var newLastSubPath =
                    tempPat.FirstOrDefault(x => DistanceHelpers.AreClose(x.First(), coordinates.First()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = newLastSubPath.Reverse().Concat(coordinates);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => DistanceHelpers.AreClose(x.First(), coordinates.Last()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = coordinates.Concat(newLastSubPath);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => DistanceHelpers.AreClose(x.Last(), coordinates.First()));
                if (newLastSubPath != null)
                {
                    tempPat.Remove(newLastSubPath);

                    coordinates = newLastSubPath.Concat(coordinates);

                    continue;
                }

                newLastSubPath = tempPat.FirstOrDefault(x => DistanceHelpers.AreClose(x.Last(), coordinates.Last()));
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
    }
}