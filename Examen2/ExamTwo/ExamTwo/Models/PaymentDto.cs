using System.ComponentModel.DataAnnotations;

namespace ExamTwo.Models
{
    public class PaymentDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Total amount must be greater than 0")]
        public int TotalAmount { get; set; }

        public List<int> Coins { get; set; } = new();

        public List<int> Bills { get; set; } = new();
    }
}
