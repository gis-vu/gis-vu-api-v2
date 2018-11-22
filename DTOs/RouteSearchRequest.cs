namespace DTOs
{
    public class RouteSearchRequest
    {
        public Coordinate Start { get; set; }
        public Coordinate End { get; set; }
        public Coordinate[] Points { get; set; }
        public SearchOptions SearchOptions { get; set; }
    }
}
