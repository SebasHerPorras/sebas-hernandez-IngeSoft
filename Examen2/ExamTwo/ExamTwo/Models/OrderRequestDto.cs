using System.ComponentModel.DataAnnotations;

namespace ExamTwo.Models
{
    public class OrderRequestDto
    {
        [Required(ErrorMessage = "Order is required")]
        public Dictionary<string, int> Order { get; set; } = new();

        [Required(ErrorMessage = "Payment is required")]
        public PaymentDto Payment { get; set; } = new();
    }
}
