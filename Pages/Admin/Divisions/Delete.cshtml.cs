using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Admin.Divisions
{
    public class DeleteModel : Sbt.Pages.Admin.AdminPageModel
    {
        public DeleteModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        // Note - using base class version of OnGetAsync()

        public async Task<IActionResult> OnPostAsync()
        {
            if (base._context.Divisions == null)
            {
                return NotFound();
            }
            var divisions = await base._context.Divisions.FindAsync(base.Divisions.Organization, base.Divisions.ID);

            if (divisions != null)
            {
                base._context.Divisions.Remove(divisions);
                await base._context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { organization = base.Divisions.Organization });
        }
    }
}
