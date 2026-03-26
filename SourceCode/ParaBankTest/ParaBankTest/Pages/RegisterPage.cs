using OpenQA.Selenium;
using System;

namespace ParaBankTest.Pages
{
    public class RegisterPage
    {
        private IWebDriver driver;

        public RegisterPage(IWebDriver driver) => this.driver = driver;

        // Locators
        private By txtFirstName = By.Id("customer.firstName");
        private By txtLastName = By.Id("customer.lastName");
        private By txtAddress = By.Id("customer.address.street");
        private By txtCity = By.Id("customer.address.city");
        private By txtState = By.Id("customer.address.state");
        private By txtZipCode = By.Id("customer.address.zipCode");
        private By txtPhone = By.Id("customer.phoneNumber");
        private By txtSSN = By.Id("customer.ssn");
        private By txtUsername = By.Id("customer.username");
        private By txtPassword = By.Id("customer.password");
        private By txtConfirm = By.Id("repeatedPassword");
        private By btnRegister = By.XPath("//input[@value='Register']");
        private By lblSuccessMessage = By.XPath("//div[@id='rightPanel']/p");
        private By lblErrorMessage = By.ClassName("error");

        // Actions
        public void RegisterUser(dynamic data)
        {
            driver.FindElement(txtFirstName).SendKeys(data.FirstName.ToString());
            driver.FindElement(txtLastName).SendKeys(data.LastName.ToString());
            driver.FindElement(txtAddress).SendKeys(data.Address.ToString());
            driver.FindElement(txtCity).SendKeys(data.City.ToString());
            driver.FindElement(txtState).SendKeys(data.State.ToString());
            driver.FindElement(txtZipCode).SendKeys(data.ZipCode.ToString());
            driver.FindElement(txtPhone).SendKeys(data.Phone.ToString());
            driver.FindElement(txtSSN).SendKeys(data.SSN.ToString());
            driver.FindElement(txtUsername).SendKeys(data.Username.ToString());
            driver.FindElement(txtPassword).SendKeys(data.Password.ToString());
            driver.FindElement(txtConfirm).SendKeys(data.Confirm.ToString());
            driver.FindElement(btnRegister).Click();
        }

        public string GetResultText()
        {
            try
            {
                // 1. Quét tất cả các thông báo lỗi chữ đỏ (class="error")
                // Đây là ưu tiên số 1 để bắt các Case Negative (nhập thiếu/sai)
                var errorElements = driver.FindElements(By.ClassName("error"));

                string allText = "";
                foreach (var error in errorElements)
                {
                    if (error.Displayed && !string.IsNullOrWhiteSpace(error.Text))
                    {
                        allText += error.Text + " ";
                    }
                }

                // 2. Nếu không thấy lỗi chữ đỏ nào, mới đi tìm thông báo thành công (Welcome...)
                if (string.IsNullOrWhiteSpace(allText))
                {
                    var successElements = driver.FindElements(By.XPath("//div[@id='rightPanel']/p"));
                    if (successElements.Count > 0 && successElements[0].Displayed)
                    {
                        return successElements[0].Text;
                    }
                }

                return allText.Trim();
            }
            catch (Exception)
            {
                return "Error_Not_Found";
            }
        }
    }
}