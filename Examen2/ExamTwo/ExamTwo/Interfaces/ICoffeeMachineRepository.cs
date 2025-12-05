namespace ExamTwo.Interfaces
{
    public interface ICoffeeMachineRepository
    {
        Dictionary<string, int> GetCoffeeInventory();
        Dictionary<string, int> GetCoffeePrices();
        Dictionary<int, int> GetAvailableChange();
        void UpdateCoffeeStock(string coffeeName, int quantity);
        void UpdateChangeStock(Dictionary<int, int> change);
        bool HasSufficientStock(string coffeeName, int quantity);
    }
}
