namespace ExamTwo.Models
{
    public class OrderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int TotalCost { get; set; }
        public int ChangeAmount { get; set; }
        public Dictionary<int, int>? ChangeBreakdown { get; set; }
    }
}
