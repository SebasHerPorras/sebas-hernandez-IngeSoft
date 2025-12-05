using FluentAssertions;
using Moq;
using ExamTwo.Services;
using ExamTwo.Interfaces;
using Microsoft.Extensions.Logging;

namespace UnitTestExamen2
{
    [TestFixture]
    public class InventoryServiceTests
    {
        private Mock<ICoffeeMachineRepository> _mockRepository;
        private Mock<ILogger<InventoryService>> _mockLogger;
        private InventoryService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ICoffeeMachineRepository>();
            _mockLogger = new Mock<ILogger<InventoryService>>();
            
            _service = new InventoryService(_mockRepository.Object, _mockLogger.Object);
        }

        [Test]
        public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act1 = () => new InventoryService(null, _mockLogger.Object);
            Action act2 = () => new InventoryService(_mockRepository.Object, null);
            
            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void ValidateStock_WithInvalidOrders_ReturnsFalseWithErrors()
        {
            // Arrange
            var inventory = new Dictionary<string, int> { { "Espresso", 10 } };
            var prices = new Dictionary<string, int> { { "Espresso", 50 } };
            _mockRepository.Setup(r => r.GetCoffeeInventory()).Returns(inventory);
            _mockRepository.Setup(r => r.GetCoffeePrices()).Returns(prices);

            // Act & Assert - null y vacío
            _service.ValidateStock(null, out var error1).Should().BeFalse();
            error1.Should().Contain("empty");
            
            _service.ValidateStock(new Dictionary<string, int>(), out var error2).Should().BeFalse();
            error2.Should().Contain("empty");
            
            // Act & Assert - café inexistente
            _service.ValidateStock(new Dictionary<string, int> { { "Invalid", 1 } }, out var error3).Should().BeFalse();
            error3.Should().Contain("not found");
            
            // Act & Assert - cantidades inválidas
            _service.ValidateStock(new Dictionary<string, int> { { "Espresso", 0 } }, out var error4).Should().BeFalse();
            _service.ValidateStock(new Dictionary<string, int> { { "Espresso", -5 } }, out var error5).Should().BeFalse();
            error4.Should().Contain("Invalid quantity");
            
            // Act & Assert - stock insuficiente
            _service.ValidateStock(new Dictionary<string, int> { { "Espresso", 15 } }, out var error6).Should().BeFalse();
            error6.Should().Contain("Insufficient stock");
        }

        [Test]
        public void ValidateStock_WithValidOrders_ReturnsTrue()
        {
            // Arrange
            var inventory = new Dictionary<string, int> { { "Espresso", 10 }, { "Latte", 5 } };
            var prices = new Dictionary<string, int> { { "Espresso", 50 }, { "Latte", 75 } };
            _mockRepository.Setup(r => r.GetCoffeeInventory()).Returns(inventory);
            _mockRepository.Setup(r => r.GetCoffeePrices()).Returns(prices);

            // Act & Assert
            _service.ValidateStock(new Dictionary<string, int> { { "Espresso", 2 }, { "Latte", 1 } }, out var error).Should().BeTrue();
            error.Should().BeEmpty();
        }

        [Test]
        public void UpdateStock_CalculatesAndUpdatesCorrectly()
        {
            // Arrange
            var order = new Dictionary<string, int> { { "Espresso", 2 }, { "Latte", 1 } };
            var inventory = new Dictionary<string, int> { { "Espresso", 10 }, { "Latte", 5 } };
            _mockRepository.Setup(r => r.GetCoffeeInventory()).Returns(inventory);

            // Act
            _service.UpdateStock(order);

            // Assert - verifica cálculo correcto: 10-2=8, 5-1=4
            _mockRepository.Verify(r => r.UpdateCoffeeStock("Espresso", 8), Times.Once);
            _mockRepository.Verify(r => r.UpdateCoffeeStock("Latte", 4), Times.Once);
        }

        [Test]
        public void UpdateStock_WithInvalidItems_IgnoresNonExistent()
        {
            // Arrange
            var order = new Dictionary<string, int> { { "InvalidCoffee", 1 } };
            var inventory = new Dictionary<string, int> { { "Espresso", 10 } };
            _mockRepository.Setup(r => r.GetCoffeeInventory()).Returns(inventory);

            // Act
            _service.UpdateStock(order);

            // Assert - no debe actualizar nada
            _mockRepository.Verify(r => r.UpdateCoffeeStock(It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Test]
        public void GetInventoryAndPrices_ReturnRepositoryData()
        {
            // Arrange
            var expectedInventory = new Dictionary<string, int> { { "Espresso", 10 }, { "Latte", 5 } };
            var expectedPrices = new Dictionary<string, int> { { "Espresso", 50 }, { "Latte", 75 } };
            _mockRepository.Setup(r => r.GetCoffeeInventory()).Returns(expectedInventory);
            _mockRepository.Setup(r => r.GetCoffeePrices()).Returns(expectedPrices);

            // Act & Assert
            _service.GetInventory().Should().BeEquivalentTo(expectedInventory);
            _service.GetPrices().Should().BeEquivalentTo(expectedPrices);
        }
    }
}
