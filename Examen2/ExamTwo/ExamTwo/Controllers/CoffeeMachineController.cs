using Microsoft.AspNetCore.Mvc;
using ExamTwo.Models;
using ExamTwo.Interfaces;

namespace ExamTwo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CoffeeMachineController : ControllerBase
    {
        private readonly ICoffeeOrderService _orderService;
        private readonly IInventoryService _inventoryService;
        private readonly IChangeCalculatorService _changeCalculatorService;
        private readonly ILogger<CoffeeMachineController> _logger;

        public CoffeeMachineController(
            ICoffeeOrderService orderService,
            IInventoryService inventoryService,
            IChangeCalculatorService changeCalculatorService,
            ILogger<CoffeeMachineController> logger)
        {
            _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _changeCalculatorService = changeCalculatorService ?? throw new ArgumentNullException(nameof(changeCalculatorService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet("inventory")]
        public ActionResult<InventoryResponseDto> GetInventory()
        {
            _logger.LogInformation("Getting coffee inventory");
            var inventory = _inventoryService.GetInventory();
            return Ok(new InventoryResponseDto { CoffeeInventory = inventory });
        }

        [HttpGet("prices")]
        public ActionResult<PriceListResponseDto> GetPrices()
        {
            _logger.LogInformation("Getting coffee prices");
            var prices = _inventoryService.GetPrices();
            return Ok(new PriceListResponseDto { CoffeePrices = prices });
        }

        [HttpGet("change")]
        public ActionResult<ChangeResponseDto> GetAvailableChange()
        {
            _logger.LogInformation("Getting available change");
            var change = _changeCalculatorService.GetAvailableChange();
            return Ok(new ChangeResponseDto { AvailableChange = change });
        }

        [HttpPost("orders")]
        public async Task<ActionResult<OrderResultDto>> PlaceOrder([FromBody] OrderRequestDto request)
        {
            _logger.LogInformation("Received order request");

            // Validate model state (DataAnnotations validation)
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for order request");
                return BadRequest(new OrderResultDto
                {
                    Success = false,
                    Message = "Invalid request data"
                });
            }

            try
            {
                // Delegate to service layer
                var result = await _orderService.ProcessOrderAsync(request);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                // Format success message with change breakdown
                if (result.ChangeAmount > 0 && result.ChangeBreakdown != null)
                {
                    var changeDetails = string.Join(", ", 
                        result.ChangeBreakdown.Select(c => $"{c.Value} coin(s) of {c.Key}"));
                    result.Message = $"Order successful! Your change is {result.ChangeAmount} colones. Breakdown: {changeDetails}";
                }
                else
                {
                    result.Message = "Order successful! Exact payment received.";
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing order");
                return StatusCode(StatusCodes.Status500InternalServerError, new OrderResultDto
                {
                    Success = false,
                    Message = "An error occurred while processing your order"
                });
            }
        }
    }
}
