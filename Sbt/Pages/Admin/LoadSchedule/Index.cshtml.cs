using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Sbt.Pages.Admin.LoadSchedule
{
    public class LoadScheduleModel : Sbt.Pages.Admin.AdminPageModel
    {
        [BindProperty]
        [Required]
        [RegularExpression(@"^[a-zA-Z]+[a-zA-Z0-9-_]*$")]
        [StringLength(50, MinimumLength = 2)]
        public string DivisionID { get; set; } = string.Empty;

        [BindProperty]
        [Required]
        [DisplayName("Schedule File")]
        public IFormFile ScheduleFile { get; set; } = default!;

        [BindProperty]
        public bool UsesDoubleHeaders { get; set; } = false;

        [BindProperty]
        public bool ClearScheduleFirst { get; set; } = false;

        public string Result { get; set; } = string.Empty;

        public LoadScheduleModel(Sbt.Data.DemoContext context) : base(context)
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

            if (organization == null || this.ScheduleFile == null || this.ScheduleFile.Length == 0)
            {
                return Page();
            }

            base.Organization = organization;

            var loadResult = await base._context.LoadScheduleFileAsync(
                this.ScheduleFile, organization, this.DivisionID, this.UsesDoubleHeaders, this.ClearScheduleFirst);

            if (loadResult.Success)
            {
                this.Result = DateTime.Now.ToShortTimeString() + ": Success loading schedule from " +
                    ScheduleFile.FileName + ". <br>Games start on " +
                    loadResult.FirstGameDate.ToShortDateString() +
                    " and end on " +
                    loadResult.LastGameDate.ToShortDateString();
            }
            else
            {
                this.Result = loadResult.ErrorMessage;
            }
            return Page();
        }
    }
}
