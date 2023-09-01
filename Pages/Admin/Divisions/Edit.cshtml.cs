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

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(string organization)
        {
            if (organization == null)
            {
                return Page();
            }

            base.Organization = organization;

            if (!ModelState.IsValid)
            {
                return Page();
            }

            base.Divisions.Updated = base.GetEasternTime();
            base._context.Attach(Divisions).State = EntityState.Modified;

            try
            {
                await base._context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DivisionsExists(base.Divisions.ID, base.Divisions.Organization))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index", new { organization = organization });
        }

        private bool DivisionsExists(string id, string organization)
        {
            return base._context.Divisions.Any(e => e.Organization == organization && e.ID == id);
        }
    }
}
