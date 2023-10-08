using Microsoft.AspNetCore.Mvc;

namespace Sbt.Pages.Admin.CreateDataForSelenium
{
    public class IndexModel : Sbt.Pages.Admin.AdminPageModel
    {
        private string _testOrganization { get; } = "TestOrgForSelenium";
        private string _testDivisionID { get; } = "Selenium01";
        public string Result { get; set; } = string.Empty;

        public IndexModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        override public async Task<IActionResult> OnGetAsync(string organization = "", string id = "")
        {
            return await Task.FromResult(Page());
        }

        public async Task<IActionResult> OnPostAsync(string handler)
        {
            switch (handler)
            {
                case "CreateDataAsync":
                    await CreateDataAsync();
                    break;

                case "DeleteDataAsync":
                    await DeleteDataAsync();
                    break;

                default:
                    return Page();
            }

            return Page();
        }

        private async Task CreateDataAsync()
        {
            // Note - any changes to the data created here must also be made in the
            // Utilities.GetTestGameRecords() method of the Selenium test project

            var divisions = new Sbt.Divisions[]
            {
                new Sbt.Divisions()
                {
                    Organization = this._testOrganization,
                    ID = this._testDivisionID,
                    League = "league",
                    Division = "1",
                    Updated = DateTime.Now,
                    Locked = false
                }
            };

            base._context.Divisions.AddRange(divisions);

            var standings = new Sbt.Standings[]
            {
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Red",TeamID=1 },
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Green",TeamID=2 },
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Silver",TeamID=3 },
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Maroon",TeamID=4 },
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Light Blue",TeamID=5 },
                new Sbt.Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
                Organization=this._testOrganization,
                Forfeits=0,ForfeitsCharged=0,Division=this._testDivisionID,Name="Royal Blue",TeamID=6 }
            };

            base._context.Standings.AddRange(standings);

