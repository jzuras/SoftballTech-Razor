using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using Sbt.Tests.Selenium.PageObjectModels;

namespace Sbt.Tests.Selenium
{
    internal class ReportScoresTests
    {
        private string _urlBase { get; } = "https://sbt.azurewebsites.net";
        private StandingsPage _standingsPage { get; set; } = default!;
        private CreateDataForSeleniumPage _createDataForSeleniumPage { get; set; } = default!;

        private IWebDriver _driver { get; set; } = default!;
        private int _firstDoubleHeaderGameID = 1;
        private int _firstSingleGameID = 25;

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

            // tests need to start on Standings Page to click on a game for which to report scores
            this._standingsPage = new StandingsPage(this._driver, this._urlBase);
        }

        [Test]
        public void CorrectGameInfoDisplayedForSingleGameTest()
        {
            // verify that clicking on a team name (in a few places)
            //    will move to the ReportScores Page and shows the correct game info:
            // verify that all read-only fields are correct
            // verify that the scores are empty and the forfeit checkboxes are unchecked
            
            TestGame(this._firstSingleGameID);
            TestGame(this._firstSingleGameID+1);
            TestGame(this._firstSingleGameID+2);

            void TestGame(int gameID)
            {
                var gameRecords = this._standingsPage.GetGameRecords(gameID, false);
                var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

                var gameRecordActual = reportScoresPage.GetGameRecord(1);
                this.CompareGameRecords(gameRecordActual, gameRecords[0]);

                this._standingsPage.Load();
            }
        }

        [Test]
        public void CorrectGameInfoDisplayedForDoubleHeaderTest()
        {
            // verify that clicking on a team name (in a few places)
            //    will move to the ReportScores Page and shows the correct game info:
            // verify that all read-only fields are correct
            // verify that the scores are empty and the forfeit checkboxes are unchecked
            // do this for both games of doubleheader

            TestGame(this._firstDoubleHeaderGameID);
            TestGame(this._firstDoubleHeaderGameID+2);
            TestGame(this._firstDoubleHeaderGameID+4);

            TestGame(this._firstDoubleHeaderGameID + 10);
            TestGame(this._firstDoubleHeaderGameID + 12);
            TestGame(this._firstDoubleHeaderGameID + 14);

            void TestGame(int gameID)
            {
                var gameRecords = this._standingsPage.GetGameRecords(gameID, true);
                var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

                var gameRecordActual = reportScoresPage.GetGameRecord(1);
                this.CompareGameRecords(gameRecordActual, gameRecords[0]);
                gameRecordActual = reportScoresPage.GetGameRecord(2);
                this.CompareGameRecords(gameRecordActual, gameRecords[1]);

                this._standingsPage.Load();
            }
        }

