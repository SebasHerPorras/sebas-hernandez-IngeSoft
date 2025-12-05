namespace ExamTwo.Interfaces
{
    public interface IChangeStrategy
    {
        bool Calculate(int amount, Dictionary<int, int> availableCoins, out Dictionary<int, int> changeBreakdown);
    }
}
