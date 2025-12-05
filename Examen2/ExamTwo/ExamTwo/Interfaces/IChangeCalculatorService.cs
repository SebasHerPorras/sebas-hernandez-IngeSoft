namespace ExamTwo.Interfaces
{
    public interface IChangeCalculatorService
    {
        bool CalculateChange(int changeAmount, out Dictionary<int, int> changeBreakdown, out string errorMessage);
        Dictionary<int, int> GetAvailableChange();
    }
}
