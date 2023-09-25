using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Sbt.Data;

namespace Sbt.Tests.xUnitTests
{
    internal class Utilities
    {
        private static readonly IConfiguration _configuration;
        private static readonly string _connectionString;

        static Utilities()
        {
            _configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json")
               .Build();

            _connectionString = _configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING") ??
                throw new InvalidOperationException("Connection string 'AZURE_SQL_CONNECTIONSTRING' not found.");
        }

        public static DemoContext CreateContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<DemoContext>()
                .UseSqlServer(_connectionString, sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(5), 
                        errorNumbersToAdd: null);
                });

            return new DemoContext(optionsBuilder.Options);
        }

        #region FilePathHelpers
        public static MemoryStream GetMemoryStreamForDataFile(string dataFilename)
        {
            string filePath = Utilities.GetFullPathToFile(dataFilename);
            var fileContent = File.ReadAllBytes(filePath);
            return new MemoryStream(fileContent);
        }

        internal static string GetFullPathToFile(string pathRelativeUnitTestingFile)
        {
            string folderProjectLevel = Utilities.GetPathToCurrentUnitTestProject();
            string finalPath = System.IO.Path.Combine(folderProjectLevel, pathRelativeUnitTestingFile);
            return finalPath;
        }

        private static string GetPathToCurrentUnitTestProject()
        {
            string pathAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string? folderAssembly = System.IO.Path.GetDirectoryName(pathAssembly);

            if (folderAssembly != null)
            {
                if (!folderAssembly.EndsWith("\\"))
                {
                    folderAssembly += "\\";
                }

                string folderProjectLevel = System.IO.Path.GetFullPath(
                    System.IO.Path.Combine(folderAssembly, "..\\..\\..\\"));

                return folderProjectLevel;
            }
            else
            {
                // not really sure when if ever folderAssembly would be null
                return "";
            }
        }
        #endregion
    }
}
