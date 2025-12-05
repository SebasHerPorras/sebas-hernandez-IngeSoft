using ExamTwo.Data;
using ExamTwo.Interfaces;

namespace ExamTwo.Repositories
{
    public class CoffeeMachineRepository : ICoffeeMachineRepository
    {
        private readonly Database _database;

        public CoffeeMachineRepository(Database database)
        {
            _database = database ?? throw new ArgumentNullException(nameof(database));
        }

        public Dictionary<string, int> GetCoffeeInventory()
        {
            return new Dictionary<string, int>(_database.keyValues);
        }

        public Dictionary<string, int> GetCoffeePrices()
        {
            return new Dictionary<string, int>(_database.keyValues2);
        }

        public Dictionary<int, int> GetAvailableChange()
        {
            return new Dictionary<int, int>(_database.keyValues3);
        }

        public void UpdateCoffeeStock(string coffeeName, int quantity)
        {
            if (!_database.keyValues.ContainsKey(coffeeName))
            {
                throw new ArgumentException($"Coffee '{coffeeName}' not found in inventory");
            }

            _database.keyValues[coffeeName] = quantity;
        }

        public void UpdateChangeStock(Dictionary<int, int> change)
        {
            foreach (var coin in change)
            {
                if (_database.keyValues3.ContainsKey(coin.Key))
                {
                    _database.keyValues3[coin.Key] = coin.Value;
                }
            }
        }

        public bool HasSufficientStock(string coffeeName, int quantity)
        {
            return _database.keyValues.ContainsKey(coffeeName) && 
                   _database.keyValues[coffeeName] >= quantity;
        }
    }
}
