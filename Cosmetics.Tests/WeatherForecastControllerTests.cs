using Cosmetics.Controllers;
using Microsoft.Extensions.Logging;
using Moq;

namespace Cosmetics.Tests
{
    public class WeatherForecastControllerTests
    {
        [Fact]
        public void Get_ReturnsFiveForecasts()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<WeatherForecastController>>();
            var controller = new WeatherForecastController(mockLogger.Object);

            // Act
            var result = controller.Get();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(5, list.Count);
            foreach (var item in list)
            {
                Assert.InRange(item.TemperatureC, -20, 55);
                Assert.False(string.IsNullOrWhiteSpace(item.Summary));
            }
        }
    }
}
