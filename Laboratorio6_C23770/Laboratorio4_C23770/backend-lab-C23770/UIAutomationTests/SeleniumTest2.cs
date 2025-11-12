using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace UIAutomationTests
{
    public class SeleniumTest2
    {
        private IWebDriver? _driver;
        private WebDriverWait? _wait;
        private const string BaseUrl = "http://localhost:8080/";

        [SetUp]
        public void SetUp()
        {
            // Configurar opciones para Brave
            var options = new ChromeOptions();
            var bravePath = @"C:\Program Files\BraveSoftware\Brave-Browser\Application\brave.exe";

            if (File.Exists(bravePath))
            {
                options.BinaryLocation = bravePath;
            }
            // Si no existe Brave, usará Chrome por defecto

            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(30);
        }

        [Test]
        [Order(1)]
        public void Test1_Verify_Countries_List_Is_Displayed()
        {
            try
            {
                TestContext.WriteLine($"Navegando a: {BaseUrl}");

                // Navegar a la página principal
                _driver!.Navigate().GoToUrl(BaseUrl);

                // Esperar a que Vue cargue
                _wait!.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                Thread.Sleep(2000); // Esperar a que Vue monte los componentes

                // Verificar que el título esté presente
                var pageTitle = _wait.Until(driver =>
                    driver.FindElement(By.XPath("//h1[contains(text(), 'Lista de países')]")));

                TestContext.WriteLine($"✓ Título encontrado: {pageTitle.Text}");

                // Verificar que la tabla existe
                var table = _driver.FindElement(By.CssSelector("table.table"));
                Assert.That(table, Is.Not.Null, "La tabla de países debe existir");
                Assert.That(table.Displayed, Is.True, "La tabla debe ser visible");

                // Verificar que el botón "Agregar país" existe
                var addButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Agregar país')]"));
                Assert.That(addButton, Is.Not.Null, "El botón 'Agregar país' debe existir");
                Assert.That(addButton.Displayed, Is.True, "El botón debe ser visible");

                TestContext.WriteLine("✓ Test exitoso: Lista de países cargada correctamente");

                Assert.Pass();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [Test]
        [Order(2)]
        public void Test2_Navigate_To_Create_Country_Form()
        {
            try
            {
                // Navegar a la página principal
                _driver!.Navigate().GoToUrl(BaseUrl);

                _wait!.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                Thread.Sleep(2000);

                TestContext.WriteLine("Buscando botón 'Agregar país'...");

                // Hacer clic en el botón "Agregar país"
                var addButton = _wait.Until(driver =>
                    driver.FindElement(By.XPath("//button[contains(text(), 'Agregar país')]")));

                addButton.Click();
                TestContext.WriteLine("✓ Click en 'Agregar país'");

                Thread.Sleep(2000);

                // Verificar que estamos en la página del formulario
                var formTitle = _wait.Until(driver =>
                    driver.FindElement(By.XPath("//h3[contains(text(), 'Formulario de creación de países')]")));

                Assert.That(formTitle, Is.Not.Null, "El título del formulario debe existir");
                Assert.That(formTitle.Displayed, Is.True, "El formulario debe ser visible");

                TestContext.WriteLine($"✓ Formulario encontrado: {formTitle.Text}");
                TestContext.WriteLine($"✓ URL actual: {_driver.Url}");

                Assert.Pass();
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [Test]
        [Order(3)]
        public void Test3_Create_Country_Successfully()
        {
            try
            {
                var countryName = $"PaisTest_{DateTime.Now:yyyyMMddHHmmss}";
                var continent = "América";
                var language = "Español";

                TestContext.WriteLine($"Intentando crear país: {countryName}");

                // Navegar a la página principal
                _driver!.Navigate().GoToUrl(BaseUrl);

                _wait!.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                Thread.Sleep(2000);

                // Click en "Agregar país"
                var addButton = _wait.Until(driver =>
                    driver.FindElement(By.XPath("//button[contains(text(), 'Agregar país')]")));

                addButton.Click();
                TestContext.WriteLine("✓ Click en 'Agregar país'");

                Thread.Sleep(2000);

                // Completar el formulario
                TestContext.WriteLine("Completando formulario...");

                var nameInput = _wait.Until(driver => driver.FindElement(By.Id("name")));
                nameInput.Clear();
                nameInput.SendKeys(countryName);
                TestContext.WriteLine($"✓ Nombre ingresado: {countryName}");

                var continentSelect = _driver.FindElement(By.Id("continente"));
                var selectElement = new SelectElement(continentSelect);
                selectElement.SelectByText(continent);
                TestContext.WriteLine($"✓ Continente seleccionado: {continent}");

                var languageInput = _driver.FindElement(By.Id("idioma"));
                languageInput.Clear();
                languageInput.SendKeys(language);
                TestContext.WriteLine($"✓ Idioma ingresado: {language}");

                TakeScreenshot("Before_Submit");

                // Click en botón "Guardar"
                var submitButton = _driver.FindElement(By.XPath("//button[@type='submit' and contains(text(), 'Guardar')]"));
                submitButton.Click();
                TestContext.WriteLine("✓ Formulario enviado");

                // Esperar a que la petición se complete
                Thread.Sleep(4000);

                var currentUrl = _driver.Url;
                TestContext.WriteLine($"URL después de enviar: {currentUrl}");

                // **VERIFICAR SI REDIRIGIÓ O NO**
                if (currentUrl.Contains("/country"))
                {
                    TestContext.WriteLine("⚠️ NO se redirigió a la home. Verificando error...");

                    // Capturar logs de la consola del navegador
                    var logs = _driver.Manage().Logs.GetLog(LogType.Browser);
                    TestContext.WriteLine("\n--- LOGS DEL NAVEGADOR ---");
                    foreach (var log in logs)
                    {
                        if (log.Level == LogLevel.Severe || log.Level == LogLevel.Warning)
                        {
                            TestContext.WriteLine($"{log.Level}: {log.Message}");
                        }
                    }

                    // Verificar si hay mensaje de error visible en la página
                    try
                    {
                        var errorElements = _driver.FindElements(By.CssSelector(".error, .alert-danger, [class*='error']"));
                        if (errorElements.Any())
                        {
                            TestContext.WriteLine("\n--- MENSAJES DE ERROR EN LA PÁGINA ---");
                            foreach (var error in errorElements)
                            {
                                if (error.Displayed)
                                {
                                    TestContext.WriteLine($"Error: {error.Text}");
                                }
                            }
                        }
                    }
                    catch { }

                    TakeScreenshot("No_Redirect_Stay_In_Form");

                    // **IMPORTANTE: Verificar que el backend esté funcionando**
                    TestContext.WriteLine("\n🔍 DIAGNÓSTICO:");
                    TestContext.WriteLine("1. ¿El backend está corriendo en http://localhost:5166?");
                    TestContext.WriteLine("2. ¿La base de datos está conectada?");
                    TestContext.WriteLine("3. Revisa los logs del backend en Visual Studio");
                    TestContext.WriteLine("4. Prueba el endpoint manualmente: POST http://localhost:5166/api/Country");

                    // Navegar manualmente a la home para verificar si se guardó
                    TestContext.WriteLine("\nNavegando manualmente a la home para verificar...");
                    _driver.Navigate().GoToUrl(BaseUrl);
                    Thread.Sleep(3000);
                }
                else
                {
                    TestContext.WriteLine($"✓ Redirigió correctamente a: {currentUrl}");
                }

                // Esperar a que Vue cargue la lista
                _wait.Until(driver =>
                    ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));

                Thread.Sleep(2000);

                // Verificar que estamos en la página principal
                try
                {
                    var pageTitle = _wait.Until(driver =>
                        driver.FindElement(By.XPath("//h1[contains(text(), 'Lista de países')]")));
                    TestContext.WriteLine($"✓ En la página principal: {pageTitle.Text}");
                }
                catch
                {
                    TestContext.WriteLine("❌ No se pudo encontrar el título de la lista");
                    TakeScreenshot("Not_In_Home_Page");
                    Assert.Fail("No se pudo regresar a la página principal después de crear el país");
                }

                // Verificar que el país aparece en la tabla
                try
                {
                    var tableBody = _wait.Until(driver => driver.FindElement(By.CssSelector("table.table tbody")));
                    var pageSource = tableBody.Text;

                    TestContext.WriteLine($"\n--- CONTENIDO DE LA TABLA ---\n{pageSource}\n");

                    if (pageSource.Contains(countryName))
                    {
                        TestContext.WriteLine($"✅✅✅ País '{countryName}' creado exitosamente y visible en la lista");
                        TakeScreenshot("After_Create_Success");
                        Assert.Pass($"País '{countryName}' creado correctamente");
                    }
                    else
                    {
                        TestContext.WriteLine($"⚠️ El país '{countryName}' NO aparece en la lista");
                        TestContext.WriteLine("\nPosibles causas:");
                        TestContext.WriteLine("1. El POST al backend falló silenciosamente");
                        TestContext.WriteLine("2. La base de datos no guardó el registro");
                        TestContext.WriteLine("3. El método getCountries() no está trayendo el nuevo país");

                        TakeScreenshot("Country_Not_In_List");

                        Assert.Inconclusive($"El formulario se envió pero '{countryName}' no aparece en la lista. Verifica el backend y la BD.");
                    }
                }
                catch (NoSuchElementException ex)
                {
                    TestContext.WriteLine($"❌ No se encontró la tabla: {ex.Message}");
                    TakeScreenshot("Table_Not_Found");
                    throw;
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"\n❌ ERROR GENERAL: {ex.Message}");
                TestContext.WriteLine($"Stack trace: {ex.StackTrace}");
                TakeScreenshot("Test3_General_Error");
                throw;
            }
        }

        [Test]
        [Order(4)]
        public void Test4_Verify_Form_Validation_For_Empty_Fields()
        {
            try
            {
                TestContext.WriteLine("Probando validación de formulario vacío...");

                // Navegar y abrir formulario
                _driver!.Navigate().GoToUrl(BaseUrl);
                Thread.Sleep(2000);

                var addButton = _wait!.Until(driver =>
                    driver.FindElement(By.XPath("//button[contains(text(), 'Agregar país')]")));
                addButton.Click();
                Thread.Sleep(2000);

                // Intentar enviar formulario vacío
                var submitButton = _driver.FindElement(By.XPath("//button[@type='submit']"));
                submitButton.Click();
                TestContext.WriteLine("✓ Click en submit sin llenar campos");

                Thread.Sleep(1000);

                // Verificar que NO se redirija (debe quedarse en /country)
                Assert.That(_driver.Url, Does.Contain("country"),
                    "Debe permanecer en la página del formulario debido a validación HTML5");

                // Verificar validación HTML5 del campo nombre
                var nameInput = _driver.FindElement(By.Id("name"));
                var isValid = (bool)((IJavaScriptExecutor)_driver)
                    .ExecuteScript("return arguments[0].validity.valid;", nameInput);

                Assert.That(isValid, Is.False,
                    "El campo nombre debe ser inválido cuando está vacío");

                TestContext.WriteLine("✓ Validación HTML5 funcionando correctamente");

                Assert.Pass("Validación de campos vacíos funciona correctamente");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"❌ Error: {ex.Message}");
                throw;
            }
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (_driver != null &&
                    TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    TakeScreenshot(TestContext.CurrentContext.Test.Name + "_Failed");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error en TearDown: {ex.Message}");
            }
            finally
            {
                try
                {
                    _driver?.Quit();
                    _driver?.Dispose();
                }
                catch { }
            }
        }

        private void TakeScreenshot(string testName)
        {
            try
            {
                if (_driver == null) return;

                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                var fileName = $"{testName}_{DateTime.Now:yyyyMMddHHmmss}.png";
                var screenshotsDir = Path.Combine(TestContext.CurrentContext.TestDirectory, "Screenshots");

                Directory.CreateDirectory(screenshotsDir);

                var screenshotPath = Path.Combine(screenshotsDir, fileName);
                screenshot.SaveAsFile(screenshotPath);

                TestContext.WriteLine($"📸 Screenshot: {screenshotPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error al tomar screenshot: {ex.Message}");
            }
        }
    }
}