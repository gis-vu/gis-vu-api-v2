using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using DTOs;
using Helpers;
using Models;

namespace LoadGIS
{
    public class Loader : ILoader
    {
        private readonly string _pathToData;
        private GridCell[] _grid;
        private Dictionary<string, CellData> cellToFeatures = new Dictionary<string, CellData>();


        public Loader(string pathToGrid, string pathToData)
        {
            _pathToData = pathToData;

            _pathToData = pathToData;
            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(pathToGrid, FileMode.Open))
            {
                _grid = (GridCell[]) formatter.Deserialize(fileStream);
            }
        }


        public LoadedData Load(LoadRequest request)
        {
            var cellSequence = new List<GridCell>();

            var startGridCell = FindGridCell(request.Start);
            var endGridCell = FindGridCell(request.End);

            //var gridCells = new List<GridCell>();

            //foreach (var p in intermediates)
            //{
            //    gridCells.Add(FindGridCell(p));
            //}

            cellSequence.Add(startGridCell);

            if (!cellToFeatures.ContainsKey(startGridCell.Index))
            {
                cellToFeatures[startGridCell.Index] = ReadCellData(startGridCell.Index);
                UpdateNeighbours(cellToFeatures, startGridCell);
            }

            if (!cellToFeatures.ContainsKey(endGridCell.Index))
            {
                cellToFeatures[endGridCell.Index] = ReadCellData(endGridCell.Index);
                UpdateNeighbours(cellToFeatures, endGridCell);
            }

            var intermediateFeatures = new List<RouteFeature>();

            foreach (var p in request.Intermediates)
            {
                var cell = FindGridCell(p);

                cellSequence.Add(cell);

                if (!cellToFeatures.ContainsKey(cell.Index))
                {
                    cellToFeatures[cell.Index] = ReadCellData(cell.Index);
                    UpdateNeighbours(cellToFeatures, cell);
                }

                intermediateFeatures.Add(FindClosetFeature(p, cellToFeatures[cell.Index].Features));
            }

            cellSequence.Add(endGridCell);

            var tempGridCells = GetTempGridCells(cellSequence.ToArray());

            foreach (var c in tempGridCells)
            {
                if (!cellToFeatures.ContainsKey(c.Index))
                {
                    cellToFeatures[c.Index] = ReadCellData(c.Index);
                    UpdateNeighbours(cellToFeatures, c);
                }
            }


            return new LoadedData()
            {
                StartFeature = FindClosetFeature(request.Start, cellToFeatures[startGridCell.Index].Features),
                EndFeature = FindClosetFeature(request.End, cellToFeatures[endGridCell.Index].Features),
                IntermediateFeatures = intermediateFeatures.ToArray(),
                AllFeatures = cellToFeatures.Values.SelectMany(x => x.Features).ToArray()
            };
        }

