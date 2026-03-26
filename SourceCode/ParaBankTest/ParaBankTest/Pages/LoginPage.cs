using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace ParaBankTest.Pages
{
    public class LoginPage
    {
        private IWebDriver driver;
        public LoginPage(IWebDriver driver) => this.driver = driver;

        // Locators
        private By txtUsername = By.Name("username");
        private By txtPassword = By.Name("password");
        private By btnLogin = By.XPath("//input[@value='Log In']");
        private By lnkLogout = By.LinkText("Log Out");

        public void Login(string user, string pass)
        {
            // Xóa dữ liệu cũ trước khi nhập
            driver.FindElement(txtUsername).Clear();
            driver.FindElement(txtUsername).SendKeys(user);
            driver.FindElement(txtPassword).Clear();
            driver.FindElement(txtPassword).SendKeys(pass);
            driver.FindElement(btnLogin).Click();
        }

        public void Logout()
        {
            try
            {
                IWebElement logoutLink = driver.FindElement(By.LinkText("Log Out"));
                // Dùng JavaScript để click "xuyên" qua popup
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("arguments[0].click();", logoutLink);
            }
            catch (Exception)
            {
                // Nếu không tìm thấy nút Log Out, có thể là đã logout rồi, điều hướng thẳng về index luôn cho chắc
                driver.Navigate().GoToUrl("https://parabank.parasoft.com/parabank/index.htm");
            }
        }

        public string GetResultText()
        {
            try
            {
                // 1. Ưu tiên tìm các thông báo lỗi chữ đỏ (class='error')
                var errorElements = driver.FindElements(By.ClassName("error"));
                string allErrors = "";
                foreach (var error in errorElements)
                {
                    if (error.Displayed) allErrors += error.Text + " ";
                }

                if (!string.IsNullOrWhiteSpace(allErrors)) return allErrors.Trim();

                // 2. Nếu không có lỗi, tìm tiêu đề trang thành công (ví dụ: Accounts Overview)
                var titleElements = driver.FindElements(By.ClassName("title"));
                if (titleElements.Count > 0 && titleElements[0].Displayed)
                {
                    return titleElements[0].Text;
                }

                return "";
            }
            catch { return ""; }
        }
    }
}