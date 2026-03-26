using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;
using System.Collections.Generic;

namespace ParaBankTest.Pages
{
    public class BillPayPage
    {
        private IWebDriver driver;
        public BillPayPage(IWebDriver driver) => this.driver = driver;

        // Locators
        private By menuBillPay = By.LinkText("Bill Pay");
        private By btnSendPayment = By.XPath("//input[@value='Send Payment']");

        public void ClickBillPayMenu() => driver.FindElement(menuBillPay).Click();

        public void FillBillForm(dynamic data)
        {
            // ParaBank dùng name thay vì id cho một số trường trong Bill Pay
            driver.FindElement(By.Name("payee.name")).SendKeys(data.PayeeName.ToString());
            driver.FindElement(By.Name("payee.address.street")).SendKeys(data.Address.ToString());
            driver.FindElement(By.Name("payee.address.city")).SendKeys(data.City.ToString());
            driver.FindElement(By.Name("payee.address.state")).SendKeys(data.State.ToString());
            driver.FindElement(By.Name("payee.address.zipCode")).SendKeys(data.ZipCode.ToString());
            driver.FindElement(By.Name("payee.phoneNumber")).SendKeys(data.Phone.ToString());
            driver.FindElement(By.Name("payee.accountNumber")).SendKeys(data.AccountNumber.ToString());
            driver.FindElement(By.Name("verifyAccount")).SendKeys(data.VerifyAccount.ToString());
            driver.FindElement(By.Name("amount")).SendKeys(data.Amount.ToString());

            // Chọn tài khoản nguồn
            SelectElement fromAcc = new SelectElement(driver.FindElement(By.Name("fromAccountId")));
            fromAcc.SelectByIndex(int.Parse(data.FromAccountIndex.ToString()));

            driver.FindElement(btnSendPayment).Click();
        }

        public string GetResultText()
        {
            System.Threading.Thread.Sleep(2000); // Đợi AJAX xử lý
            try
            {
                // 1. Kiểm tra lỗi đỏ class="error" trước
                var errors = driver.FindElements(By.ClassName("error"));
                string errorText = "";
                foreach (var err in errors) if (err.Displayed) errorText += err.Text + " ";
                if (!string.IsNullOrEmpty(errorText)) return errorText.Trim();

                // 2. Nếu không có lỗi đỏ, kiểm tra tiêu đề thành công trong div "billpayResult"
                return driver.FindElement(By.XPath("//div[@id='billpayResult']/h1")).Text;
            }
            catch { return "Not Found"; }
        }
    }
}