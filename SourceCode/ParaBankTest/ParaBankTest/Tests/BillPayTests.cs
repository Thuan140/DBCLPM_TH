using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ParaBankTest.Pages;
using ParaBankTest.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ParaBankTest.Tests
{
    [TestFixture]
    public class BillPayTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private BillPayPage billPayPage;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "BillPayData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\Tuan7-9(New)\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--incognito"); // Chế độ ẩn danh để sạch Session và Popup
            options.AddArgument("--disable-save-password-bubble");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();

            loginPage = new LoginPage(driver);
            billPayPage = new BillPayPage(driver);

            // BƯỚC 1: Đăng nhập trước khi thanh toán hóa đơn
            driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            loginPage.Login("thuan12", "Adsada2.");

            // Dập popup Chrome bằng phím ESC
            try
            {
                System.Threading.Thread.Sleep(500);
                new OpenQA.Selenium.Interactions.Actions(driver).SendKeys(Keys.Escape).Perform();
            }
            catch { }
        }

        [Test]
        public void TestBillPayProcess()
        {
            // Đọc dữ liệu từ file JSON
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(jsonPath));

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                try
                {
                    // 2. Vào menu Bill Pay
                    billPayPage.ClickBillPayMenu();

                    // 3. Điền toàn bộ Form thanh toán
                    billPayPage.FillBillForm(data);

                    // 4. Lấy kết quả thực tế
                    string actual = billPayPage.GetResultText();
                    string expected = data.ExpectedResult.ToString();

                    // Chuẩn hóa chuỗi để so sánh
                    string actualClean = actual.ToLower().Trim();
                    string expectedClean = expected.ToLower().Trim();

                    // CHỤP ẢNH MÀN HÌNH (Chụp trước khi ghi file Excel để có đường dẫn)
                    string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);

                    if (actualClean.Contains(expectedClean))
                    {
                        // PASS: Bây giờ đã có screenPath để truyền vào Excel
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "PASS", "");
                    }
                    else
                    {
                        // FAIL: Truyền thông tin lỗi và ảnh chụp
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi Case {testCaseID}: {ex.Message}");
                    // Nếu lỗi hệ thống, cố gắng chụp ảnh một lần nữa nếu driver còn sống
                    try
                    {
                        string errScreen = CaptureHelper.TakeScreenshot(driver, testCaseID + "_ERR");
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi hệ thống", "FAIL", errScreen);
                    }
                    catch
                    {
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi hệ thống", "FAIL", ex.Message);
                    }
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }
}