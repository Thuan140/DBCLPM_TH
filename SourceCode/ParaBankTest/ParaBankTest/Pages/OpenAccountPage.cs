using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Support.UI;
using System;

namespace ParaBankTest.Pages
{
    public class OpenAccountPage
    {
        private IWebDriver driver;
        public OpenAccountPage(IWebDriver driver) => this.driver = driver;

        // Locators
        private By menuOpenAccount = By.LinkText("Open New Account");
        private By drpAccountType = By.Id("type");
        private By drpFromAccount = By.Id("fromAccountId");
        private By btnOpenAccount = By.XPath("//input[@value='Open New Account']");
        private By lblResultTitle = By.XPath("//div[@id='rightPanel']/div/div/h1");
        private By lblNewAccountId = By.Id("newAccountId");

        // Actions
        public void ClickOpenAccountMenu() => driver.FindElement(menuOpenAccount).Click();

        public void CreateAccount(string typeIndex, string fromAccountIndex)
        {
            // Đợi một chút để Dropdown load dữ liệu (ParaBank load cái này bằng AJAX)
            System.Threading.Thread.Sleep(1000);

            SelectElement typeSelect = new SelectElement(driver.FindElement(drpAccountType));
            typeSelect.SelectByValue(typeIndex);

            SelectElement fromSelect = new SelectElement(driver.FindElement(drpFromAccount));
            fromSelect.SelectByIndex(int.Parse(fromAccountIndex));

            driver.FindElement(btnOpenAccount).Click();
        }
        public string GetNewAccountId()
        {
            try
            {
                // ID của số tài khoản mới hiện lên sau khi click Open
                return driver.FindElement(By.Id("newAccountId")).Text;
            }
            catch { return "N/A"; }
        }
        public string GetResultText()
        {
            // Đợi tối đa 5 lần, mỗi lần nghỉ 1 giây (tổng 5s)
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    // Trỏ thẳng vào h1 nằm trong div openAccountResult cho chuẩn xác theo ảnh Inspect
                    var element = driver.FindElement(By.XPath("//div[@id='openAccountResult']/h1"));

                    if (element.Displayed && !string.IsNullOrEmpty(element.Text))
                    {
                        string result = element.Text.Trim();
                        Console.WriteLine("Tìm thấy kết quả: " + result);
                        return result;
                    }
                }
                catch { /* Chưa thấy thì đợi tiếp */ }

                System.Threading.Thread.Sleep(1000);
            }
            return "Timeout: Không tìm thấy chữ Account Opened!";
        }
    }
}