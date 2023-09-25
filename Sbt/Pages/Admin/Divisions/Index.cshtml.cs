using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Admin.Divisions
{
    public class IndexModel : Sbt.Pages.Admin.AdminPageModel
    {
        public IndexModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        public IList<Sbt.Divisions> DivisionsList { get; set; } = default!;

        override public async Task<IActionResult> OnGetAsync(string organization, string id = "")
        {
            await base.OnGetAsync(organization, id);

            if (base._context.Divisions != null && organization != null)
            {
                DivisionsList = await base._context.Divisions
                    .Where(d => d.Organization == organization)
                    .OrderBy(d => d.ID)
                    .ToListAsync();
            }

            return Page();
        }
    }
}
