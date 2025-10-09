namespace Project02.ViewModels.Customer
{
    public class UserProfileVm
    {
        public Project02.Models.User User { get; set; }
        public List<Project02.Models.Order> Orders { get; set; }
        public List<Project02.Models.OrderSeat> Seat { get; set; }
    }

}
