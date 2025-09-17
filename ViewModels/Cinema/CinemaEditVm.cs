namespace Project02.ViewModels.Cinema
{
    public class CinemaEditVm
    {
        public long Cinema_ID { get; set; }
        public string Cinema_Name { get; set; } = default!;
        public string Location { get; set; } = default!;
        public string Contact_Info { get; set; } = default!;
    }
}
