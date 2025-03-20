using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using SpinTrack;

namespace SpinTrackTests
{
    [TestClass]
    public class ExcelManagerTests
    {
        private string _testFilePath;
        private ExcelManager _excelManager;

        [TestInitialize]
        public void Setup()
        {
            // Create a temporary file for testing
            _testFilePath = Path.Combine(Path.GetTempPath(), "Test_SpinTrackRecords.xlsx");
            _excelManager = new ExcelManager(_testFilePath); // Custom path for testing
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete the test file after each test
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);
        }

        [TestMethod]
        public void AddRecord_ShouldAddRecordToExcelFile()
        {
            // Arrange
            string artist = "Test Artist";
            string albumTitle = "Test Album";
            int releaseYear = 2023;
            string category = "Rock";
            string length = "LP";
            int quantity = 1;
            bool outerCover = true;
            bool innerCover = false;
            string vinylQuality = "NM+";
            string sleeveQuality = "VG";

            // Act
            _excelManager.AddRecord(artist, albumTitle, releaseYear, category, length, quantity, outerCover, innerCover, vinylQuality, sleeveQuality);

            // Assert
            using (var package = new ExcelPackage(new FileInfo(_testFilePath)))
            {
                var worksheet = package.Workbook.Worksheets["Records"];
                Assert.IsNotNull(worksheet, "Worksheet should exist.");
                Assert.AreEqual(2, worksheet.Dimension.End.Row, "There should be 1 record + headers.");
                Assert.AreEqual(artist, worksheet.Cells[2, 1].Text, "Artist should match.");
                Assert.AreEqual(albumTitle, worksheet.Cells[2, 2].Text, "Album Title should match.");
                Assert.AreEqual("2023", worksheet.Cells[2, 3].Text, "Release Year should match.");
                Assert.AreEqual(category, worksheet.Cells[2, 4].Text, "Category should match.");
                Assert.AreEqual(length, worksheet.Cells[2, 5].Text, "Length should match.");
                Assert.AreEqual("1", worksheet.Cells[2, 6].Text, "Quantity should match.");
                Assert.AreEqual("Yes", worksheet.Cells[2, 7].Text, "Outer Cover should match.");
                Assert.AreEqual("No", worksheet.Cells[2, 8].Text, "Inner Cover should match.");
                Assert.AreEqual(vinylQuality, worksheet.Cells[2, 9].Text, "Vinyl Quality should match.");
                Assert.AreEqual(sleeveQuality, worksheet.Cells[2, 10].Text, "Sleeve Quality should match.");
            }
        }
    }
}