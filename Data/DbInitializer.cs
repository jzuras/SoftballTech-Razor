using System.Diagnostics;

namespace Sbt.Data
{
    public static class DbInitializer
    {
        public static void Initialize(DemoContext context)
        {
            // Look for any students.
            if (context.Divisions.Any())
            {
                return;   // DB has been seeded
            }

            var divisions = new Divisions[]
            {
                // todo - remove the constructor with this signature when done init-ing DB
                new Divisions ( "Demo Softball", "FC01", "Fall Coed", "1", DateTime.Now, false),
                new Divisions ( "Demo Softball", "FC02", "Fall Coed", "2", DateTime.Now, false),
                new Divisions ( "Demo Softball", "FM01", "Fall Men", "1", DateTime.Now, false),
                new Divisions ( "Demo Softball", "FM02", "Fall Men", "2", DateTime.Now, false),
                new Divisions ( "Demo Hockey", "FM01", "Fall Men", "1", DateTime.Now, false),
                new Divisions ( "Demo Hockey", "FM02", "Fall Men", "2", DateTime.Now, false)
            };

            context.Divisions.AddRange(divisions);

            //var standings = new Standings[]
            //{
            //    new Standings {Wins=0,Losses=0,Ties=0,OvertimeLosses=0,Percentage=0,GB=0,RunsAgainst=0,RunsScored=0,
            //    Forfeits=0,ForfeitsCharged=0,Division="FC01",Name="team 1" }
            //};

            //context.Standings.AddRange(standings);
            context.SaveChanges();

            // could also do standings and schedules here if so desired
        }
    }
}