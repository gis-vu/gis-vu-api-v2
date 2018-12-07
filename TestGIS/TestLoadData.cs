using System;
using System.Collections.Generic;
using System.Text;
using DTOs;
using LoadGIS;
using Xunit;

namespace TestGIS
{
    public class TestLoadData
    {
        [Fact]
        public void LoadFeaturesBetweenTwoPoints()
        {
            var loader = new Loader(
                @"C:\Users\daini\Desktop\GIS\Projektas\Projektas.V2\API2\GIS.VU.API\Data\grid.txt",
                @"C:\Users\daini\Desktop\GIS\Projektas\Projektas.V2\API2\GIS.VU.API\Data\");


            var loadedDataBetweenTwoPoints = loader.LoadDataBetweenTwoPoints(
                new CoordinateDTO()
                {
                    Lng = 25.386275646947524,
                    Lat = 56.009083498783212
                }.ToDoubleArray(),
                new CoordinateDTO()
                {
                    Lng = 25.359882641624324,
                    Lat = 55.989464314910151
                }.ToDoubleArray());
        }
    }
}
