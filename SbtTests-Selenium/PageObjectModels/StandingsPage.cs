using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Globalization;

namespace Sbt.Tests.Selenium.PageObjectModels
{
    internal class StandingsPage
    {
        private IWebDriver _driver { get; init; } = default!;
        private string _pageURL { get; init; } = "Standings/TestOrgForSelenium/Selenium01";
        private string _fullURL { get; set; } = ""; // set in constructor for use in Load()
        private string _title { get; init; } = "TestOrgForSelenium Standings - SoftballTech";
        private string _standingsColumnSelector = ".standings-table td:nth-child({0})";
        private string _schedulesColumnSelector = ".schedule-table td:nth-child({0})";
        private int _visitorScoreColumnIndex = 2;
        private int _homeScoreColumnIndex = 4;

        #region Page Elements
        private By _byUpdated = By.Id("spanUpdated");
        private By _byStandingsTable = By.ClassName("standings-table");
        private By _bySelectionDropdown = By.Id("teamNameSelect");
        #endregion

        internal StandingsPage(IWebDriver driver, string baseURL)
        {
            this._driver = driver;
            this._fullURL = $"{baseURL}/{this._pageURL}";
            this.Load();
            if (driver.Title != this._title)
            {
                throw new InvalidDataException(
                    "Unable to navigate to the Standings Page for the Selenium Test Organization " +
                    "- please ensure that test data has been created for this organization");
            }

            // make sure both tables havw at least one row of data
            var columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._standingsColumnSelector, 1)));
            if (columnCells.Count == 0)
            {
                throw new InvalidDataException("No data in Standings Table");
            }
            columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._schedulesColumnSelector, 1)));
            if (columnCells.Count == 0)
            {
                throw new InvalidDataException("No data in Schedule Table");
            }
        }

        internal void Load()
        {
            this._driver.Navigate().GoToUrl(this._fullURL);
        }

        internal bool IsDriverOnPage()
        {
            return (this._driver.Url == this._fullURL);
        }

        #region Get Data Methods
        internal DateTime GetUpdated()
        {
            var elementUpdated = this._driver.FindElement(_byUpdated);
            var updatedText = elementUpdated.Text;

            // format is "Updated:  " followed by a date we need to return
            return DateTime.ParseExact(updatedText.Substring(10), "M/dd/yyyy h:mm tt", CultureInfo.InvariantCulture);
        }

        internal bool IsStandingsColumnZero(int columnIndex, bool isWinPct)
        {
            // Check if all cell values in the specified standings column are zero
            var columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._standingsColumnSelector, columnIndex)));

            if (isWinPct)
            {
                return columnCells.All(cell => cell.Text == ".000");

            }
            return columnCells.All(cell => cell.Text == "0");
        }

        internal bool AreAllScoresEmpty()
        {
            // Check if all cell values in both home and visitor score columns are empty
            var columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._schedulesColumnSelector, this._visitorScoreColumnIndex)));

            bool visitorScoresEmpty = columnCells.All(cell => cell.Text == "");

            columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._schedulesColumnSelector, this._homeScoreColumnIndex)));

            bool homeScoresEmpty = columnCells.All(cell => cell.Text == "");

            return visitorScoresEmpty && homeScoresEmpty;
        }

        internal int GetNumberOfGamesInSchedule()
        {
            // any column will do, using visitor scores here
            var columnCells = _driver.FindElements(By.CssSelector(string.Format(
                this._schedulesColumnSelector, this._visitorScoreColumnIndex)));
            return columnCells.Count;
        }

        internal string GetSelectedTeam()
        {
            var dropdown = this._driver.FindElement(this._bySelectionDropdown);
            SelectElement select = new SelectElement(dropdown);
            return select.SelectedOption.Text;
        }

        internal GameRecord[] GetGameRecords(int gameID, bool isDoubleHeader)
        {
            // parse one or two HTML <tr> elements to pull data to create
            // one or two game records

            // first, find the game requested using the "href" attribute in the anchor tag
            var gameLink = this._driver.FindElement(
                By.CssSelector($"a[href*='gameID={gameID}']"));

            // Navigate back to the parent <tr> element
            var parentTrElement = gameLink.FindElement(By.XPath("./ancestor::tr"));

            // Get the entire HTML string of the <tr> element
            string trHtml = parentTrElement.GetAttribute("outerHTML");

            if (isDoubleHeader)
            {
                // repeat for the next sibling <tr> element
                IWebElement nextTrElement = parentTrElement.FindElement(By.XPath("following-sibling::tr"));

                string nextTrHtml = nextTrElement.GetAttribute("outerHTML");

                return new GameRecord[]
                {
                    Utilities.HtmlToGameRecord(trHtml),
                    Utilities.HtmlToGameRecord(nextTrHtml)
                };
            }
            else // single game
            {
                return new GameRecord[] { Utilities.HtmlToGameRecord(trHtml) };
            }
        }

        internal StandingsRecord[] GetStandingsRecords()
        {
            // parse the Standings Table HTML to pull date to create a standings record
            var htmlContent = this._driver.FindElement(this._byStandingsTable).GetAttribute("outerHTML");

            return Utilities.HtmlToStandingsRecord(htmlContent);
        }
        #endregion

        #region Action Methods
        internal StandingsPage SelectTeamInDropdownList(string teamName)
        {
            string initialUrl = this._driver.Url;
            IWebElement dropdown = this._driver.FindElement(this._bySelectionDropdown);
            WebDriverWait wait = new WebDriverWait(this._driver, TimeSpan.FromSeconds(5));

            SelectElement select = new SelectElement(dropdown);
            select.SelectByText(teamName);

            // Selecting a team redirects to this page with a query parameter for the selected team,
            // so wait for the URL to change from the original (without that parameter).
            wait.Until(driver => !driver.Url.Equals(initialUrl));

            return this;
        }

        internal ReportScoresPage ClickOnGameToReportScores(int gameID)
        {
            IWebElement gameLink = this._driver.FindElement(
                By.CssSelector($"a[href*='gameID={gameID}']"));

            SeleniumWrapper.Click(this._driver, By.CssSelector($"a[href*='gameID={gameID}']"));

            return new ReportScoresPage(this._driver);
        }
        #endregion
    }
}
