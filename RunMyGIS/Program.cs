using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ReadMyGIS;

namespace RunMyGIS
{
    class Program
    {
        static void Main(string[] args)
        {

            //var reader = new GeoJsonFileReader();
            //var data = reader.Read(".\\routedata.geojson");


            //var a = new A();
            //a.title = "A";

            //var b = new A();
            //b.title = "b";

            //a.Friends.Add(b);
            //b.Friends.Add(a);

            // Save(data, "data.txt");

            var watch = System.Diagnostics.Stopwatch.StartNew();
            // the code that you want to measure comes here
            var reader = new GeoJsonFileReader();

            var data = reader.Read(".\\routedata.geojson");


            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;
            Save(data, "data.txt");
            //var dsf = Read("data.txt");
        }

        private static object Read(string fileName)
        {
            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(fileName, FileMode.Open))
            {
                return formatter.Deserialize(fileStream);
            }
        }


        private static void Save(object data, string fileName)
        {
            var formatter = new BinaryFormatter();

            using (var fileStream = new FileStream(fileName, FileMode.Create))
            {
                formatter.Serialize(fileStream, data);

            }
        }


        [Serializable]
        private class A
        {
            public string title { get; set; }
            public List<A> Friends { get; set; } = new List<A>();
        }
    }
}
