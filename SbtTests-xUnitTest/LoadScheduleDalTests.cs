using Microsoft.AspNetCore.Http;
using System.Globalization;
using Xunit;

namespace Sbt.Tests.xUnitTests
{
    public class LoadScheduleDalTests : IDisposable
    {
        #region LoadScheduleFileInputs
        private string _filenameForValidData = "Data\\TestData.csv";
        private string _filenameForInvalidData = "Data\\TestData-Bad.csv";
        private string _filenameForEmptyData = "Data\\TestData-Empty.csv";
        private string _organization = "LoadScheduleDalxUnitTests";
        private string _divisionID = "Test01";
        #endregion

        [Fact]
        public async Task LoadScheduleTest_Valid_FileAsync()
        {
            var memoryStream = Utilities.GetMemoryStreamForDataFile(_filenameForValidData);
            var scheduleFile = new FormFile(memoryStream, 0, memoryStream.Length, "", "");

            using (var db = Utilities.CreateContext())
            {
                // Arrange
                var expectedResultErrorMessage = string.Empty;
                var expectedResultFirstGameDate = 
                    DateTime.ParseExact("9/3/2023", "M/d/yyyy", CultureInfo.InvariantCulture);
                var expectedResultLastGameDate =
                    DateTime.ParseExact("10/24/2023", "MM/dd/yyyy", CultureInfo.InvariantCulture);

                // Act
                var result = await db.LoadScheduleFileAsync(scheduleFile, 
                    this._organization, this._divisionID, false, false);

                // Assert
                Assert.Equal(expectedResultFirstGameDate, result.FirstGameDate );
                Assert.Equal(expectedResultLastGameDate, result.LastGameDate );
                Assert.Equal(expectedResultErrorMessage, result.ErrorMessage );
            }
        }

        [Fact]
        public async Task LoadScheduleTest_Invalid_FileAsync()
        {
            var memoryStream = Utilities.GetMemoryStreamForDataFile(_filenameForInvalidData);
            var scheduleFile = new FormFile(memoryStream, 0, memoryStream.Length, "", "");

            using (var db = Utilities.CreateContext())
            {
                // Arrange

                // Act
                var result = await db.LoadScheduleFileAsync(scheduleFile, this._organization, this._divisionID, false, false);

                // Assert
                Assert.False(result.Success);
            }
        }

        [Fact]
        public async Task LoadScheduleTest_Empty_FileAsync()
        {
            var memoryStream = Utilities.GetMemoryStreamForDataFile(_filenameForEmptyData);
            var scheduleFile = new FormFile(memoryStream, 0, memoryStream.Length, "", "");

            using (var db = Utilities.CreateContext())
            {
                // Arrange

                // Act
                var result = await db.LoadScheduleFileAsync(scheduleFile, this._organization, this._divisionID, false, false);

                // Assert
                Assert.False(result.Success);
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