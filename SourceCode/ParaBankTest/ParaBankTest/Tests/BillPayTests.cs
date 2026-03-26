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
            loginPage.Login("thuan12", "Adsada2."); // Thay bằng tài khoản thật của Thuận

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

                    // 3. Điền toàn bộ Form thanh toán (Hàm này dùng dynamic data từ JSON)
                    billPayPage.FillBillForm(data);

                    // 4. Lấy kết quả thực tế (Hàm này xử lý cả lỗi đỏ class="error" và thành công)
                    string actual = billPayPage.GetResultText();
                    string expected = data.ExpectedResult.ToString();

                    // Chuẩn hóa chuỗi để so sánh chính xác
                    string actualClean = actual.ToLower().Trim();
                    string expectedClean = expected.ToLower().Trim();

                    if (actualClean.Contains(expectedClean))
                    {
                        // PASS: Ghi chú số tiền đã thanh toán cho ai
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "PASS", "");
                    }
                    else
                    {
                        // FAIL: Chụp ảnh màn hình làm bằng chứng
                        string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi Case {testCaseID}: {ex.Message}");
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi hệ thống", "FAIL", ex.Message);
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