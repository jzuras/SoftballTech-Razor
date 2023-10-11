using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Sbt.Tests.Selenium.PageObjectModels;

namespace Sbt.Tests.Selenium
{
    // todo - separate into two test classes, one for each page
    public class StandingsPageTests
    {
        private string _urlBase { get; } = "https://sbt.azurewebsites.net";
        private StandingsPage _standingsPage { get; set; } = default!;
        private CreateDataForSeleniumPage _createDataForSeleniumPage { get; set; } = default!;

        private IWebDriver _driver { get; set; } = default!;
        private int _maxNumberOfGames = 27;
        private string _alLTeams = "All Teams";
        private string _teamNameToVerifyScheduleGameCount = "Green";
        private int _scheduleGameCountForOneTeam = 9;

        [SetUp]
        public void Setup()
        {
            var firefoxOptions = new FirefoxOptions();
            firefoxOptions.AcceptInsecureCertificates = true;
            this._driver = new FirefoxDriver(firefoxOptions);

            // first Delete the Org data in case it is still around for some reason, then Create it
            this._createDataForSeleniumPage = new CreateDataForSeleniumPage(this._driver, this._urlBase);
            this._createDataForSeleniumPage.DaleteData();
            this._createDataForSeleniumPage.CreateData();

            this._standingsPage = new StandingsPage(this._driver, this._urlBase);
        }


        [Test]
        public void StandingsPageAfterInitializationTest()
        {
            // test the page immediately after data was created to make sure it is empty
            // (no scores reported, standings all zero values, updated time very close to now)

            // verify data was updated within the last minute
            var updated = this._standingsPage.GetUpdated();
            var diff = this.GetEasternTime() - updated;
            Assert.IsTrue(Math.Abs(diff.Seconds) < 60, 
                "Data not recently updated. Difference found was " + diff.Seconds + " seconds.");

            // check for all 0s in standings
            for (int i = 2; i < 10; i++)
            {
                Assert.IsTrue(this._standingsPage.IsStandingsColumnZero(i, i == 5), 
                    "Column " + i + " not all zero values in Standings Table.");
            }

            // schedule should have no scores reported
            Assert.IsTrue(this._standingsPage.AreAllScoresEmpty(), "Schedule has at least one score reported.");
            Assert.That(this._maxNumberOfGames, Is.EqualTo(this._standingsPage.GetNumberOfGamesInSchedule()));
        }

        [Test]
        public void StandingsPageFilterByTeamTest()
        {
            // verify ability to display the schedule for a single team and back again to all teams

            // switch to a team, verify the team was selected and that fewer games are displayed
            var green = this._standingsPage.SelectTeamInDropdownList(this._teamNameToVerifyScheduleGameCount)
                .GetSelectedTeam();
            Assert.That(green, Is.EqualTo(this._teamNameToVerifyScheduleGameCount));
            Assert.That(this._standingsPage.GetNumberOfGamesInSchedule(), 
                Is.EqualTo(this._scheduleGameCountForOneTeam));

            // reset to all teams. verify and count games again
            var allTeams = this._standingsPage.SelectTeamInDropdownList(this._alLTeams).GetSelectedTeam();
            Assert.That(allTeams, Is.EqualTo(this._alLTeams));
            Assert.That(this._standingsPage.GetNumberOfGamesInSchedule(), 
                Is.EqualTo(this._maxNumberOfGames));
        }

        [TearDown]
        public void TearDown()
        {
            this._createDataForSeleniumPage.Load();
            this._createDataForSeleniumPage.DaleteData();
            this._driver.Quit();
        }

        #region Helper Methods
        private DateTime GetEasternTime()
        {
            DateTime utcTime = DateTime.UtcNow;

            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
        }
        #endregion
    }
}