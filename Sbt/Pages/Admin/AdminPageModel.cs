using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Admin
{
    // this is the base class for the Admin pages, created because
    // Razor scaffolding creates a lot of duplicate code.
    // Also included is a utility method for EST, and a boolean
    // to temporarily allow me to quickly enable/disable Admin functions
    // when posting to Azure.
    // (Soon to be replaced by a runtime-adjustable method.)
    public class AdminPageModel : PageModel
    {
        protected readonly Sbt.Data.DemoContext _context;

        // for now, this is a quick way to disable Admin Functions on Azure
        // I hope to turn this into a Feature (on/off from Azure Portal)
        public bool DisableSubmitButton = true;

        public string Organization = string.Empty;

        [BindProperty]
        public Sbt.Divisions Divisions { get; set; } = default!;

        public AdminPageModel(Sbt.Data.DemoContext context)
        {
            this._context = context;
        }

        virtual public async Task<IActionResult> OnGetAsync(string organization, string id)
        {
            if (organization == null || this._context.Divisions == null)
            {
                return NotFound();
            }

            this.Organization = organization;

            // some pages may not have a need for an ID so this is not an error
            if( id == null || id == string.Empty)
            {
                return Page();
            }

            var divisions = await this._context.Divisions.FirstOrDefaultAsync(
                m => m.Organization == organization && m.ID == id);

            if (divisions == null)
            {
                return NotFound();
            }

            this.Divisions = divisions;
            return Page();
        }


        protected DateTime GetEasternTime()
        {
            DateTime utcTime = DateTime.UtcNow;

            TimeZoneInfo easternTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            return TimeZoneInfo.ConvertTimeFromUtc(utcTime, easternTimeZone);
        }
    }
}
