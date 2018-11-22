namespace DTOs
{
    public class SearchOptions
    {
        public PropertyImportance[] PropertyImportance { get; set; }
        public PropertyValueImportance[] PropertyValueImportance { get; set; }
        public double TrackOverlapImportance { get; set; }
    }
}