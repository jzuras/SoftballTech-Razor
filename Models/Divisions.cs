using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

// Razor does not play well with nullable reference types,
// but this line will still allow for null derefernce warnings
#nullable disable annotations

namespace Sbt
{
    [PrimaryKey(nameof(Organization), nameof(ID))]
    public class Divisions
    {
        [RegularExpression(@"^[a-zA-Z0-9]+[a-zA-Z0-9-_]*$")] 
        public string Organization { get; set; } = string.Empty;

        [Required]
        [Comment("short string version used in URLs")]
        [DisabledOnAzure(ErrorMessage = "Division mods are disbaled on Azure.")]
        [RegularExpression(@"^[a-zA-Z0-9]+[a-zA-Z0-9-_]*$")] 
        public string ID { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+[ a-zA-Z0-9-_]*$")] 
        public string League { get; set; }

        [Required]
        [RegularExpression(@"^[a-zA-Z0-9]+[ a-zA-Z0-9-_]*$")] 
        public string Division { get; set; }

        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy h:mm tt}", ApplyFormatInEditMode = false)]
        public DateTime Updated { get; set; }

        [Comment("Locked means that scores can no longer be reported")]
        public bool Locked { get; set; }
    }


    public class DisabledOnAzureAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // Originally, this was going to validate the uniqueness of the division ID within an Organization,
            // but since this is called before the Organization can be set in Create's OnPost(),
            // this doesn't work. But I am keeping this code here and commented out for future reference purposes.
                /*
                var dbContext = (Sbt.Data.DemoContext)validationContext.GetService(typeof(Sbt.Data.DemoContext));
                var id = (string)value;
                var organization = ((Divisions)validationContext.ObjectInstance).Organization;

                if (dbContext!.Divisions.Any(e => e.Organization == organization && e.ID != id))
                {
                    return new ValidationResult(ErrorMessage);
                }
                */
                return ValidationResult.Success;
        }
    }
}