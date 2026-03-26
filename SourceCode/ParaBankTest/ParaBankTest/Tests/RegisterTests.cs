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
    public class RegisterTests
    {
        private IWebDriver driver;
        private RegisterPage registerPage;

        // CẬP NHẬT ĐƯỜNG DẪN MỚI TẠI ĐÂY
        private string jsonPath = Path.Combine(Directory.GetCurrentDirectory(),"TestData","RegisterData.json");
        private string excelPath = @"C:\BaoDamChatLuongPM\Tuan7-9(New)\Testcase.xlsx";

        [SetUp]
        public void Setup()
        {
            driver = new ChromeDriver();
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Window.Maximize();
            driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");
            registerPage = new RegisterPage(driver);
        }

        [Test]
        public void TestRegisterProcess()
        {
            string jsonContent = File.ReadAllText(jsonPath);
            // Chuyển dynamic thành List để lấy được Count và dùng vòng lặp for
            var testDataList = JsonConvert.DeserializeObject<List<dynamic>>(jsonContent);

            for (int i = 0; i < testDataList.Count; i++)
            {
                var data = testDataList[i];
                string testCaseID = data.TestCaseID?.ToString() ?? "Unknown";

                try
                {
                    // Reset trang web trước mỗi case
                    driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/register.htm");

                    registerPage.RegisterUser(data);

                    string actualResult = registerPage.GetResultText();
                    string expectedResult = data.ExpectedResult.ToString();

                    // Chuẩn hóa để so sánh chính xác hơn (bỏ khoảng trắng thừa, đưa về chữ thường)
                    string actualClean = actualResult.ToLower().Trim();
                    string expectedClean = expectedResult.ToLower().Trim();

                    // Kiểm tra xem câu lỗi mong đợi có nằm trong danh sách lỗi thực tế không
                    if (actualClean.Contains(expectedClean))
                    {
                        // PASS: Nếu tìm thấy câu lỗi tương ứng
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actualResult, "PASS", "");
                    }
                    else
                    {
                        // FAIL: Nếu không tìm thấy
                        string screenPath = CaptureHelper.TakeScreenshot(driver, testCaseID);
                        // Ghi Actual Result vào Excel để biết thực tế web đang hiện lỗi gì
                        ExcelHelper.UpdateExcel(excelPath, testCaseID, actualResult, "FAIL", screenPath);
                    }
                }
                catch (Exception ex)
                {
                    // Nếu lỗi hệ thống, vẫn ghi vào Excel để không bị lệch dòng các case sau
                    Console.WriteLine($"Case {testCaseID} lỗi: {ex.Message}");
                    ExcelHelper.UpdateExcel(excelPath, testCaseID, "Lỗi kỹ thuật", "FAIL", "");
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