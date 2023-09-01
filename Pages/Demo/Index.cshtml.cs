using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Demo
{
    public class IndexModel : PageModel
    {
        private readonly Sbt.Data.DemoContext _context;

        public string Organization { get; private set; } = string.Empty;

        public IList<Divisions> DivisionsList { get; set; } = default!;

        public IndexModel(Sbt.Data.DemoContext context)
        {
            this._context = context;
        }


        public async Task OnGetAsync(string organization)
        {
            this.Organization = organization ?? "[Missing Organization]";

            if (this._context.Divisions != null && organization != null)
            {
                DivisionsList = await this._context.Divisions
                    .Where(d => d.Organization == organization)
                    .OrderBy(d => d.ID)
                    .ToListAsync();
            }
        }
    }
}
