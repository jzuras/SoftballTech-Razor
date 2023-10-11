using HtmlAgilityPack;

namespace Sbt.Tests.Selenium
{
    public record GameRecord(
        string VisitorTeam, string VisitorScore, string HomeTeam, string HomeScore, string Date, string Field, string Time,
        bool VisitorForfeit = false, bool HomeForfeit = false, int gameID = 0);

    public record StandingsRecord(
        string Team, string Wins, string Losses, string Ties, string WinPercentage, string GamesBehind,
        string RunsScored, string RunsAgainst, string Forfeits);

    internal class Utilities
    {
        internal static GameRecord HtmlToGameRecord(string htmlFromStandingsPage)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlFromStandingsPage);

            var tdElements = doc.DocumentNode.SelectNodes("//td");

            // team names are inside <a> tags
            string visitorTeam = tdElements[0].InnerText.Trim();
            string homeTeam = tdElements[2].InnerText.Trim();

            string visitorScore = tdElements[1].InnerText.Trim();
            string homeScore = tdElements[3].InnerText.Trim();
            string date = tdElements[4].InnerText.Trim();
            string field = tdElements[5].InnerText.Trim();
            string time = tdElements[6].InnerText.Trim();

            return new GameRecord(visitorTeam, visitorScore, homeTeam, homeScore, date, field, time);
        }

        internal static StandingsRecord[] HtmlToStandingsRecord(string htmlFromStandingsPage)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(htmlFromStandingsPage);

            var records = new List<StandingsRecord>();
            var rows = doc.DocumentNode.SelectNodes("//table/tbody/tr");
            foreach (var row in rows)
            {
                var columns = row.SelectNodes("td");
                var record = new StandingsRecord(
                    columns[0].InnerText.Trim(), // team name
                    columns[1].InnerText.Trim(), // wins
                    columns[2].InnerText.Trim(), // losses
                    columns[3].InnerText.Trim(), // ties
                    columns[4].InnerText.Trim(), // win %
                    columns[5].InnerText.Trim(), // games behind
                    columns[6].InnerText.Trim(), // runs scored
                    columns[7].InnerText.Trim(), // runs against
                    columns[8].InnerText.Trim() // forfeits
                );
                records.Add(record);
            }

            return records.ToArray();
        }

        internal static GameRecord[] GetTestGameRecords()
        {
            // Create and return game records with scores and forfeits for test purposes - all double-headers
            // note - this data must exactly match the test data created by the Selenium Test Org page,
            // as the Standings computation comparison must match team names with W/L records, etc.

            return new GameRecord[]
            {
                new GameRecord("Green", "6", "Red", "14", "Sep-10", "BRD 1", "9:30 AM", false, false, 1),
                new GameRecord("Red", "12", "Green", "15", "Sep-10", "BRD 1", "11:00 AM", false, false, 2),
                new GameRecord("Maroon", "17", "Silver", "6", "Sep-10", "BRD 2", "9:30 AM", false, false, 3),
                new GameRecord("Silver", "3", "Maroon", "9", "Sep-10", "BRD 2", "11:00 AM", false, false, 4),
                new GameRecord("Royal Blue", "7", "Light Blue", "12", "Sep-10", "BRD 3", "9:30 AM", false, false, 5),
                new GameRecord("Light Blue", "16", "Royal Blue", "10", "Sep-10", "BRD 3", "11:00 AM", false, false, 6),

                new GameRecord("Maroon", "8", "Red", "14", "Sep-12", "BRD 1", "9:30 AM", false, false, 7),
                new GameRecord("Red", "12", "Maroon", "1", "Sep-12", "BRD 1", "11:00 AM", false, false, 8),
                new GameRecord("Light Blue", "11", "Green", "17", "Sep-12", "BRD 2", "9:30 AM", false, false, 9),
                new GameRecord("Green", "15", "Light Blue", "15", "Sep-12", "BRD 2", "11:00 AM", false, false, 10),
                new GameRecord("Silver", "9", "Royal Blue", "14", "Sep-12", "BRD 3", "9:30 AM", false, false, 11),
                new GameRecord("Royal Blue", "7", "Silver", "0", "Sep-12", "BRD 3", "11:00 AM", false, true, 12),

                new GameRecord("Silver", "19", "Red", "11", "Sep-17", "BRD 1", "9:30 AM", false, false, 13),
                new GameRecord("Red", "11", "Silver", "8", "Sep-17", "BRD 1", "11:00 AM", false, false, 14),
                new GameRecord("Light Blue", "9", "Maroon", "16", "Sep-17", "BRD 2", "9:30 AM", false, false, 15),
                new GameRecord("Maroon", "6", "Light Blue", "10", "Sep-17", "BRD 2", "11:00 AM", false, false, 16),
                new GameRecord("Royal Blue", "17", "Green", "10", "Sep-17", "BRD 3", "9:30 AM", false, false, 17),
                // note - scores for game below should be ignored due to double-forfeit,
                // comparing standings column for runs scored will confirm this works as expected
                new GameRecord("Green", "999", "Royal Blue", "888", "Sep-17", "BRD 3", "11:00 AM", true, true, 18)
            };
        }

        internal static StandingsRecord[] GetTestStandingsRecords()
        {
            // this data is what the standings results should be after the scores are reported
            // for ALL games defined in GetTestGameRecords()

            return new StandingsRecord[]
            {
                new StandingsRecord("Red", "4", "2", "0", ".667", "0", "74", "57", "0"),
                new StandingsRecord("Light Blue", "3", "2", "1", ".500", "0.5", "73", "71", "0"),
                new StandingsRecord("Maroon", "3", "3", "0", ".500", "1", "57", "54", "0"),
                new StandingsRecord("Royal Blue", "3", "3", "0", ".500", "1", "55", "47", "1"),
                new StandingsRecord("Green", "2", "3", "1", ".333", "1.5", "63", "69", "1"),
                new StandingsRecord("Silver", "1", "5", "0", ".167", "3", "45", "69", "1")
            };
        }
    }
}
