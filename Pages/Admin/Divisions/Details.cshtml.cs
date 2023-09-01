using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sbt.Pages.Admin.Divisions
{
    public class DetailsModel : Sbt.Pages.Admin.AdminPageModel
    {
        public DetailsModel(Sbt.Data.DemoContext context) : base(context)
        {
        }

        // Note - using base class version of OnGetAsync()

        // Note - no need for OnPostAsync() for details page
    }
}
