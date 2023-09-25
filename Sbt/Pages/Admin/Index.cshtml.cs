using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Sbt.Pages.Admin
{
    public class IndexModel : Sbt.Pages.Admin.AdminPageModel
    {
        public List<string> OrganizationList;

        public string Result { get; set; } = string.Empty;

        public IndexModel(IConfiguration configuration, Sbt.Data.DemoContext context) : base(context)
        {
            // this populates the HTML Select element list on the page.
            // The data should really come from the database, but I used this method
            // to learn about Configuration, as well as for simplicty.
                this.OrganizationList =
                    configuration.GetSection("Organizations")?.Get<List<string>>() ?? new List<string>();
        }

        override public async Task<IActionResult> OnGetAsync(string organization, string id = "")
        {
            return await Task.FromResult(Page());
        }
    }
}
