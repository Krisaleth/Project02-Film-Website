using System.ComponentModel.DataAnnotations;

namespace Project02.ViewModels.Hall
{
    public class HallCreateVm
    {
        [Required]
        public long Cinema_ID { get; set; }
        [Required]
        public int Capacity { get; set; }
    }
}
