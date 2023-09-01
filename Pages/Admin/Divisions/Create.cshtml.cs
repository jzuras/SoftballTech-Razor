using Microsoft.AspNetCore.Mvc;

namespace Sbt.Pages.Admin.Divisions
{
    public class CreateModel : Sbt.Pages.Admin.AdminPageModel
    {
        public CreateModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        // Note - using base class version of OnGetAsync()

        // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD
        public async Task<IActionResult> OnPostAsync(string organization)
        {
            if (organization == null)
            {
                return Page();
            }

            base.Divisions.Organization = base.Organization = organization;
            
            if (!ModelState.IsValid || base._context.Divisions == null || base.Divisions == null)
            {
                return Page();
            }

            // need to make sure new division ID is unique
            if (this.DivisionIDExists(base.Divisions.Organization, base.Divisions.ID))
            {
                ModelState.AddModelError(string.Empty, "This Division ID already exists.");
                return Page();
            }

            base.Divisions.Updated = base.GetEasternTime();
            base._context.Divisions.Add(base.Divisions);
            await base._context.SaveChangesAsync();

            return RedirectToPage("./Index", new { organization = organization });
        }

        private bool DivisionIDExists(string organization, string id)
        {
            return base._context.Divisions.Any(e => e.Organization == organization && e.ID == id);
        }
    }
}
