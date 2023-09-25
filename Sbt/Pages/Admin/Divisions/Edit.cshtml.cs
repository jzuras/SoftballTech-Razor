using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Admin.Divisions
{
    public class EditModel : Sbt.Pages.Admin.AdminPageModel
    {
        public EditModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        // Note - using base class version of OnGetAsync()

        public async Task<IActionResult> OnPostAsync(string organization, string id)
        {
            // submit button should be disbled if true, but protect against other entries
            if (base.DisableSubmitButton == true)
            {
                return Page();
            }

            if (organization == null)
            {
                return Page();
            }

            base.Organization = organization;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // handle overposting
            var divisionToUpdate = await this._context.Divisions.FirstOrDefaultAsync(
                d => d.Organization == organization && d.ID == id);

            if (divisionToUpdate == null)
            {
                return Page();
            }

            if (await TryUpdateModelAsync<Sbt.Divisions>(
                divisionToUpdate, "divisions",
                d => d.League, d => d.Division, d => d.Locked))
            {
                divisionToUpdate.Organization = organization;
                divisionToUpdate.ID = id;
                divisionToUpdate.Updated = base.GetEasternTime();
                await base._context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { organization = organization });
        }
    }
}
