using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sbt.Pages.Admin.LoadSchedule
{
    public class LoadScheduleModel : Sbt.Pages.Admin.AdminPageModel
    {
        [BindProperty]
        [Required]
        [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9-_]*$")]
        [StringLength(50, MinimumLength = 2)]
        public string League { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [DisplayName("Schedule File")]
        public IFormFile ScheduleFile { get; set; } = default!;

        [BindProperty]
        public bool UsesDoubleHeaders { get; set; } = false;

        public string Result { get; set; } = string.Empty;

        public LoadScheduleModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        // Note - using base class version of OnGetAsync()

        public async Task<IActionResult> OnPostAsync(string organization)
        {
            // submit button should be disbled if true, but protect against other entries
            if (base.DisableSubmitButton == true)
            {
                return Page();
            }

            List<string> lines = new();
            
            this.Result = string.Empty;

            if ( organization == null ||  this.ScheduleFile == null || this.ScheduleFile.Length == 0)
            {
                return Page();
            }

            base.Organization = organization;

            // NOTE - Game IDs are unique only within an Organization, simply for aesthetic reasons
            // (just so the number doesn't become huge over time). 
            // However, I was unable to find a database-agnostic way to do this within EF Core,
            // which means it will be handled here, when I populate the Schedule table.

            // get max game id within this organization
            var maxCurrentGameID = await base._context.Schedules
                .Where(s => s.Organization == organization)
                .Select(s => (int?)s.GameID)
                .MaxAsync();

            int maxGameID = maxCurrentGameID ?? 0;
            maxGameID++;
            
            using (var reader = new StreamReader(ScheduleFile.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    lines.Add(reader.ReadLine()!);
            }

            try
            {
                // Note - expecting a properly formatted file since it is self-created,
                // solely for the purposes of populating some demo data for the website.
                // therefore no error-checking is done here - just wrapping in try-catch
                // and displaying exceptions to the user

                List<string> teams = new();
                int lineNumber = 0;
                short teamID = 1;

                DateTime firstGameDate = DateTime.MinValue;
                DateTime lastGameDate = DateTime.MinValue;

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
                        Division = League,
                        Name = lines[lineNumber].Trim(),
                        TeamID = teamID++
                    };
                    base._context.Standings.Add(standingsRow);
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
                        this.AddWeekBoundary(data[0], maxGameID, League, organization);
                        maxGameID++;
                        continue;
                    }
                    DateTime gameDate = DateTime.Parse(data[0]);
                    // index 1 is day-of-week, not currently used in this simplified version of the website
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
                        Division = League,
                        Field = field,
                        Home = teams[homeTeamID - 1],
                        HomeForfeit = false,
                        HomeID = homeTeamID,
                        Time = gameTime,
                        Visitor = teams[visitorTeamID - 1],
                        VisitorForfeit = false,
                        VisitorID = visitorTeamID,
                    };
                    base._context.Schedules.Add(scheduleRow);

                    if (this.UsesDoubleHeaders)
                    {
                        // add a second game 90 minutes later, swapping home/visitor
                        scheduleRow = new Schedules
                        {
                            Organization = organization,
                            GameID = maxGameID++,
                            Day = gameDate,
                            Division = League,
                            Field = field,
                            Home = teams[visitorTeamID - 1],
                            HomeForfeit = false,
                            HomeID = visitorTeamID,
                            Time = gameTime.AddMinutes(90),
                            Visitor = teams[homeTeamID - 1],
                            VisitorForfeit = false,
                            VisitorID = homeTeamID,
                        };
                        base._context.Schedules.Add(scheduleRow);
                    }

                    // keep track of first and last games to show when done processing file,
                    // as a way to show user that the entire schedule was processed.
                    if (index == lineNumber + 1)
                    {
                        firstGameDate = gameDate;
                    }
                    else if (index == lines.Count - 1)
                    {
                        lastGameDate = gameDate;
                    }
                } // for loop for schedule data

                await base._context.SaveChangesAsync();

                this.Result = "Success loading schedule from " + ScheduleFile.FileName +
                    ". <br>Games start on " + firstGameDate.ToShortDateString() + 
                    " and end on " + lastGameDate.ToShortDateString();
                return Page();
            }
            catch (Exception ex)
            {
                if (ex.InnerException != null)
                {
                    this.Result = ex.Message + ":<br>" + ex.InnerException.Message;
                }
                else
                {
                    this.Result = ex.Message;
                }
                return Page();
            }
        }

        private void AddWeekBoundary(string week, int maxGameID, string league, string organization)
        {
            // this creates a mostly empty "WEEK #" row to make it easier to show
            // week boundaries when displaying the schedule.
            var scheduleRow = new Schedules
            {
                Organization = organization,
                GameID = maxGameID,
                Division = league,
                HomeForfeit = false,
                Visitor = week,
                VisitorForfeit = false,
            };
            base._context.Schedules.Add(scheduleRow);
        }
    }
}
