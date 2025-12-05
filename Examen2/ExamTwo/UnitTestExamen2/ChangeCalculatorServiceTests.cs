using FluentAssertions;
using Moq;
using ExamTwo.Services;
using ExamTwo.Interfaces;
using Microsoft.Extensions.Logging;

namespace UnitTestExamen2
{
    [TestFixture]
    public class ChangeCalculatorServiceTests
    {
        private Mock<ICoffeeMachineRepository> _mockRepository;
        private Mock<IChangeStrategy> _mockStrategy;
        private Mock<ILogger<ChangeCalculatorService>> _mockLogger;
        private ChangeCalculatorService _service;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<ICoffeeMachineRepository>();
            _mockStrategy = new Mock<IChangeStrategy>();
            _mockLogger = new Mock<ILogger<ChangeCalculatorService>>();
            
            _service = new ChangeCalculatorService(
                _mockRepository.Object,
                _mockStrategy.Object,
                _mockLogger.Object
            );
        }

        [Test]
        public void Constructor_WithNullDependencies_ThrowsArgumentNullException()
        {
            // Act & Assert
            Action act1 = () => new ChangeCalculatorService(null, _mockStrategy.Object, _mockLogger.Object);
            Action act2 = () => new ChangeCalculatorService(_mockRepository.Object, null, _mockLogger.Object);
            
            act1.Should().Throw<ArgumentNullException>();
            act2.Should().Throw<ArgumentNullException>();
        }

        [Test]
        public void CalculateChange_WithInvalidAmounts_HandlesCorrectly()
        {
            // Act & Assert - caso negativo
            _service.CalculateChange(-10, out _, out var error1).Should().BeFalse();
            error1.Should().Contain("cannot be negative");
            
            // Act & Assert - caso cero
            _service.CalculateChange(0, out var breakdown, out _).Should().BeTrue();
            breakdown.Should().BeEmpty();
            
            _mockRepository.Verify(r => r.GetAvailableChange(), Times.Never);
        }

        [Test]
        public void CalculateChange_WithSuccessfulCalculation_UpdatesInventoryAndReturnsBreakdown()
        {
            // Arrange
            int changeAmount = 85;
            var availableCoins = new Dictionary<int, int> { { 50, 3 }, { 25, 5 }, { 10, 10 } };
            var expectedBreakdown = new Dictionary<int, int> { { 50, 1 }, { 25, 1 }, { 10, 1 } };
            Dictionary<int, int> capturedUpdatedCoins = null;

            _mockRepository.Setup(r => r.GetAvailableChange()).Returns(availableCoins);
            _mockStrategy.Setup(s => s.Calculate(changeAmount, availableCoins, out It.Ref<Dictionary<int, int>>.IsAny))
                .Returns((int amt, Dictionary<int, int> coins, out Dictionary<int, int> bd) =>
                {
                    bd = expectedBreakdown;
                    return true;
                });
            _mockRepository.Setup(r => r.UpdateChangeStock(It.IsAny<Dictionary<int, int>>()))
                .Callback<Dictionary<int, int>>(coins => capturedUpdatedCoins = coins);

            // Act
            bool result = _service.CalculateChange(changeAmount, out var changeBreakdown, out var errorMessage);

            // Assert
            result.Should().BeTrue();
            errorMessage.Should().BeEmpty();
            changeBreakdown.Should().BeEquivalentTo(expectedBreakdown);
            capturedUpdatedCoins[50].Should().Be(2);
            capturedUpdatedCoins[25].Should().Be(4);
            capturedUpdatedCoins[10].Should().Be(9);
            _mockRepository.Verify(r => r.UpdateChangeStock(It.IsAny<Dictionary<int, int>>()), Times.Once);
        }

        [Test]
        public void CalculateChange_WithFailedCalculation_DoesNotUpdateInventory()
        {
            // Arrange
            var availableCoins = new Dictionary<int, int> { { 50, 1 } };
            _mockRepository.Setup(r => r.GetAvailableChange()).Returns(availableCoins);
            _mockStrategy.Setup(s => s.Calculate(100, availableCoins, out It.Ref<Dictionary<int, int>>.IsAny))
                .Returns((int amt, Dictionary<int, int> coins, out Dictionary<int, int> bd) =>
                {
                    bd = new Dictionary<int, int>();
                    return false;
                });

            // Act
            bool result = _service.CalculateChange(100, out _, out var errorMessage);

            // Assert
            result.Should().BeFalse();
            errorMessage.Should().Contain("Unable to provide exact change");
            _mockRepository.Verify(r => r.UpdateChangeStock(It.IsAny<Dictionary<int, int>>()), Times.Never);
        }

        [Test]
        public void GetAvailableChange_ReturnsRepositoryData()
        {
            // Arrange
            var expectedCoins = new Dictionary<int, int> { { 50, 5 }, { 25, 10 } };
            _mockRepository.Setup(r => r.GetAvailableChange()).Returns(expectedCoins);

            // Act
            var result = _service.GetAvailableChange();

            // Assert
            result.Should().BeEquivalentTo(expectedCoins);
        }
    }
}
