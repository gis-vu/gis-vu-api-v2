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
            var startGridCell = FindGridCell(start);
            var endGridCell = FindGridCell(end);

            var gridCells = new List<GridCell>();

            foreach (var p in intermediates)
            {
                gridCells.Add(FindGridCell(p));
            }

            var cellToFeatures = new Dictionary<string,CellData>();
            cellToFeatures[startGridCell.Index] = ReadCellData(startGridCell.Index);

            var startFeature = FindClosetFeature(start, cellToFeatures[startGridCell.Index].Features);

            RouteFeature endFeature;

            if (!cellToFeatures.ContainsKey(endGridCell.Index))
            {
                cellToFeatures[endGridCell.Index] = ReadCellData(endGridCell.Index);
                UpdateNeighbours(cellToFeatures, endGridCell.Index);
            }

            endFeature = FindClosetFeature(end, cellToFeatures[endGridCell.Index].Features);


            var intermediateFeatures = new List<RouteFeature>();

            foreach (var p in intermediates)
            {
                var cell = FindGridCell(end);

                if (!cellToFeatures.ContainsKey(cell.Index))
                {
                    cellToFeatures[cell.Index] = ReadCellData(cell.Index);
                    UpdateNeighbours(cellToFeatures, cell.Index);
                }

                intermediateFeatures.Add(FindClosetFeature(p, cellToFeatures[cell.Index].Features));
            }


            return new LoadedData()
            {
                StartFeature = startFeature,
                EndFeature = endFeature,
                IntermediateFeatures = intermediateFeatures.ToArray(),
                AllFeatures = cellToFeatures.Values.SelectMany(x=> x.Features).ToArray()
            };
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