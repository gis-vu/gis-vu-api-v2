namespace DTOs
{
    public class RouteSearchRequestDTO
    {
        public CoordinateDTO Start { get; set; }
        public CoordinateDTO End { get; set; }
        public CoordinateDTO[] Points { get; set; }
        public SearchOptionsDTO SearchOptions { get; set; }
    }
}