            int gameID = 1;
            var schedule = new Sbt.Schedules[]
            {
                #region Week 1
                #region Week 1 First Day
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Red",HomeID=1,Visitor="Green",VisitorID=2,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 09:30"),Field="BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Green",HomeID=2,Visitor="Red",VisitorID=1,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 11:00"),Field="BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Silver",HomeID=3,Visitor="Maroon",VisitorID=4,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 09:30"),Field="BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Maroon",HomeID=4,Visitor="Silver",VisitorID=3,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 11:00"),Field="BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Light Blue",HomeID=5,Visitor="Royal Blue",VisitorID=6,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 09:30"),Field="BRD 3"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,Division=this._testDivisionID,GameID=gameID++,
                    Home="Royal Blue",HomeID=6,Visitor="Light Blue",VisitorID=5,
                    Day=DateTime.Parse("09/10/2023"), Time=DateTime.Parse("09/10/2023 11:00"),Field="BRD 3"
                },
                #endregion
                #region Week 1 Second Day
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Red",
                    HomeID = 1,
                    Visitor = "Maroon", // Changed opponent
                    VisitorID = 4,       // New opponent's ID
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 09:30"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Maroon",  // Changed opponent
                    HomeID = 4,       // New opponent's ID
                    Visitor = "Red",
                    VisitorID = 1,
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 11:00"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Green",
                    HomeID = 2,
                    Visitor = "Light Blue", // Changed opponent
                    VisitorID = 5,           // New opponent's ID
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 09:30"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Light Blue",  // Changed opponent
                    HomeID = 5,           // New opponent's ID
                    Visitor = "Green",
                    VisitorID = 2,
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 11:00"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Royal Blue",
                    HomeID = 6,
                    Visitor = "Silver", // Changed opponent
                    VisitorID = 3,      // New opponent's ID
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 09:30"),
                    Field = "BRD 3" // Assuming a different field
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Silver", // Changed opponent
                    HomeID = 3,      // New opponent's ID
                    Visitor = "Royal Blue",
                    VisitorID = 6,
                    Day = DateTime.Parse("09/12/2023"),
                    Time = DateTime.Parse("09/12/2023 11:00"),
                    Field = "BRD 3" // Assuming a different field
                },
                #endregion
                #endregion

                #region Week 2
                #region Week 2 First Day
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Red",
                    HomeID = 1,
                    Visitor = "Silver",
                    VisitorID = 3,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 09:30"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Silver",
                    HomeID = 3,
                    Visitor = "Red",
                    VisitorID = 1,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 11:00"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Maroon",
                    HomeID = 4,
                    Visitor = "Light Blue",
                    VisitorID = 5,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 09:30"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Light Blue",
                    HomeID = 5,
                    Visitor = "Maroon",
                    VisitorID = 4,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 11:00"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Green",
                    HomeID = 2,
                    Visitor = "Royal Blue",
                    VisitorID = 6,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 09:30"),
                    Field = "BRD 3"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Royal Blue",
                    HomeID = 6,
                    Visitor = "Green",
                    VisitorID = 2,
                    Day = DateTime.Parse("09/17/2023"),
                    Time = DateTime.Parse("09/17/2023 11:00"),
                    Field = "BRD 3"
                },
                #endregion
                #region Week 2 Second Day
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Red",
                    HomeID = 1,
                    Visitor = "Light Blue",
                    VisitorID = 5,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 09:30"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Light Blue",
                    HomeID = 5,
                    Visitor = "Red",
                    VisitorID = 1,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 11:00"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Silver",
                    HomeID = 3,
                    Visitor = "Green",
                    VisitorID = 2,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 09:30"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Green",
                    HomeID = 2,
                    Visitor = "Silver",
                    VisitorID = 3,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 11:00"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Maroon",
                    HomeID = 4,
                    Visitor = "Royal Blue",
                    VisitorID = 6,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 09:30"),
                    Field = "BRD 3"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Royal Blue",
                    HomeID = 6,
                    Visitor = "Maroon",
                    VisitorID = 4,
                    Day = DateTime.Parse("09/19/2023"),
                    Time = DateTime.Parse("09/19/2023 11:00"),
                    Field = "BRD 3"
                },
                #endregion

                #region Week 3
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Red",
                    HomeID = 1,
                    Visitor = "Royal Blue",
                    VisitorID = 6,
                    Day = DateTime.Parse("09/24/2023"),
                    Time = DateTime.Parse("09/24/2023 09:30"),
                    Field = "BRD 1"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Maroon",
                    HomeID = 4,
                    Visitor = "Green",
                    VisitorID = 2,
                    Day = DateTime.Parse("09/24/2023"),
                    Time = DateTime.Parse("09/24/2023 09:30"),
                    Field = "BRD 2"
                },
                new Sbt.Schedules
                {
                    Organization = this._testOrganization,
                    Division = this._testDivisionID,
                    GameID = gameID++,
                    Home = "Light Blue",
                    HomeID = 5,
                    Visitor = "Silver",
                    VisitorID = 3,
                    Day = DateTime.Parse("09/24/2023"),
                    Time = DateTime.Parse("09/24/2023 11:00"),
                    Field = "BRD 3"
                }
                #endregion
                #endregion
            };

            base._context.Schedules.AddRange(schedule);

            await base._context.SaveChangesAsync(); 
            
            this.Result = "Created";
        }

        private async Task DeleteDataAsync()
        {
            // delete everything from test org
            base._context.Divisions.RemoveRange(base._context.Divisions.Where(d => d.Organization == this._testOrganization &&
                d.ID == this._testDivisionID));
            base._context.Standings.RemoveRange(base._context.Standings.Where(s => s.Organization == this._testOrganization &&
                    s.Division == this._testDivisionID));
            base._context.Schedules.RemoveRange(base._context.Schedules.Where(s => s.Organization == this._testOrganization &&
                    s.Division == this._testDivisionID));
            
            await base._context.SaveChangesAsync();
         
            this.Result = "Deleted";
        }
    }
}
