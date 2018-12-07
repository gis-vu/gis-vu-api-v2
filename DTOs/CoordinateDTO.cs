namespace DTOs
{
    public class CoordinateDTO
    {
        public double Lng { get; set; }
        public double Lat { get; set; }


        public double[] ToDoubleArray()
        {
            return new[] { Lng, Lat };
        }
    }
}
