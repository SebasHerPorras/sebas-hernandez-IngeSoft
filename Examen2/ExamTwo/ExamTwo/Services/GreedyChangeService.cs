using ExamTwo.Interfaces;

namespace ExamTwo.Services
{
    public class GreedyChangeService : IChangeStrategy
    {
        public bool Calculate(int amount, Dictionary<int, int> availableCoins, out Dictionary<int, int> changeBreakdown)
        {
            changeBreakdown = new Dictionary<int, int>();
            int remainingAmount = amount;

            // Sort coins in descending order (greedy approach)
            var sortedCoins = availableCoins.Keys.OrderByDescending(c => c).ToList();

            foreach (var coinValue in sortedCoins)
            {
                if (remainingAmount <= 0)
                    break;

                // Calculate how many coins of this denomination we can use
                int coinsNeeded = remainingAmount / coinValue;
                int coinsAvailable = availableCoins[coinValue];
                int coinsToUse = Math.Min(coinsNeeded, coinsAvailable);

                if (coinsToUse > 0)
                {
                    changeBreakdown[coinValue] = coinsToUse;
                    remainingAmount -= coinValue * coinsToUse;
                }
            }

            // Return true only if we managed to give exact change
            return remainingAmount == 0;
        }
    }
}
