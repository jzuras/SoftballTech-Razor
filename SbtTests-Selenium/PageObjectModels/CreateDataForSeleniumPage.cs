using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sbt.Tests.Selenium.PageObjectModels
{
    internal class CreateDataForSeleniumPage
    {
        private IWebDriver _driver { get; init; } = default!;
        private string _pageURL { get; init; } = "Admin/CreateDataForSelenium";
        private string _fullURL { get; set; } = ""; // set in constructor for use in Load()
        private string _title { get; init; } =
            "Admin - Create / Delete Data for Selenium - SoftballTech";

        #region Page Elements
        private By _byCreateData = By.Id("btnCreateData");
        private By _byDeleteData = By.Id("btnDeleteData");
        private IWebElement _createButton { get; set; } = null!;
        private IWebElement _deleteButton { get; set; } = null!;
        #endregion

        internal CreateDataForSeleniumPage(IWebDriver driver, string baseURL)
        {
            this._driver = driver;
            this._fullURL = $"{baseURL}/{this._pageURL}";
            this.Load();
            if (driver.Title != _title)
            {
                throw new InvalidDataException(
                    "Unable to navigate to the Create Data For Selenium Page for the Selenium Test Organization");
            }

            // verify that both buttons can be found
            this._createButton = this._driver.FindElement(this._byCreateData);
            this._deleteButton = this._driver.FindElement(this._byDeleteData);
        }

        internal void Load()
        {
            this._driver.Navigate().GoToUrl(this._fullURL);
        }

        internal CreateDataForSeleniumPage CreateData()
        {
            try
            {
                this._createButton.Click();
            }
            catch (StaleElementReferenceException)
            {
                this._createButton = this._driver.FindElement(this._byCreateData);
                this._createButton.Click();
            }
            return this;
        }

        internal CreateDataForSeleniumPage DaleteData()
        {
            try
            {
                this._deleteButton.Click();
            }
            catch (StaleElementReferenceException)
            {
                this._deleteButton = this._driver.FindElement(this._byDeleteData);
                this._deleteButton.Click();
            }
            return this;
        }
    }
}