        [Test]
        public void InvalidScoreReportForSingleGameTest()
        {
            // this method checks client-side validation, single-game scenario

            // check validation messages when an empty score report is attempted,
            // then enter a valid score, which should clear the validation message,
            // then enter an invalid (non-integer) score - validation message should say that

            var gameID = this._firstSingleGameID;
            var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

            // validation messages should be empty
            var message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Is.Empty);
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);

            // scores should be empty
            var score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.Empty);
            score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.Empty);

            // attempt to save score report ...
            // it should fail with valiation message due to empty scores
            reportScoresPage.ClickSaveButton();
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("required"));
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Contains.Substring("required"));

            // entering a valid score should clear the validation message
            reportScoresPage.EnterScore(1, "3", false);
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);

            // entering an invalid score should display a new and different validation message
            reportScoresPage.EnterScore(1, "3.5", true);
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("integer"));
        }

        [Test]
        public void ScoreReportsWithJavaScriptDisabledTest()
        {
            // test features with JS disabled:
            //   - turn off JavaScript (verify with forfeit checkbox click)
            //    - add scores then click a forfeit checkbox and save scores:
            //        (server-side should override reported scores with 7-0 forfeit score instead)
            //   - verify a game with no scores is not reported, and shows validation message

            var gameID = this._firstSingleGameID;
            var homeScore = "9";
            var visitorScore = "4";

            // minimize the setup-created browser to avoid visual confusion
            this._driver.Manage().Window.Minimize();

            var firefoxOptions = new FirefoxOptions();
            firefoxOptions.AcceptInsecureCertificates = true;
            firefoxOptions.SetPreference("javascript.enabled", false);

            // note - a new driver is being created (necessary to disable JS),
            // so a try-finally is needed to Quit() it
            var driverNoJs = new FirefoxDriver(firefoxOptions);

            try
            {
                var standingsPage = new StandingsPage(driverNoJs, this._urlBase);
                var reportScoresPage = standingsPage.ClickOnGameToReportScores(gameID);

                // confirm that JavaScript is disabled by adding scores, then
                // click on forfeit checkbox with other tests have confirmed
                // should zero-out a score when JS is enabled
                this.EnterAndVerrifyScore(1, false, visitorScore, reportScoresPage);
                this.EnterAndVerrifyScore(1, true, homeScore, reportScoresPage);
                reportScoresPage.ClickForfeitCheckbox(1, true);
                var score = reportScoresPage.GetScore(1, true);
                Assert.That(score, Is.EqualTo(homeScore));

                // save scores then verify 7-0 in standings
                reportScoresPage.ClickSaveButton();

                // verify that the driver is now on the Standings Page
                Assert.That(standingsPage.IsDriverOnPage(), Is.True);

                // verify that the standings page shows the scores we just reported for this game
                var gameRecords = standingsPage.GetGameRecords(gameID, false);
                Assert.That(gameRecords[0].HomeScore, Is.EqualTo("0"));
                Assert.That(gameRecords[0].VisitorScore, Is.EqualTo("7"));

                // verify a game with no scores is not reported, and shows validation message
                gameID = this._firstSingleGameID + 1;
                reportScoresPage = standingsPage.ClickOnGameToReportScores(gameID);

                // score should be empty but enter empty score just to be safe
                this.EnterAndVerrifyScore(1, false, string.Empty, reportScoresPage);
                this.EnterAndVerrifyScore(1, true, string.Empty, reportScoresPage);

                // verify empty validation messages
                var message = reportScoresPage.GetValidationMessage(1, true);
                Assert.That(message, Is.Empty);
                message = reportScoresPage.GetValidationMessage(1, false);
                Assert.That(message, Is.Empty);
                reportScoresPage.ClickSaveButton();

                // now validation messages should not be empty
                message = reportScoresPage.GetValidationMessage(1, true);
                Assert.That(message, Contains.Substring("required"));
                message = reportScoresPage.GetValidationMessage(1, false);
                Assert.That(message, Contains.Substring("required"));
            }
            finally
            {
                driverNoJs.Quit();
            }
        }

        [Test]
        public void InvalidScoreReportdForDoubleHeaderTest()
        {
            // this method checks client-side validation, double-header scenario

            // check validation messages when an empty score report is attempted,
            // then enter a valid score, which should clear the validation message,
            // then enter an invalid (non-integer) score - validation message should say that

            var gameID = this._firstDoubleHeaderGameID;
            var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

            // validation messages should be empty
            var message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Is.Empty);
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);
            
            // check game 2
            message = reportScoresPage.GetValidationMessage(2, true);
            Assert.That(message, Is.Empty);
            message = reportScoresPage.GetValidationMessage(2, false);
            Assert.That(message, Is.Empty);

            // scores should be empty
            var score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.Empty);
            score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.Empty);

            // check game 2
            score = reportScoresPage.GetScore(2, true);
            Assert.That(score, Is.Empty);
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.Empty);

            // attempt to save score report ...
            // it should fail with valiation message due to empty scores
            reportScoresPage.ClickSaveButton();
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("required"));
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Contains.Substring("required"));

            // check game 2
            message = reportScoresPage.GetValidationMessage(2, true);
            Assert.That(message, Contains.Substring("required"));
            message = reportScoresPage.GetValidationMessage(2, false);
            Assert.That(message, Contains.Substring("required"));

            // entering a valid score (game 1) should clear the validation message
            reportScoresPage.EnterScore(1, "3", false);

            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("required"));

            // double-header specific: scores entered for one game should not clear
            // validation messages for the other game (scores should remain empty as well)
            // check game 2
            message = reportScoresPage.GetValidationMessage(2, true);
            Assert.That(message, Contains.Substring("required"));
            message = reportScoresPage.GetValidationMessage(2, false);
            Assert.That(message, Contains.Substring("required"));

            // check game 2
            score = reportScoresPage.GetScore(2, true);
            Assert.That(score, Is.Empty);
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.Empty);

            // entering an invalid score should display a new and different validation message
            // use game 2, home team for this, then verify game 1 did not change
            reportScoresPage.EnterScore(2, "3.5", true);

            message = reportScoresPage.GetValidationMessage(2, true);
            Assert.That(message, Contains.Substring("integer"));
            message = reportScoresPage.GetValidationMessage(2, false);
            Assert.That(message, Contains.Substring("required"));
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.Empty);

            // check game 1
            message = reportScoresPage.GetValidationMessage(1, true);
            Assert.That(message, Contains.Substring("required"));
            score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.Empty);
            message = reportScoresPage.GetValidationMessage(1, false);
            Assert.That(message, Is.Empty);
            score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.EqualTo("3"));
        }

        [Test]
        public void ForfeitCheckboxChangesScoresTest()
        {
            // This test verifies that the javascript code for the forfeit checkboxes
            // correctly zeroes out the appropriate score text box(es).
            // This will only test a doubleheader as it is the more complicated test

            var gameID = this._firstDoubleHeaderGameID;
            var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

            // enter scores for both games, verifying along the way
            EnterAndVerrifyScore(1, false, "3", reportScoresPage);
            EnterAndVerrifyScore(1, true, "8", reportScoresPage);
            EnterAndVerrifyScore(2, false, "15", reportScoresPage);
            EnterAndVerrifyScore(2, true, "12", reportScoresPage);

            // click forfeit check box for visitor in game 1, verify click,
            // then verify that scores are changed appropriately (and only for game 1)
            reportScoresPage.ClickForfeitCheckbox(1, false);
            var forfeit = reportScoresPage.GetForfeit(1, false);
            Assert.That(forfeit, Is.EqualTo(true));
            var score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.EqualTo("0"));
            score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.EqualTo("7"));
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.EqualTo("15"));
            score = reportScoresPage.GetScore(2, true);
            Assert.That(score, Is.EqualTo("12"));

            // repeat for home in game 2
            reportScoresPage.ClickForfeitCheckbox(2, true);
            forfeit = reportScoresPage.GetForfeit(2, true);
            Assert.That(forfeit, Is.EqualTo(true));
            score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.EqualTo("0"));
            score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.EqualTo("7"));
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.EqualTo("7"));
            score = reportScoresPage.GetScore(2, true);
            Assert.That(score, Is.EqualTo("0"));

            // one more time, for visitor for game 2
            reportScoresPage.ClickForfeitCheckbox(2, false);
            forfeit = reportScoresPage.GetForfeit(2, false);
            Assert.That(forfeit, Is.EqualTo(true));
            score = reportScoresPage.GetScore(1, false);
            Assert.That(score, Is.EqualTo("0"));
            score = reportScoresPage.GetScore(1, true);
            Assert.That(score, Is.EqualTo("7"));
            score = reportScoresPage.GetScore(2, false);
            Assert.That(score, Is.EqualTo("0"));
            score = reportScoresPage.GetScore(2, true);
            Assert.That(score, Is.EqualTo("0"));
        }

        [Test]
        public void CorrectGameInfoDisplayedWithScoresReportedTest()
        {
            // verify that doubleheader games with scores reported
            //    display correct information

            var testGameRecords = Utilities.GetTestGameRecords();
            int index = 0;
            var gameID = testGameRecords[index].gameID;
            var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

            this.EnterAndVerifyGameRecord(testGameRecords[index], 1, reportScoresPage);
            this.EnterAndVerifyGameRecord(testGameRecords[index+1], 2, reportScoresPage);
            reportScoresPage.ClickSaveButton();

            // verify that the driver is now on the Standings Page
            Assert.That(this._standingsPage.IsDriverOnPage(), Is.True);

            // verify that the standings page shows the scores we just reported for both games
            var gameRecords = this._standingsPage.GetGameRecords(gameID, true);
            this.CompareGameRecords(gameRecords[0], testGameRecords[index], true);
            this.CompareGameRecords(gameRecords[1], testGameRecords[index+1], true);
        }

        [Test]
        public void ScoresCorrectlyReflectedInStandingsTest()
        {
            // report scores for several games, with and without forfeits, and
            // verify the standings were properly calculated. 

            var testGameRecords = Utilities.GetTestGameRecords();
            for (int index = 0; index < testGameRecords.Length; index = index + 2)
            {

                var gameID = testGameRecords[index].gameID;
                var reportScoresPage = this._standingsPage.ClickOnGameToReportScores(gameID);

                this.EnterAndVerifyGameRecord(testGameRecords[index], 1, reportScoresPage);
                this.EnterAndVerifyGameRecord(testGameRecords[index + 1], 2, reportScoresPage);
                reportScoresPage.ClickSaveButton();
            }

            // verify that the driver is now on the Standings Page
            Assert.That(this._standingsPage.IsDriverOnPage(), Is.True);

            // finished reporting scores, verify standings
            
            // get standings records from standings page, and test data for comparison
            var standingsRecordsExpected = Utilities.GetTestStandingsRecords();
            var standingsRecordsActual = this._standingsPage.GetStandingsRecords();
            Assert.That(standingsRecordsActual.Length, Is.EqualTo(standingsRecordsExpected.Length));
            
            // loop through and compare each index in the two arrays
            for (int index = 0; (index < standingsRecordsActual.Length); index++)
            {
                this.CompareStandingsRecord(standingsRecordsActual[index], standingsRecordsExpected[index]);
            }
        }

        [TearDown]
        public void TearDown()
        {
            this._createDataForSeleniumPage.Load();
            this._createDataForSeleniumPage.DaleteData();
            this._driver.Quit();
        }

        #region Helper Methods
        private void EnterAndVerifyGameRecord(GameRecord gameRecord, int gameNumber, ReportScoresPage reportScoresPage)
        {
            if(gameRecord.HomeForfeit || gameRecord.VisitorForfeit)
            {
                // we will enter scores even though one or both may be overridden by clicking on forfeit
                this.EnterAndVerrifyScore(gameNumber, false, gameRecord.VisitorScore, reportScoresPage);
                this.EnterAndVerrifyScore(gameNumber, true, gameRecord.HomeScore, reportScoresPage);
                if (gameRecord.HomeForfeit)
                {
                    reportScoresPage.ClickForfeitCheckbox(gameNumber, true);
                }
                if (gameRecord.VisitorForfeit)
                {
                    reportScoresPage.ClickForfeitCheckbox(gameNumber, false);
                }
            }
            else
            {
                this.EnterAndVerrifyScore(gameNumber, false, gameRecord.VisitorScore, reportScoresPage);
                this.EnterAndVerrifyScore(gameNumber, true, gameRecord.HomeScore, reportScoresPage);
            }
        }

        private void EnterAndVerrifyScore(int gameNumber, bool home, string scoreToReport, ReportScoresPage reportScoresPage)
        {
            reportScoresPage.EnterScore(gameNumber, scoreToReport, home);
            var score = reportScoresPage.GetScore(gameNumber, home);
            Assert.That(score, Is.EqualTo(scoreToReport));
        }

        private void CompareGameRecords( GameRecord gameRecordActual, GameRecord gameRecordExpected, bool ignoreForfeits = false)
        {
            Assert.That(gameRecordActual.Date, Is.EqualTo(gameRecordExpected.Date));
            Assert.That(gameRecordActual.Field, Is.EqualTo(gameRecordExpected.Field));
            Assert.That(gameRecordActual.Time, Is.EqualTo(gameRecordExpected.Time));
            Assert.That(gameRecordActual.HomeTeam, Is.EqualTo(gameRecordExpected.HomeTeam));
            Assert.That(gameRecordActual.VisitorTeam, Is.EqualTo(gameRecordExpected.VisitorTeam));
            Assert.That(gameRecordActual.HomeScore, Is.EqualTo(gameRecordExpected.HomeScore));
            Assert.That(gameRecordActual.VisitorScore, Is.EqualTo(gameRecordExpected.VisitorScore));
            if (ignoreForfeits == false)
            {
                // standings page does not show forfeits
                Assert.That(gameRecordActual.HomeForfeit, Is.EqualTo(gameRecordExpected.HomeForfeit));
                Assert.That(gameRecordActual.VisitorForfeit, Is.EqualTo(gameRecordExpected.VisitorForfeit));
            }
        }

        private void CompareStandingsRecord(StandingsRecord recordActual, StandingsRecord recordExpected)
        {
            Assert.That(recordActual.Team, Is.EqualTo(recordExpected.Team));
            Assert.That(recordActual.Wins, Is.EqualTo(recordExpected.Wins));
            Assert.That(recordActual.Losses, Is.EqualTo(recordExpected.Losses));
            Assert.That(recordActual.Ties, Is.EqualTo(recordExpected.Ties));
            Assert.That(recordActual.WinPercentage, Is.EqualTo(recordExpected.WinPercentage));
            Assert.That(recordActual.GamesBehind, Is.EqualTo(recordExpected.GamesBehind));
            Assert.That(recordActual.RunsScored, Is.EqualTo(recordExpected.RunsScored));
            Assert.That(recordActual.RunsAgainst, Is.EqualTo(recordExpected.RunsAgainst));
            Assert.That(recordActual.Forfeits, Is.EqualTo(recordExpected.Forfeits));
        }
        #endregion
    }
}