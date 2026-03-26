using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace ParaBankTest.Pages
{
    public class TransferPage
    {
        private IWebDriver driver;
        public TransferPage(IWebDriver driver) => this.driver = driver;

        // Locators
        private By menuTransfer = By.LinkText("Transfer Funds");
        private By txtAmount = By.Id("amount");
        private By drpFromAccount = By.Id("fromAccountId");
        private By drpToAccount = By.Id("toAccountId");
        private By btnTransfer = By.XPath("//input[@value='Transfer']");
        private By lblResultTitle = By.XPath("//div[@id='showResult']/h1");
        private By lblDetail = By.XPath("//div[@id='showResult']/p");

        public void ClickTransferMenu() => driver.FindElement(menuTransfer).Click();

        public void TransferMoney(string amount, int fromIndex, int toIndex)
        {
            // Đợi AJAX load danh sách tài khoản vào Dropdown
            System.Threading.Thread.Sleep(2000);

            driver.FindElement(txtAmount).SendKeys(amount);

            SelectElement fromSelect = new SelectElement(driver.FindElement(drpFromAccount));
            fromSelect.SelectByIndex(fromIndex);

            SelectElement toSelect = new SelectElement(driver.FindElement(drpToAccount));
            toSelect.SelectByIndex(toIndex);

            driver.FindElement(btnTransfer).Click();
        }

        public string GetResultText()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    var element = driver.FindElement(lblResultTitle);
                    if (element.Displayed) return element.Text.Trim();
                }
                catch { System.Threading.Thread.Sleep(1000); }
            }
            return "Timeout";
        }
    }
}