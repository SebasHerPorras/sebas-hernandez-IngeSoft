namespace ExamTwo.Interfaces
{
    public interface IInventoryService
    {
        bool ValidateStock(Dictionary<string, int> order, out string errorMessage);
        void UpdateStock(Dictionary<string, int> order);
        Dictionary<string, int> GetInventory();
        Dictionary<string, int> GetPrices();
    }
}
