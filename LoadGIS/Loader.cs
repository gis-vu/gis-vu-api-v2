using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Helpers;
using Models;

namespace LoadGIS
{
    public class Loader : ILoader
    {
        private readonly string _pathToData;
        private GridCell[] _grid;

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


        public LoadedData Load(PointPosition start, PointPosition end, PointPosition[] intermediates)
        {
            var cellSequence = new List<GridCell>();

            var startGridCell = FindGridCell(start);
            var endGridCell = FindGridCell(end);

            //var gridCells = new List<GridCell>();

            //foreach (var p in intermediates)
            //{
            //    gridCells.Add(FindGridCell(p));
            //}

            cellSequence.Add(startGridCell);

            var cellToFeatures = new Dictionary<string,CellData>();
            cellToFeatures[startGridCell.Index] = ReadCellData(startGridCell.Index);

            if (!cellToFeatures.ContainsKey(endGridCell.Index))
            {
                cellToFeatures[endGridCell.Index] = ReadCellData(endGridCell.Index);
                UpdateNeighbours(cellToFeatures, endGridCell.Index);
            }
            
            var intermediateFeatures = new List<RouteFeature>();

            foreach (var p in intermediates)
            {
                var cell = FindGridCell(p);

                cellSequence.Add(cell);

                if (!cellToFeatures.ContainsKey(cell.Index))
                {
                    cellToFeatures[cell.Index] = ReadCellData(cell.Index);
                    UpdateNeighbours(cellToFeatures, cell.Index);
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
                    UpdateNeighbours(cellToFeatures, c.Index);
                }
            }


            return new LoadedData()
            {
                StartFeature = FindClosetFeature(start, cellToFeatures[startGridCell.Index].Features),
                EndFeature = FindClosetFeature(end, cellToFeatures[endGridCell.Index].Features),
                IntermediateFeatures = intermediateFeatures.ToArray(),
                AllFeatures = cellToFeatures.Values.SelectMany(x=> x.Features).ToArray()
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
            var minX = cellCoordinates.Select(x => x.Item1).Min();

            var maxY = cellCoordinates.Select(x => x.Item2).Max();
            var minY = cellCoordinates.Select(x => x.Item2).Min();

            var result = new List<GridCell>();

            for (int i = minX; i <= maxX; i++)
            {
                for (int j = minY; j <= maxY; j++)
                {
                    result.Add(_grid.First(x=>int.Parse(x.Index) == j * 38 + i));
                }
            }


            return result.ToArray();

        }

        private GridCell[] GetTempGridCells2(GridCell[] gridCells)
        {

            var cellsVertice = gridCells.Select(x => x.Border.Select(y=>y.ToDoubleArray()).ToArray()).ToArray();

            var allLines = new List<Tuple<double[],double[]>[]>();

            for (int i = 0; i < cellsVertice.Length - 1; i++)
            {
                var c1 = cellsVertice[i];
                var c2 = cellsVertice[i+1];

                var lines = new List<Tuple<double[],double[]>>();

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

                    if(endPoint == null)
                        throw new Exception("smth went wrong");

                    lines.Add(new Tuple<double[], double[]>(startPoint, endPoint));
                }

                allLines.Add(lines.ToArray());
            }

            return null;
        }

        private void UpdateNeighbours(Dictionary<string, CellData> cellToFeatures, string cellIndex)
        {
            Console.WriteLine("Updating feature neigbhours");

            double amount = 0, temp = 0, all = cellToFeatures[cellIndex].BorderFeatures.Length;

            var newFeatures = cellToFeatures[cellIndex].BorderFeatures;

            var borderFeatures = cellToFeatures.Where(x => x.Key != cellIndex).SelectMany(x => x.Value.BorderFeatures).ToArray();

            for (var i = 0; i < all; i++)
            {
                amount++;
                temp++;
                if (temp > all / 100)
                {
                    Console.WriteLine(Math.Round(amount / all * 100, 2));
                    temp = 0;
                }

                for (var j = 0; j < borderFeatures.Length; j++)
                    if (DistanceHelpers.AreNeighbours(
                        newFeatures[i].Data.Coordinates.Select(x=>x.ToDoubleArray()).ToArray(),
                        borderFeatures[j].Data.Coordinates.Select(x=>x.ToDoubleArray()).ToArray()))
                    {
                        newFeatures[i].Neighbours.Add(borderFeatures[j]);
                        borderFeatures[j].Neighbours.Add(newFeatures[i]);
                    }
            }
        }

        private CellData ReadCellData(string index)
        {
            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(_pathToData + index + ".txt", FileMode.Open))
            {
                return (CellData)formatter.Deserialize(fileStream);
            }
        }

        private GridCell FindGridCell(PointPosition requestStart)
        {
            foreach (var g in _grid)
            {
                if (DistanceHelpers.IsInside(requestStart.ToDoubleArray(), g.Border.Select(x => x.ToDoubleArray()).ToArray()))
                    return g;
            }

            throw new Exception();
        }

        private RouteFeature FindClosetFeature(PointPosition p, RouteFeature[] features)
        {
            var closet = features.First();
            var dist = DistanceHelpers.CalcualteDistanceToFeature(
                closet.Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray(), p.ToDoubleArray());

            foreach (var f in features.Skip(1))
            {
                var newDistance = DistanceHelpers.CalcualteDistanceToFeature(f.Data.Coordinates.Select(x => x.ToDoubleArray()).ToArray(), p.ToDoubleArray());

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