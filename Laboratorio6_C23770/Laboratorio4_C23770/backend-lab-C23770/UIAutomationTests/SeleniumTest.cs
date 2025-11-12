using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace UIAutomationTests
{
    public class SeleniumTest
    {
        IWebDriver _driver;

        [SetUp]

        public void SetUp()
        { 
            _driver = new ChromeDriver();
        }

        [Test]
        public void Enter_To_List_Of_Countries_Tests()
        {
            // Arrange
            var url = "http://localhost:8080/";

            _driver.Manage().Window.Maximize();

            // Act
            _driver.Navigate().GoToUrl(url);

            // Assert
            Assert.IsNotNull(_driver);
        }
    }
}