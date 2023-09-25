using Microsoft.AspNetCore.Mvc;

namespace Sbt.Pages.Admin.Divisions
{
    public class CreateModel : Sbt.Pages.Admin.AdminPageModel
    {
        public CreateModel(Sbt.Data.DemoContext context) : base(context)
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

            if (organization == null)
            {
                return Page();
            }

            base.Organization = organization;

            if (!ModelState.IsValid || base._context.Divisions == null || base.Divisions == null)
            {
                return Page();
            }

            // need to make sure new division ID is unique
            if (this.DivisionIDExists(organization, base.Divisions.ID))
            {
                ModelState.AddModelError(string.Empty, "This Division ID already exists.");
                return Page();
            }

            // handle overposting
            var emptyDivision = new Sbt.Divisions();
            emptyDivision.Organization = organization;
            if( await TryUpdateModelAsync<Sbt.Divisions>(
                emptyDivision, "divisions",
                d => d.ID, d => d.League, d => d.Division))
            {
                emptyDivision.Updated = base.GetEasternTime();
                base._context.Divisions.Add(emptyDivision);
                await base._context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { organization = organization });
        }

        private bool DivisionIDExists(string organization, string id)
        {
            return base._context.Divisions.Any(e => e.Organization == organization && e.ID == id);
        }
    }
}
