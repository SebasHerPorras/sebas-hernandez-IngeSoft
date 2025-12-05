using FluentAssertions;
using Moq;
using ExamTwo.Services;
using ExamTwo.Interfaces;
using ExamTwo.Models;
using Microsoft.Extensions.Logging;

namespace UnitTestExamen2
{
    [TestFixture]
    public class CoffeeOrderServiceTests
    {
        private Mock<IInventoryService> _mockInventoryService;
        private Mock<IChangeCalculatorService> _mockChangeCalculatorService;
        private Mock<ILogger<CoffeeOrderService>> _mockLogger;
        private CoffeeOrderService _service;

        [SetUp]
        public void Setup()
        {
            _mockInventoryService = new Mock<IInventoryService>();
            _mockChangeCalculatorService = new Mock<IChangeCalculatorService>();
            _mockLogger = new Mock<ILogger<CoffeeOrderService>>();
            
            _service = new CoffeeOrderService(
                _mockInventoryService.Object,
                _mockChangeCalculatorService.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act1 = () => new CoffeeOrderService(null, _mockChangeCalculatorService.Object, _mockLogger.Object);
            Action act2 = () => new CoffeeOrderService(_mockInventoryService.Object, null, _mockLogger.Object);
            
            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public async Task ProcessOrderAsync_WithFailureScenarios_ReturnsCorrectErrors()
        {
            // Arrange - validación de stock falla
            var invalidStockOrder = new OrderRequestDto
            {
                Order = new Dictionary<string, int> { { "Espresso", 100 } },
                Payment = new PaymentDto { TotalAmount = 5000 }
            };
            _mockInventoryService.Setup(s => s.ValidateStock(It.IsAny<Dictionary<string, int>>(), out It.Ref<string>.IsAny))
                .Returns((Dictionary<string, int> order, out string error) =>
                {
                    error = "Insufficient stock";
                    return false;
                });

            // Act & Assert - stock inválido
            var result1 = await _service.ProcessOrderAsync(invalidStockOrder);
            result1.Success.Should().BeFalse();
            result1.Message.Should().Be("Insufficient stock");

            // Arrange - pago insuficiente
            var insufficientPaymentOrder = new OrderRequestDto
            {
                Order = new Dictionary<string, int> { { "Espresso", 2 } },
                Payment = new PaymentDto { TotalAmount = 50 }
            };
            _mockInventoryService.Setup(s => s.ValidateStock(It.IsAny<Dictionary<string, int>>(), out It.Ref<string>.IsAny))
                .Returns((Dictionary<string, int> order, out string error) =>
                {
                    error = "";
                    return true;
                });
            _mockInventoryService.Setup(s => s.GetPrices()).Returns(new Dictionary<string, int> { { "Espresso", 50 } });

            // Act & Assert - pago insuficiente
            var result2 = await _service.ProcessOrderAsync(insufficientPaymentOrder);
            result2.Success.Should().BeFalse();
            result2.Message.Should().Contain("Insufficient payment");
            result2.TotalCost.Should().Be(100);

            // Arrange - precio no disponible
            var noPriceOrder = new OrderRequestDto
            {
                Order = new Dictionary<string, int> { { "Espresso", 1 } },
                Payment = new PaymentDto { TotalAmount = 100 }
            };
            _mockInventoryService.Setup(s => s.GetPrices()).Returns(new Dictionary<string, int>());

            // Act & Assert - precio no disponible
            var result3 = await _service.ProcessOrderAsync(noPriceOrder);
            result3.Success.Should().BeFalse();
            result3.Message.Should().Contain("Price information not available");
        }

        [Test]
        public async Task ProcessOrderAsync_WithValidOrder_CalculatesTotalAndChangeCorrectly()
        {
            // Arrange
            var orderRequest = new OrderRequestDto
            {
                Order = new Dictionary<string, int> 
                { 
                    { "Espresso", 3 },
                    { "Latte", 2 },
                    { "Cappuccino", 1 }
                },
                Payment = new PaymentDto { TotalAmount = 500 }
            };

            var prices = new Dictionary<string, int>
            {
                { "Espresso", 50 },
                { "Latte", 75 },
                { "Cappuccino", 70 }
            };

            var expectedBreakdown = new Dictionary<int, int> { { 50, 2 }, { 25, 1 }, { 5, 1 } };

            _mockInventoryService.Setup(s => s.ValidateStock(It.IsAny<Dictionary<string, int>>(), out It.Ref<string>.IsAny))
                .Returns((Dictionary<string, int> order, out string error) =>
                {
                    error = "";
                    return true;
                });
            _mockInventoryService.Setup(s => s.GetPrices()).Returns(prices);
            _mockInventoryService.Setup(s => s.GetInventory()).Returns(new Dictionary<string, int>());

            _mockChangeCalculatorService.Setup(s => s.CalculateChange(130, out It.Ref<Dictionary<int, int>>.IsAny, out It.Ref<string>.IsAny))
                .Returns((int amount, out Dictionary<int, int> breakdown, out string error) =>
                {
                    breakdown = expectedBreakdown;
                    error = "";
                    return true;
                });

            // Act
            var result = await _service.ProcessOrderAsync(orderRequest);

            // Assert - verifica cálculo: 3*50 + 2*75 + 1*70 = 370, cambio = 500-370 = 130
            result.Success.Should().BeTrue();
            result.TotalCost.Should().Be(370);
            result.ChangeAmount.Should().Be(130);
            result.ChangeBreakdown.Should().BeEquivalentTo(expectedBreakdown);
            _mockInventoryService.Verify(s => s.UpdateStock(orderRequest.Order), Times.Once);
        }

        [Test]
        public async Task ProcessOrderAsync_WithChangeCalculationFailure_RollsBackStock()
        {
            // Arrange
            var orderRequest = new OrderRequestDto
            {
                Order = new Dictionary<string, int> { { "Espresso", 2 } },
                Payment = new PaymentDto { TotalAmount = 125 }
            };

            _mockInventoryService.Setup(s => s.ValidateStock(It.IsAny<Dictionary<string, int>>(), out It.Ref<string>.IsAny))
                .Returns((Dictionary<string, int> order, out string error) =>
                {
                    error = "";
                    return true;
                });
            _mockInventoryService.Setup(s => s.GetPrices()).Returns(new Dictionary<string, int> { { "Espresso", 50 } });
            _mockInventoryService.Setup(s => s.GetInventory()).Returns(new Dictionary<string, int>());

            _mockChangeCalculatorService.Setup(s => s.CalculateChange(It.IsAny<int>(), out It.Ref<Dictionary<int, int>>.IsAny, out It.Ref<string>.IsAny))
                .Returns((int amount, out Dictionary<int, int> breakdown, out string error) =>
                {
                    breakdown = new Dictionary<int, int>();
                    error = "Unable to provide change";
                    return false;
                });

            // Act
            var result = await _service.ProcessOrderAsync(orderRequest);

            // Assert - verifica que se actualizó stock y luego se revirtió (2 llamadas)
            result.Success.Should().BeFalse();
            result.Message.Should().Be("Unable to provide change");
            _mockInventoryService.Verify(s => s.UpdateStock(It.IsAny<Dictionary<string, int>>()), Times.Exactly(2));
        }

        [Test]
        public async Task ProcessOrderAsync_WithExactPayment_HandlesZeroChange()
        {
            // Arrange
            var orderRequest = new OrderRequestDto
            {
                Order = new Dictionary<string, int> { { "Espresso", 2 } },
                Payment = new PaymentDto { TotalAmount = 100 }
            };

            _mockInventoryService.Setup(s => s.ValidateStock(It.IsAny<Dictionary<string, int>>(), out It.Ref<string>.IsAny))
                .Returns((Dictionary<string, int> order, out string error) =>
                {
                    error = "";
                    return true;
                });
            _mockInventoryService.Setup(s => s.GetPrices()).Returns(new Dictionary<string, int> { { "Espresso", 50 } });
            _mockInventoryService.Setup(s => s.GetInventory()).Returns(new Dictionary<string, int>());

            _mockChangeCalculatorService.Setup(s => s.CalculateChange(0, out It.Ref<Dictionary<int, int>>.IsAny, out It.Ref<string>.IsAny))
                .Returns((int amount, out Dictionary<int, int> breakdown, out string error) =>
                {
                    breakdown = new Dictionary<int, int>();
                    error = "";
                    return true;
                });

            // Act
            var result = await _service.ProcessOrderAsync(orderRequest);

            // Assert
            result.Success.Should().BeTrue();
            result.ChangeAmount.Should().Be(0);
            _mockChangeCalculatorService.Verify(s => s.CalculateChange(0, out It.Ref<Dictionary<int, int>>.IsAny, out It.Ref<string>.IsAny), Times.Once);
        }
    }
}
