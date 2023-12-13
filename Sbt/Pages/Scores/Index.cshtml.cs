using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Scores
{
    public class IndexModel : PageModel
    {
        private readonly Sbt.Data.DemoContext _context;

        [BindProperty]
        public IList<Sbt.Schedules> Schedules { get; set; } = default!;

        [BindProperty]
        public IList<Sbt.SchedulesVM> SchedulesVM { get; set; } = default!;

        private bool ShowOvertimeLosses { get; set; } = false;

        public IndexModel(Sbt.Data.DemoContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync(string organization, string divisionID)
        {
            if (this._context.Schedules != null && organization != null && divisionID != null)
            {
                int gameID = 0;
                if (Request.Query.TryGetValue("gameID", out var gameIDString) == false ||
                    int.TryParse(gameIDString, out gameID) == false)
                {
                    return NotFound();
                }

                var gameInfo = await this._context.Schedules.FirstOrDefaultAsync(
                    s => s.Organization == organization && s.Division == divisionID && s.GameID == gameID);

                if (gameInfo == null)
                {
                    return NotFound();
                }

                // now find all games on that day at that field
                this.Schedules = await this._context.Schedules
                    .Where(s => s.Organization == organization && s.Division == divisionID &&
                    s.Day == gameInfo.Day && s.Field == gameInfo.Field)
                    .OrderBy(s => s.Time)
                    .ToListAsync();

                // populate ViewModel (which is used to prevent overposting)
                this.SchedulesVM = new List<Sbt.SchedulesVM>();
                for (int i = 0; i < this.Schedules.Count; i++)
                {
                    var scheduleVM = new SchedulesVM();
                    scheduleVM.GameID = this.Schedules[i].GameID;
                    scheduleVM.HomeScore = this.Schedules[i].HomeScore;
                    scheduleVM.VisitorScore = this.Schedules[i].VisitorScore;
                    scheduleVM.HomeForfeit = this.Schedules[i].HomeForfeit;
                    scheduleVM.VisitorForfeit = this.Schedules[i].VisitorForfeit;
                    this.SchedulesVM.Add(scheduleVM);
                }

                this.DetermineOvertimeLossVisibility();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string organization, string divisionID)
        {
            if (!ModelState.IsValid || this._context.Schedules == null || 
                this.Schedules == null || this.Schedules.Count == 0)
            {
                return Page();
            }

            for (int i = 0; i < this.Schedules.Count; i++)
            {
                var gameInfo = await this._context.Schedules.FirstOrDefaultAsync(
                    s => s.Organization == organization && s.Division == divisionID &&
                    s.GameID == this.SchedulesVM[i].GameID);

                if (gameInfo == null)
                {
                    continue;
                }

                // populate Model from ViewModel (which is used to prevent overposting)
                this._context.Entry(gameInfo).CurrentValues.SetValues(this.SchedulesVM[i]);
                
                if (gameInfo.VisitorForfeit)
                {
                    gameInfo.VisitorScore = 0;
                    gameInfo.HomeScore = (gameInfo.HomeForfeit) ? (short)0 : (short)7;
                }
                else if (gameInfo.HomeForfeit)
                {
                    gameInfo.VisitorScore = 7;
                    gameInfo.HomeScore = 0;
                }

                await this._context.SaveChangesAsync();
            } // for each game in list

            await this.ReCalcStandings(this.Schedules[0].Organization, this.Schedules[0].Division);
            await this._context.SaveChangesAsync();
            await this._context.Divisions
                .Where(d => d.Organization == organization && d.ID == divisionID)
                .ExecuteUpdateAsync(s => s.SetProperty(c => c.Updated, c => this.GetEasternTime()));

            return RedirectToPage("/Standings/Index", 
                new { organization = this.Schedules[0].Organization, id = this.Schedules[0].Division });
        }

        private void DetermineOvertimeLossVisibility()
        {
            // in a production system this would be handled more generically,
            // but for now we are just checking if Org contains "Hockey"
            this.ShowOvertimeLosses = this.Schedules[0].Organization.ToLower().Contains("hockey");
        }

        private async Task ReCalcStandings( string organization, string divisionID)
        {
            var standings = await this._context.Standings
                .Where(s => s.Organization == organization && s.Division == divisionID)
                .OrderBy(s => s.TeamID)
                .ToListAsync();

            var schedules = await this._context.Schedules
                .Where(s => s.Organization == organization && s.Division == divisionID)
                .OrderBy(s => s.GameID)
                .ToListAsync() ;

            // zero-out standings
            foreach ( var stand in standings) 
            {
                stand.Forfeits = stand.Losses = stand.OvertimeLosses = stand.Ties = stand.Wins = 0;
                stand.RunsAgainst = stand.RunsScored = stand.ForfeitsCharged = 0;
                stand.GB = stand.Percentage = 0;
            }

            foreach( var sched in schedules)
            {
                // skip week boundary
                if (sched.Visitor.ToUpper().StartsWith("WEEK") == true) continue;

                this.UpdateStandings(standings, sched);
            }
        }

        private void UpdateStandings(List<Sbt.Standings> standings, Sbt.Schedules sched)
        {
            // note - IList starts at 0, team IDs start at 1
            var homeTteam = standings[sched.HomeID - 1];
            var visitorTeam = standings[sched.VisitorID - 1];

            if (sched.HomeScore > -1) // this will catch null values (no scores reported yet)
            {
                homeTteam.RunsScored += (short)sched.HomeScore!;
                homeTteam.RunsAgainst += (short)sched.VisitorScore!;
                visitorTeam.RunsScored += (short)sched.VisitorScore!;
                visitorTeam.RunsAgainst += (short)sched.HomeScore!;
            }

            if (sched.HomeForfeit)
            {
                homeTteam.Forfeits++;
                homeTteam.ForfeitsCharged++;
            }
            if (sched.VisitorForfeit)
            {
                visitorTeam.Forfeits++;
                visitorTeam.ForfeitsCharged++;
            }

            if (sched.VisitorForfeit && sched.HomeForfeit)
            {
                // special case - not a tie - counted as losses for both team
                homeTteam.Losses++;
                visitorTeam.Losses++;
            }
            else if (sched.HomeScore > sched.VisitorScore)
            {
                homeTteam.Wins++;
                visitorTeam.Losses++;
            }
            else if(sched.HomeScore < sched.VisitorScore) 
            { 
                homeTteam.Losses++;
                visitorTeam.Wins++; 
            }
            else if (sched.HomeScore > -1) // this will catch null values (no scores reported yet)
            {
                homeTteam.Ties++;
                visitorTeam.Ties++;
            }

            // calculate Games Behind (GB)
            var sortedTeams = standings.OrderByDescending(t => t.Wins).ToList();
            var maxWins = sortedTeams.First().Wins;
            var maxLosses = sortedTeams.First().Losses;
            foreach (var team in sortedTeams)
            {
                team.GB = ((maxWins - team.Wins) + (team.Losses - maxLosses)) / 2.0f;
                if ((team.Wins + team.Losses) == 0)
                {
                    team.Percentage = 0.0f;
                }
                else
                {
                    team.Percentage = (float)team.Wins / (team.Wins + team.Losses + team.Ties);
                }
            }
        }

        private DateTime GetEasternTime()
        {
            DateTime utcTime = DateTime.UtcNow;

            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
        }

    }
}
