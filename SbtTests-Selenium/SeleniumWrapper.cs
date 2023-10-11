using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace Sbt.Tests.Selenium
{
    internal class SeleniumWrapper
    {
        internal static void SendKeys(IWebDriver driver, By by, string valueToType)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));

            try
            {
                IWebElement? element = wait.Until(driver => FindElementSafely(driver, by));
                
                element?.Clear();
                element?.SendKeys(valueToType);
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail($"Exception in SendKeys(): element located by {by.ToString()} not visible and enabled within {5} seconds.");
            }
        }

        internal static void Click(IWebDriver driver, By by)
        {
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
            IWebElement? element = null;
            bool? tmp1, tmp2;

            try
            {
                element = wait.Until(driver => FindElementSafely(driver, by));
                tmp1 = element?.Displayed;
                tmp2 = element?.Enabled;
                element?.Click();
            }
            catch (WebDriverTimeoutException)
            {
                Assert.Fail($"Exception in Click(): element located by {by.ToString()} not visible and enabled within {5} seconds.");
            }
            catch (ElementNotInteractableException)
            {
                // for some reason Firefox Driver scrolls correctly but needs to wait a beat before clicking
                System.Threading.Thread.Sleep(1000);
                element?.Click();
            }
        }

        private static IWebElement? FindElementSafely(IWebDriver driver, By by)
        {
            IWebElement? element = driver.FindElement(by);
            return (element?.Displayed ?? false) && (element?.Enabled ?? false) ? element : null;
        }
    }
}
