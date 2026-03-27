using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ParaBankTest.Pages;
using ParaBankTest.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace ParaBankTest.Tests
{
    [TestFixture]
    public class OpenAccountTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;
        private OpenAccountPage openAccountPage;

        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "OpenAccountData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\Tuan7-9(New)\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            // Dùng Incognito để tránh popup rò rỉ mật khẩu như hôm trước
            options.AddArgument("--incognito");
            options.AddArgument("--disable-save-password-bubble");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();

            loginPage = new LoginPage(driver);
            openAccountPage = new OpenAccountPage(driver);

            // BƯỚC QUAN TRỌNG: Login trước để có quyền vào trang Open Account
            driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            loginPage.Login("thuan12", "Adsada2.");
        }

        [Test]
        public void TestOpenAccountProcess()
        {
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(jsonPath));

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                try
                {
                    // Reset về trang tạo tài khoản
                    driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/openaccount.htm");

                    

                    openAccountPage.CreateAccount(
                        data.AccountType.ToString(),
                        data.FromAccountIndex.ToString()
                    );
                    System.Threading.Thread.Sleep(3000);
                    // Bây giờ mới lấy kết quả (Hàm GetResultText ở trên đã có WebDriverWait 5s rồi)
                    string actual = openAccountPage.GetResultText();
                    string expected = data.ExpectedResult.ToString();

                    if (actual.ToLower().Contains(expected.ToLower()))
                    {
                        string newAccId = openAccountPage.GetNewAccountId();
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "PASS", "" );
                    }
                    else
                    {
                        string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi kỹ thuật", "FAIL", ex.Message);
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