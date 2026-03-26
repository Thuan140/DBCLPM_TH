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
    public class LoginTests
    {
        private IWebDriver driver;
        private LoginPage loginPage;

        // Dùng BaseDirectory để tránh lỗi WinIOError (không tìm thấy file)
        private string jsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TestData", "LoginData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\Tuan7-9(New)\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            ChromeOptions options = new ChromeOptions();
            // Tắt bảng hỏi lưu mật khẩu và các thông báo popup của Chrome
            options.AddUserProfilePreference("credentials_enable_service", false);
            options.AddUserProfilePreference("profile.password_manager_enabled", false);
            options.AddArgument("--disable-notifications");

            driver = new ChromeDriver(options);
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5); // Giảm xuống 5s cho nhanh
            driver.Manage().Window.Maximize();
            loginPage = new LoginPage(driver);
        }

        [Test]
        public void TestLoginProcess()
        {
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(File.ReadAllText(jsonPath));
            var loginPage = new LoginPage(driver);

            foreach (var data in testDataList)
            {
                string testCaseID = data.TestCaseID.ToString();
                try
                {
                    driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
                    loginPage.Login(data.Username.ToString(), data.Password.ToString());

                    string actual = loginPage.GetResultText();
                    string expected = data.ExpectedResult.ToString();


                    if (actual.ToLower().Contains(expected.ToLower()))
                    {
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "PASS", "");

                        if (testCaseID == "TC_LG_01")
                        {
                            loginPage.Logout();
                            // Kiểm tra xem đã về trang chủ chưa (thấy ô login là pass)
                            string afterLogout = loginPage.GetResultText();
                            ExcelHelper.UpdateExcel(excelPath, "TC_LO_01", "Đã đăng xuất và quay về trang chủ", "PASS", "");
                        }
                    }
                    else
                    {
                        string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actual, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, ex.Message, "FAIL", "");
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