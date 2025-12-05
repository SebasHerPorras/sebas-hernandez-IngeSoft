using ExamTwo.Interfaces;

namespace ExamTwo.Services
{

    public class InventoryService : IInventoryService
    {
        private readonly ICoffeeMachineRepository _repository;
        private readonly ILogger<InventoryService> _logger;

        public InventoryService(ICoffeeMachineRepository repository, ILogger<InventoryService> logger)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool ValidateStock(Dictionary<string, int> order, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (order == null || order.Count == 0)
            {
                errorMessage = "Order is empty";
                return false;
            }

            var inventory = _repository.GetCoffeeInventory();
            var prices = _repository.GetCoffeePrices();

            foreach (var item in order)
            {
                // Check if coffee type exists
                if (!inventory.ContainsKey(item.Key))
                {
                    errorMessage = $"Coffee type '{item.Key}' not found";
                    _logger.LogWarning("Invalid coffee type requested: {CoffeeType}", item.Key);
                    return false;
                }

                // Check if quantity is valid
                if (item.Value <= 0)
                {
                    errorMessage = $"Invalid quantity for {item.Key}";
                    return false;
                }

                // Check if sufficient stock
                if (inventory[item.Key] < item.Value)
                {
                    errorMessage = $"Insufficient stock for {item.Key}. Available: {inventory[item.Key]}, Requested: {item.Value}";
                    _logger.LogWarning("Insufficient stock for {CoffeeType}. Available: {Available}, Requested: {Requested}", 
                        item.Key, inventory[item.Key], item.Value);
                    return false;
                }
            }

            return true;
        }

        public void UpdateStock(Dictionary<string, int> order)
        {
            var inventory = _repository.GetCoffeeInventory();

            foreach (var item in order)
            {
                if (inventory.ContainsKey(item.Key))
                {
                    int newQuantity = inventory[item.Key] - item.Value;
                    _repository.UpdateCoffeeStock(item.Key, newQuantity);
                    _logger.LogInformation("Updated stock for {CoffeeType}: {OldQuantity} -> {NewQuantity}", 
                        item.Key, inventory[item.Key], newQuantity);
                }
            }
        }

        public Dictionary<string, int> GetInventory()
        {
            return _repository.GetCoffeeInventory();
        }

        public Dictionary<string, int> GetPrices()
        {
            return _repository.GetCoffeePrices();
        }
    }
}
