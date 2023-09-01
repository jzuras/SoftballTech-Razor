using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Sbt
{
    [PrimaryKey(nameof(Organization), nameof(GameID))]
    public class Schedules
    {
        public string Organization { get; set; } = string.Empty;

        public int GameID { get; set; }

        public string Division { get; set; } = string.Empty;

        public string Home { get; set; } = string.Empty;

        public string Visitor { get; set; } = string.Empty;

        [DataType(DataType.Date)] // only display the date here, not the time
        public DateTime? Day { get; set; }

        public DateTime? Time { get; set; }

        public string Field { get; set; } = string.Empty;

        public short HomeID { get; set; }

        public short VisitorID { get; set; }

        public short? HomeScore { get; set; }

        public short? VisitorScore { get; set; }

        public bool HomeForfeit { get; set; }

        public bool VisitorForfeit { get; set; }

        public bool OvertimeGame { get; set; }

        public DateTime? MakeupDay { get; set; }

        public DateTime? MakeupTime { get; set; }

        public string? MakeupField { get; set; }
    }
}
