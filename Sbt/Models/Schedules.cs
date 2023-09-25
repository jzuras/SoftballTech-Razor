using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// Schedules table handles the day/time/field (original and possible make-ups) for a game.
// This is also where scores are kept. Home and Visitor are string names of the teams,
// while the corresponding ID's are the values in the Standings table.
// There is also a ViewModel class here, used for reporting scores.

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

        // [Required] and [RegularExpression] attributes are only needed on ViewModel
        public short? HomeScore { get; set; }

        // [Required] and [RegularExpression] attributes are only needed on ViewModel
        public short? VisitorScore { get; set; }

        public bool HomeForfeit { get; set; }

        public bool VisitorForfeit { get; set; }

        public bool OvertimeGame { get; set; }

        public DateTime? MakeupDay { get; set; }

        public DateTime? MakeupTime { get; set; }

        public string? MakeupField { get; set; }
    }

    public class SchedulesVM
    {
        public int GameID { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid integer value.")]
        public short? HomeScore { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Please enter a valid integer value.")]
        public short? VisitorScore { get; set; }

        public bool HomeForfeit { get; set; }

        public bool VisitorForfeit { get; set; }
    }
}