        private GridCell[] GetTempGridCells(GridCell[] gridCells)
        {
            var cellCoordinates = gridCells.Select(c =>
            {
                var index = int.Parse(c.Index);
                var y = index / 38;
                var x = index - y * 38;

                return new Tuple<int, int>(x, y);
            });

            var maxX = cellCoordinates.Select(x => x.Item1).Max();

            if (maxX < 37)
                maxX++;

            var minX = cellCoordinates.Select(x => x.Item1).Min();
            if (minX > 0)
                minX--;

            var maxY = cellCoordinates.Select(x => x.Item2).Max();

            if (maxY < 29)
                maxY++;

            var minY = cellCoordinates.Select(x => x.Item2).Min();

            if (minY > 0)
                minY--;

            var result = new List<GridCell>();

            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    result.Add(_grid.First(x => int.Parse(x.Index) == j * 38 + i));
                }
            }


            return result.ToArray();

        }

        private GridCell[] GetTempGridCells2(GridCell[] gridCells)
        {

            var cellsVertice = gridCells.Select(x => x.Border.Select(y => y.ToDoubleArray()).ToArray()).ToArray();

            var allLines = new List<Tuple<double[], double[]>[]>();

            for (int i = 0; i < cellsVertice.Length - 1; i++)
            {
                var c1 = cellsVertice[i];
                var c2 = cellsVertice[i + 1];

                var lines = new List<Tuple<double[], double[]>>();

                for (int j = 0; j < c1.Length; j++)
                {
                    var maxDistance = -1d;
                    var startPoint = c1[j];
                    double[] endPoint = null;

                    for (int k = 0; k < c2.Length; k++)
                    {
                        var distance = Helpers.DistanceHelpers.GetDistance(c1[j], c2[k]);
                        if (distance > maxDistance)
                        {
                            maxDistance = distance;
                            endPoint = c2[k];
                        }
                    }

                    if (endPoint == null)
                        throw new Exception("smth went wrong");

                    lines.Add(new Tuple<double[], double[]>(startPoint, endPoint));
                }

                allLines.Add(lines.ToArray());
            }

            return null;
        }

        //TODO optimize for neigboring cells
        private void UpdateNeighbours(Dictionary<string, CellData> cellToFeatures, GridCell cell)
        {
            //Console.WriteLine("Updating feature neigbhours");


            var neighbourinCells = GetTempGridCells(new[] {cell});


            double amount = 0, temp = 0, all = cellToFeatures[cell.Index].BorderFeatures.Length;

            var newFeatures = cellToFeatures[cell.Index].BorderFeatures;

            var borderFeatures = cellToFeatures.Where(x => x.Key != cell.Index && neighbourinCells.Any(y=>y.Index == x.Key)).SelectMany(x => x.Value.BorderFeatures)
                .ToArray();

            for (var i = 0; i < all; i++)
            {
                amount++;
                temp++;
                if (temp > all / 100)
                {
                    //Console.WriteLine(Math.Round(amount / all * 100, 2));
                    temp = 0;
                }

                for (var j = 0; j < borderFeatures.Length; j++)
                    if (DistanceHelpers.AreNeighbours(
                        newFeatures[i].Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray(),
                        borderFeatures[j].Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray()))
                    {
                        newFeatures[i].Neighbours.Add(borderFeatures[j]);
                        borderFeatures[j].Neighbours.Add(newFeatures[i]);
                    }
            }
        }

        private CellData ReadCellData(string index)
        {
            Console.WriteLine("Read index: " + index);


            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(_pathToData + index + ".txt", FileMode.Open))
            {
                return (CellData) formatter.Deserialize(fileStream);
            }
        }

        private GridCell FindGridCell(double[] requestStart)
        {
            foreach (var g in _grid)
            {
                if (DistanceHelpers.IsInside(requestStart, g.Border.Select(x => x.ToDoubleArray()).ToArray()))
                    return g;
            }

            throw new Exception();
        }

        private RouteFeature FindClosetFeature(double[] p, RouteFeature[] features)
        {
            var closet = features.First();
            var dist = DistanceHelpers.CalcualteDistanceToFeature(
                closet.Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray(), p);

            foreach (var f in features.Skip(1))
            {
                var newDistance =
                    DistanceHelpers.CalcualteDistanceToFeature(
                        f.Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray(), p);

                if (newDistance < dist)
                {
                    dist = newDistance;
                    closet = f;
                }
            }

            return closet;
        }

        public LoadedData2 LoadDataBetweenTwoPoints(double[] start, double[] end, double[][] polygonPoints)
        {
            var cellSequence = new List<GridCell>();

            var startGridCell = FindGridCell(start);
            var endGridCell = FindGridCell(end);

            cellSequence.Add(startGridCell);

            if (startGridCell.Index != endGridCell.Index)
                cellSequence.Add(endGridCell);

            if (!cellToFeatures.ContainsKey(startGridCell.Index))
            {
                cellToFeatures[startGridCell.Index] = ReadCellData(startGridCell.Index);
                UpdateNeighbours(cellToFeatures, startGridCell);
            }

            if (!cellToFeatures.ContainsKey(endGridCell.Index))
            {
                cellToFeatures[endGridCell.Index] = ReadCellData(endGridCell.Index);
                UpdateNeighbours(cellToFeatures, endGridCell);
            }

            var tempGridCells = GetTempGridCells(cellSequence.ToArray());

            foreach (var c in tempGridCells)
            {
                if (!cellToFeatures.ContainsKey(c.Index))
                {
                    cellToFeatures[c.Index] = ReadCellData(c.Index);
                    UpdateNeighbours(cellToFeatures, c);
                }

                cellSequence.Add(c);

            }

            var result = new List<RouteFeature>();

            //double diff = 6000;
            //var count = 0;

            //DistanceHelpers.DistanceBetweenCoordinates();

            foreach (var c in cellSequence)
            {
                //count += cellToFeatures[c.Index].Features.Length; //how much we will save

                result.AddRange(cellToFeatures[c.Index].Features);

                //foreach (var f in cellToFeatures[c.Index].Features)
                //{

                //    var cord = f.Data.Coordinates.Skip(f.Data.Coordinates.Length / 2).FirstOrDefault().ToDoubleArray();

                //    var projection = DistanceHelpers.GetProjectionOnLine(new Tuple<double[], double[]>(start, end), cord);

                //    if (DistanceHelpers.DistanceBetweenCoordinates(projection, cord) < diff)
                //    {
                //        result.Add(f);
                //    }
                //}
            }




            return new LoadedData2()
            {
                StartFeature = FindClosetFeature(start,
                    cellToFeatures[startGridCell.Index].Features),
                EndFeature = FindClosetFeature(end, cellToFeatures[endGridCell.Index].Features),
                AllFeatures = polygonPoints.Length  == 0 ? result.ToArray() : result.Where(x=> DistanceHelpers.IsInside(x.Data.Coordinates.First().ToDoubleArray(), polygonPoints) || DistanceHelpers.IsInside(x.Data.Coordinates.Last().ToDoubleArray(), polygonPoints)).ToArray()
            };
        }
    }
}