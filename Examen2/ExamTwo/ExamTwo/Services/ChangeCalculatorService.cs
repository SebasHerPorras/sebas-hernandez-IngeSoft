using ExamTwo.Interfaces;

namespace ExamTwo.Services
{
    public class ChangeCalculatorService : IChangeCalculatorService
    {
        private readonly ICoffeeMachineRepository _repository;
        private readonly IChangeStrategy _changeStrategy;
        private readonly ILogger<ChangeCalculatorService> _logger;

        public ChangeCalculatorService(
            ICoffeeMachineRepository repository, 
            IChangeStrategy changeStrategy,
            ILogger<ChangeCalculatorService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _changeStrategy = changeStrategy ?? throw new ArgumentNullException(nameof(changeStrategy));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool CalculateChange(int changeAmount, out Dictionary<int, int> changeBreakdown, out string errorMessage)
        {
            changeBreakdown = new Dictionary<int, int>();
            errorMessage = string.Empty;

            if (changeAmount < 0)
            {
                errorMessage = "Change amount cannot be negative";
                return false;
            }

            if (changeAmount == 0)
            {
                _logger.LogInformation("No change needed - exact payment");
                return true;
            }

            var availableCoins = _repository.GetAvailableChange();
            
            // Use the injected strategy to calculate change
            bool success = _changeStrategy.Calculate(changeAmount, availableCoins, out changeBreakdown);

            if (!success)
            {
                errorMessage = $"Unable to provide exact change of {changeAmount}. Insufficient coins in machine.";
                _logger.LogWarning("Unable to calculate change for amount: {Amount}", changeAmount);
                return false;
            }

            // Update the coin inventory
            var updatedCoins = new Dictionary<int, int>(availableCoins);
            foreach (var coin in changeBreakdown)
            {
                updatedCoins[coin.Key] -= coin.Value;
            }
            _repository.UpdateChangeStock(updatedCoins);

            _logger.LogInformation("Change calculated successfully for amount: {Amount}", changeAmount);
            return true;
        }

        public Dictionary<int, int> GetAvailableChange()
        {
            return _repository.GetAvailableChange();
        }
    }
}
