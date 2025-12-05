using FluentAssertions;
using ExamTwo.Services;

namespace UnitTestExamen2
{
    [TestFixture]
    public class GreedyChangeServiceTests
    {
        private GreedyChangeService _service;

        [SetUp]
        public void Setup()
        {
            _service = new GreedyChangeService();
        }

        [Test]
        public void Calculate_WithValidAmount_ReturnsCorrectBreakdownUsingGreedyAlgorithm()
        {
            // Arrange
            int amount = 85;
            var availableCoins = new Dictionary<int, int>
            {
                { 50, 2 },
                { 25, 5 },
                { 10, 10 },
                { 5, 20 }
            };

            // Act
            bool result = _service.Calculate(amount, availableCoins, out var changeBreakdown);

            // Assert
            result.Should().BeTrue();
            changeBreakdown[50].Should().Be(1);
            changeBreakdown[25].Should().Be(1);
            changeBreakdown[10].Should().Be(1);
            changeBreakdown.Sum(kvp => kvp.Key * kvp.Value).Should().Be(85);
        }

        [Test]
        public void Calculate_WithInsufficientOrImpossibleChange_ReturnsFalse()
        {
            // Arrange - caso 1: no hay suficientes monedas
            int amount1 = 100;
            var coins1 = new Dictionary<int, int> { { 50, 1 }, { 25, 1 } };

            // Arrange - caso 2: imposible dar cambio exacto
            int amount2 = 12;
            var coins2 = new Dictionary<int, int> { { 10, 1 }, { 5, 2 } };

            // Act & Assert
            _service.Calculate(amount1, coins1, out _).Should().BeFalse();
            _service.Calculate(amount2, coins2, out _).Should().BeFalse();
        }

        [Test]
        public void Calculate_WithZeroAmountOrNoCoins_HandlesEdgeCases()
        {
            // Arrange
            var availableCoins = new Dictionary<int, int> { { 50, 5 }, { 25, 10 } };
            var noCoins = new Dictionary<int, int> { { 25, 0 } };

            // Act & Assert
            _service.Calculate(0, availableCoins, out var breakdown1).Should().BeTrue();
            breakdown1.Should().BeEmpty();
            
            _service.Calculate(25, noCoins, out _).Should().BeFalse();
        }
    }
}
