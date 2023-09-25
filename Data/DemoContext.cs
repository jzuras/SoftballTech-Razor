using Microsoft.EntityFrameworkCore;

namespace Sbt.Data
{
    public class DemoContext : DbContext
    {
        public record LoadScheduleResult
        {
            public bool Success { get; init; }
            public string ErrorMessage { get; init; } = string.Empty;
            public DateTime FirstGameDate { get; init; }
            public DateTime LastGameDate { get; init; }
        };

        public DemoContext(DbContextOptions<DemoContext> options)
            : base(options)
        {
        }

        public DbSet<Sbt.Divisions> Divisions { get; set; } = default!;
        public DbSet<Sbt.Schedules> Schedules { get; set; } = default!;
        public DbSet<Sbt.Standings> Standings { get; set; } = default!;

        #region Data Access Layer Methods
        public async Task<LoadScheduleResult> LoadScheduleFileAsync(IFormFile scheduleFile, string organization, string divisionID, 
            bool usesDoubleHeaders, bool clearScheduleFirst)
        {
            string error = string.Empty;
            DateTime firstGameDate = DateTime.MinValue;
            DateTime lastGameDate = DateTime.MinValue;
            List<string> lines = new();

            if(clearScheduleFirst)
            {
                try
                {
                    await this.DeleteScheduleAndStandingsAsync(organization, divisionID);
                }
                catch (Exception ex)
                {
                    if (ex.InnerException != null)
                    {
                        error = ex.Message + ":<br>" + ex.InnerException.Message;
                    }
                    else
                    {
                        error = ex.Message;
                    }

                    return new LoadScheduleResult
                    {
                        Success = false,
                        ErrorMessage = error,
                        FirstGameDate = firstGameDate,
                        LastGameDate = lastGameDate
                    };
                }
            }

            // NOTE - Game IDs are unique only within an Organization, simply for aesthetic reasons
            // (just so the number doesn't become huge over time). 
            // However, I was unable to find a database-agnostic way to do this within EF Core,
            // which means it will be handled here, when I populate the Schedule table.

            // get max game id within this organization
            var maxCurrentGameID = await this.Schedules
                .Where(s => s.Organization == organization)
                .Select(s => (int?)s.GameID)
                .MaxAsync();

            int maxGameID = maxCurrentGameID ?? 0;
            maxGameID++;

            using (var reader = new StreamReader(scheduleFile.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    lines.Add(reader.ReadLine()!);
            }

            try
            {
                // Note - expecting a properly formatted file since it is self-created,
                // solely for the purposes of populating some demo data for the website.
                // therefore no error-checking is done here - just wrapping in try-catch
                // and returning exceptions to the calling method

                List<string> teams = new();
                int lineNumber = 0;
                short teamID = 1;

                // skip first 4 lines which are simply for ease of reading the file
                lineNumber = 4;

                // next lines are teams - ended by blank line
                // team IDs are assumed, starting at 1
                while (lines[lineNumber].Length > 0)
                {
                    teams.Add(lines[lineNumber].Trim());

                    // create standings row for each team
                    var standingsRow = new Sbt.Standings
                    {
                        Organization = organization,
                        Wins = 0,
                        Losses = 0,
                        Ties = 0,
                        OvertimeLosses = 0,
                        Percentage = 0,
                        GB = 0,
                        RunsAgainst = 0,
                        RunsScored = 0,
                        Forfeits = 0,
                        ForfeitsCharged = 0,
                        Division = divisionID,
                        Name = lines[lineNumber].Trim(),
                        TeamID = teamID++
                    };
                    this.Standings.Add(standingsRow);
                    lineNumber++;
                }

                // rest of file is the actual schedule, in this format:
                // Date,Day,Time,Home,Visitor,Field
                for (int index = lineNumber + 1; index < lines.Count; index++)
                {
                    string[] data = lines[index].Split(',');

                    if (data[0].ToLower().StartsWith("week"))
                    {
                        // original code had complicated method to determine week boundaries,
                        // but for simplicity's sake I am adding this info in the schedule files
                        this.AddWeekBoundary(data[0], maxGameID, divisionID, organization);
                        maxGameID++;
                        continue;
                    }
                    DateTime gameDate = DateTime.Parse(data[0]);
                    // skipping value at [1] - not currently used in this version of the website
                    DateTime gameTime = DateTime.Parse(data[2]);
                    short homeTeamID = short.Parse(data[3]);
                    short visitorTeamID = short.Parse(data[4]);
                    string field = data[5];

                    // create schedule row for each game
                    var scheduleRow = new Schedules
                    {
                        Organization = organization,
                        GameID = maxGameID++,
                        Day = gameDate,
                        Division = divisionID,
                        Field = field,
                        Home = teams[homeTeamID - 1],
                        HomeForfeit = false,
                        HomeID = homeTeamID,
                        Time = gameTime,
                        Visitor = teams[visitorTeamID - 1],
                        VisitorForfeit = false,
                        VisitorID = visitorTeamID,
                    };
                    this.Schedules.Add(scheduleRow);

                    if (usesDoubleHeaders)
                    {
                        // add a second game 90 minutes later, swapping home/visitor
                        scheduleRow = new Schedules
                        {
                            Organization = organization,
                            GameID = maxGameID++,
                            Day = gameDate,
                            Division = divisionID,
                            Field = field,
                            Home = teams[visitorTeamID - 1],
                            HomeForfeit = false,
                            HomeID = visitorTeamID,
                            Time = gameTime.AddMinutes(90),
                            Visitor = teams[homeTeamID - 1],
                            VisitorForfeit = false,
                            VisitorID = homeTeamID,
                        };
                        this.Schedules.Add(scheduleRow);
                    }

                    // keep track of first and last games to show when done processing file,
                    // as a way to show user that the entire schedule was processed.
                    if (index == lineNumber + 2)
                    {
                        firstGameDate = gameDate;
                    }
                    else if (index == lines.Count - 1)
                    {
                        lastGameDate = gameDate;
                    }
                } // for loop for schedule data

                await this.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    error = ex.Message + ":<br>" + ex.InnerException.Message;
                }
                else
                {
                    error = ex.Message;
                }
            }

            return new LoadScheduleResult
            {
                Success = (error == string.Empty) ? true : false,
                ErrorMessage = error,
                FirstGameDate = firstGameDate,
                LastGameDate = lastGameDate
            };
        }

        private async Task DeleteScheduleAndStandingsAsync(string organization, string divisionID)
        {
            // first delete schedule for this division, then repeat for standings
            var schedulesToDelete = await this.Schedules
                .Where(s => s.Organization == organization && s.Division == divisionID)
                .ToListAsync();

            // Mark retrieved schedules for deletion
            this.Schedules.RemoveRange(schedulesToDelete);

            var standingsToDelete = await this.Standings
                .Where(s => s.Organization == organization && s.Division == divisionID)
                .ToListAsync();

            // Mark retrieved schedules for deletion
            this.Standings.RemoveRange(standingsToDelete);

            // Save changes to the database
            await this.SaveChangesAsync();
        }

        private void AddWeekBoundary(string week, int maxGameID, string divisionID, string organization)
        {
            // this creates a mostly empty "WEEK #" row to make it easier to show
            // week boundaries when displaying the schedule.
            var scheduleRow = new Schedules
            {
                Organization = organization,
                GameID = maxGameID,
                Division = divisionID,
                HomeForfeit = false,
                Visitor = week,
                VisitorForfeit = false,
            };
            this.Schedules.Add(scheduleRow);
        }
        #endregion
    }
}
