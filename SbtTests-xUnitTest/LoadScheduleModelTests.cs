using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Sbt.Pages.Admin.LoadSchedule;
using Xunit;

namespace Sbt.Tests.xUnitTests
{
    public class LoadScheduleModelTests : IDisposable
    {
        #region LoadScheduleFileInputs
        private string _filenameForValidData = "Data\\TestData-PageModel.csv";
        private string _organization = "LoadScheduleModelxUnitTests";
        private string _divisionID = "Test02";
        #endregion

        [Fact]
        public async Task OnPostAsync_ReturnsAPageResult_WhenLoadIsSuccessful()
        {
            // test a valid data file and then check the DB to make sure it loaded properly

            var memoryStream = Utilities.GetMemoryStreamForDataFile(_filenameForValidData);
            var scheduleFile = new FormFile(memoryStream, 0, memoryStream.Length, "", "");

            using (var db = Utilities.CreateContext())
            {
                int expectedGameCount = 72;
                int expectedStandingsCount = 9;

                // Arrange
                var httpContext = new DefaultHttpContext();
                var modelState = new ModelStateDictionary();
                var actionContext = new ActionContext(httpContext, new RouteData(), new PageActionDescriptor(), modelState);
                var modelMetadataProvider = new EmptyModelMetadataProvider();
                var viewData = new ViewDataDictionary(modelMetadataProvider, modelState);
                var pageContext = new PageContext(actionContext)
                {
                    ViewData = viewData
                };
                var pageModel = new LoadScheduleModel(db)
                {
                    PageContext = pageContext,
                    Url = new UrlHelper(actionContext),
                    ScheduleFile = scheduleFile,
                    DivisionID = this._divisionID
                };

                // Act
                var result = await pageModel.OnPostAsync(this._organization);
                var actualGameCount = db.Schedules.Count(s => s.Organization == this._organization &&
                    s.Division == this._divisionID);
                var actualStandingsCount = db.Standings.Count(s => s.Organization == this._organization &&
                    s.Division == this._divisionID);

                // Assert
                Assert.IsType<PageResult>(result);
                Assert.Equal(expectedGameCount, actualGameCount);
                Assert.Equal(expectedStandingsCount, actualStandingsCount);
            }
        }

        public void Dispose()
        {
            // Clean up resources
            using (var db = Utilities.CreateContext())
            {
                // delete everything from tests in this class:
                {
                    db.Divisions.RemoveRange(db.Divisions.Where(d => d.Organization == this._organization &&
                        d.ID == this._divisionID));
                    db.Standings.RemoveRange(db.Standings.Where(s => s.Organization == this._organization &&
                        s.Division == this._divisionID));
                    db.Schedules.RemoveRange(db.Schedules.Where(s => s.Organization == this._organization &&
                        s.Division == this._divisionID));
                    db.SaveChanges();
                }
            }
        }
    }
}