using ExamTwo.Models;
using ExamTwo.Interfaces;

namespace ExamTwo.Services
{
    public class CoffeeOrderService : ICoffeeOrderService
    {
        private readonly IInventoryService _inventoryService;
        private readonly IChangeCalculatorService _changeCalculatorService;
        private readonly ILogger<CoffeeOrderService> _logger;

        public CoffeeOrderService(
            IInventoryService inventoryService,
            IChangeCalculatorService changeCalculatorService,
            ILogger<CoffeeOrderService> logger)
        {
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _changeCalculatorService = changeCalculatorService ?? throw new ArgumentNullException(nameof(changeCalculatorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<OrderResultDto> ProcessOrderAsync(OrderRequestDto orderRequest)
        {
            _logger.LogInformation("Processing order with {ItemCount} items", orderRequest.Order.Count);

            // Step 1: Validate stock
            if (!_inventoryService.ValidateStock(orderRequest.Order, out string stockError))
            {
                _logger.LogWarning("Order validation failed: {Error}", stockError);
                return Task.FromResult(new OrderResultDto
                {
                    Success = false,
                    Message = stockError
                });
            }

            // Step 2: Calculate total cost
            var prices = _inventoryService.GetPrices();
            int totalCost = 0;

            try
            {
                totalCost = orderRequest.Order.Sum(item => prices[item.Key] * item.Value);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, "Price not found for one of the ordered items");
                return Task.FromResult(new OrderResultDto
                {
                    Success = false,
                    Message = "Price information not available for one or more items"
                });
            }

            // Step 3: Validate payment
            if (orderRequest.Payment.TotalAmount < totalCost)
            {
                _logger.LogWarning("Insufficient payment. Required: {Required}, Provided: {Provided}", 
                    totalCost, orderRequest.Payment.TotalAmount);
                return Task.FromResult(new OrderResultDto
                {
                    Success = false,
                    Message = $"Insufficient payment. Required: {totalCost}, Provided: {orderRequest.Payment.TotalAmount}",
                    TotalCost = totalCost
                });
            }

            // Step 4: Reserve stock (update inventory temporarily)
            var originalInventory = _inventoryService.GetInventory();
            _inventoryService.UpdateStock(orderRequest.Order);

            // Step 5: Calculate change
            int changeAmount = orderRequest.Payment.TotalAmount - totalCost;
            bool changeSuccess = _changeCalculatorService.CalculateChange(
                changeAmount, 
                out Dictionary<int, int> changeBreakdown, 
                out string changeError);

            if (!changeSuccess)
            {
                // Rollback: Restore original inventory
                _logger.LogWarning("Change calculation failed, rolling back stock changes");
                foreach (var item in orderRequest.Order)
                {
                    _inventoryService.UpdateStock(new Dictionary<string, int> 
                    { 
                        { item.Key, -item.Value } // Add back the quantities
                    });
                }

                return Task.FromResult(new OrderResultDto
                {
                    Success = false,
                    Message = changeError,
                    TotalCost = totalCost,
                    ChangeAmount = changeAmount
                });
            }

            // Step 6: Success - return result
            _logger.LogInformation("Order processed successfully. Total: {Total}, Change: {Change}", 
                totalCost, changeAmount);

            return Task.FromResult(new OrderResultDto
            {
                Success = true,
                Message = "Order processed successfully",
                TotalCost = totalCost,
                ChangeAmount = changeAmount,
                ChangeBreakdown = changeBreakdown
            });
        }
    }
}
