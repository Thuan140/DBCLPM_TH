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
    public class TransferTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private TransferPage transferPage;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "TransferData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\Tuan7-9(New)\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            // Dùng Incognito để không bị popup "rò rỉ mật khẩu" làm phiền
            options.AddArgument("--incognito");
            options.AddArgument("--disable-save-password-bubble");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();

            loginPage = new LoginPage(driver);
            transferPage = new TransferPage(driver);

            // BƯỚC 1: Đăng nhập trước khi vào trang chuyển tiền
            driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            loginPage.Login("thuan12", "Adsada2."); // Thay bằng User/Pass chuẩn của Thuận

            // Dập tắt popup Chrome nếu có bằng phím ESC
            try
            {
                System.Threading.Thread.Sleep(500);
                new OpenQA.Selenium.Interactions.Actions(driver).SendKeys(Keys.Escape).Perform();
            }
            catch { }
        }

        [Test]
        public void TestTransferProcess()
        {
            // Đọc dữ liệu từ file JSON
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(jsonPath));

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                try
                {
                    // 2. Vào menu Chuyển tiền
                    transferPage.ClickTransferMenu();

                    // 3. Thực hiện chuyển tiền (Hàm này đã có Sleep 2s để đợi load Dropdown)
                    transferPage.TransferMoney(
                        data.Amount.ToString(),
                        int.Parse(data.FromAccountIndex.ToString()),
                        int.Parse(data.ToAccountIndex.ToString())
                    );
                    System.Threading.Thread.Sleep(3000);

                    // 4. Lấy kết quả thực tế (Hàm này có vòng lặp đợi kết quả hiện ra)
                    string actual = transferPage.GetResultText();
                    string expected = data.ExpectedResult.ToString();

                    // Chuẩn hóa chuỗi (Xóa dấu ! và đưa về chữ thường để so sánh chuẩn)
                    string actualClean = actual.ToLower().Replace("!", "").Trim();
                    string expectedClean = expected.ToLower().Replace("!", "").Trim();

                    if (actualClean.Contains(expectedClean))
                    {
                        // PASS: Ghi chú rõ số tiền đã chuyển
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "PASS", "");
                    }
                    else
                    {
                        // FAIL: Chụp ảnh màn hình
                        string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, "Thực tế: " + actual, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi tại Case {testCaseID}: {ex.Message}");
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