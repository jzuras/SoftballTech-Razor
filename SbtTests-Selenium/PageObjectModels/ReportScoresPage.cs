using OpenQA.Selenium;

namespace Sbt.Tests.Selenium.PageObjectModels
{
    internal class ReportScoresPage
    {
        private IWebDriver _driver { get; init; } = default!;
        private string _title { get; init; } = "TestOrgForSelenium Report Scores - SoftballTech";


        #region Page Elements
        private By _bySaveButton = By.Id("saveButton");
        private By _byDay = By.Id("day");
        private By _byField = By.Id("field");
        private By[] _byTime = {
            By.CssSelector("label[for='Schedules_0__Time']"),
            By.CssSelector("label[for='Schedules_1__Time']")
        };
        private By[] _byTeam = {
            By.CssSelector("label[for='Schedules_0__Visitor']"),
            By.CssSelector("label[for='Schedules_1__Visitor']"),
            By.CssSelector("label[for='Schedules_0__Home']"),
            By.CssSelector("label[for='Schedules_1__Home']")
        };
        private By[] _byScore = {
            By.Id("SchedulesVM_0__VisitorScore"),
            By.Id("SchedulesVM_1__VisitorScore"),
            By.Id("SchedulesVM_0__HomeScore"),
            By.Id("SchedulesVM_1__HomeScore")
        };
        private By[] _byForfeit = {
            By.Name("SchedulesVM[0].VisitorForfeit"),
            By.Name("SchedulesVM[1].VisitorForfeit"),
            By.Name("SchedulesVM[0].HomeForfeit"),
            By.Name("SchedulesVM[1].HomeForfeit")
        };
        private By[] _byValidationMessage = {
            By.CssSelector("span[data-valmsg-for='SchedulesVM[0].VisitorScore']"),
            By.CssSelector("span[data-valmsg-for='SchedulesVM[1].VisitorScore']"),
            By.CssSelector("span[data-valmsg-for='SchedulesVM[0].HomeScore']"),
            By.CssSelector("span[data-valmsg-for='SchedulesVM[1].HomeScore']")
        };
        #endregion

        internal ReportScoresPage(IWebDriver driver)
        {
            _driver = driver;
            if (driver.Title != _title)
            {
                throw new InvalidDataException(
                    "Unable to navigate to the Report Scores Page");
            }
        }

        #region Get Data Methods
        internal GameRecord[] GetGameRecords(bool isDoubleHeader)
        {
            // get both game records for a doubleheader
            var gameRecord = this.GetGameRecord(1);
            if( isDoubleHeader ==  false )
            {
                return new GameRecord[]
                {
                    gameRecord
                };
            }

            var gameRecord2 = this.GetGameRecord(2);
            return new GameRecord[]
            {
                    gameRecord,
                    gameRecord2
            };
        }

        internal string GetScore(int gameNumber, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            var element = this._driver.FindElement(this._byScore[index]);
            return element.GetAttribute("value");
        }

        internal bool GetForfeit(int gameNumber, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            var element = this._driver.FindElement(this._byForfeit[index]);
            return element.Selected;
        }

        internal string GetValidationMessage(int gameNumber, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            var element = this._driver.FindElement(this._byValidationMessage[index]);
            return element.Text;
        }
        #endregion

        #region Action Methods
        internal void ClickSaveButton()
        {
            this._driver.FindElement(this._bySaveButton).Click();
        }

        internal void ClickForfeitCheckbox(int gameNumber, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            this._driver.FindElement(this._byForfeit[index]).Click();
        }

        internal void EnterScore(int gameNumber, string score, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            SeleniumWrapper.SendKeys(this._driver, this._byScore[index], score);
        }
        #endregion

        #region Helper Methods
        internal GameRecord GetGameRecord(int gameNumber)
        {
            // pull data from several HTML elements to create a single game record
            var date = this.GetGameDay();
            var field = this.GetField();
            var time = this.GetTime(gameNumber);
            var homeTeam = this.GetTeam(gameNumber, true);
            var visitorTeam = this.GetTeam(gameNumber, false);
            var homeScore = this.GetScore(gameNumber, true);
            var visitorScore = this.GetScore(gameNumber, false);
            var homeForfeit = this.GetForfeit(gameNumber, true);
            var visitorForfeit = this.GetForfeit(gameNumber, false);

            return new GameRecord(visitorTeam, visitorScore, homeTeam, homeScore, date, field, time,
                visitorForfeit, homeForfeit);
        }

        private string GetGameDay()
        {
            var element = this._driver.FindElement(this._byDay);
            return element.Text;
        }

        private string GetField()
        {
            var element = this._driver.FindElement(this._byField);
            return element.Text;
        }

        private string GetTime(int gameNumber)
        {
            int index = gameNumber - 1;
            var element = this._driver.FindElements(this._byTime[index]);
            return element[1].Text;
        }

        private string GetTeam(int gameNumber, bool forHomeTeam)
        {
            int index = (gameNumber - 1) + (forHomeTeam ? 2 : 0);
            var element = this._driver.FindElements(this._byTeam[index]);
            return element[1].Text;
        }
        #endregion
    }
}
