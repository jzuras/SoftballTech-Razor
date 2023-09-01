using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Standings
{
    public class IndexModel : PageModel
    {
        private readonly Sbt.Data.DemoContext _context;

        public string Organization { get; private set; } = string.Empty;

        public IList<Sbt.Standings> Standings { get; set; } = default!;

        public IList<Sbt.Schedules> Schedules { get; set; } = default!;
        
        public Sbt.Divisions Divisions { get; set; } = default!;

        public bool ShowOvertimeLosses { get; set; } = false;

        public IndexModel(Sbt.Data.DemoContext context)
        {
            this._context = context;
        }

        public async Task<IActionResult> OnGetAsync(string organization, string id)
        {
            this.Organization = organization ?? "[Missing Organization]";

            if (this._context.Divisions != null && organization != null && id != null)
            {
                var divisions = await this._context.Divisions.FirstOrDefaultAsync(
                    d => d.Organization == organization && d.ID == id);
                if (divisions == null)
                {
                    return NotFound();
                }
                
                this.Divisions = divisions;

                this.Standings = await this._context.Standings
                    .Where(s => s.Organization == organization && s.Division == id)
                    .OrderBy(s => s.GB)
                    .ToListAsync();

                this.Schedules = await this._context.Schedules
                    .Where(s => s.Organization == organization && s.Division == id)
                    .OrderBy(s => s.GameID)
                    .ToListAsync();

                this.DetermineOvertimeLossVisibility();
            }
            return Page();
        }

        private void DetermineOvertimeLossVisibility()
        {
            // in a production system this would be handled more generically,
            // but for now we are just checking if Org contains "Hockey"
            this.ShowOvertimeLosses = this.Organization.ToLower().Contains("hockey");
        }
    }
}
