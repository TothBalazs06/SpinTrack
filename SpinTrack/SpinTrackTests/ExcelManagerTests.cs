using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OfficeOpenXml;
using SpinTrack;  // Ensure your project’s namespace is referenced

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
            // Set the license context for EPPlus before any operations.
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Create a temporary file path for testing; delete if it exists.
            _testFilePath = Path.Combine(Path.GetTempPath(), "Test_SpinTrackRecords.xlsx");
            if (File.Exists(_testFilePath))
                File.Delete(_testFilePath);

            // Initialize ExcelManager with the test file path.
            _excelManager = new ExcelManager(_testFilePath);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // Delete the test file after each test.
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

            // Assert: Open the created Excel file and check that the record is in row 2.
            using (var package = new ExcelPackage(new FileInfo(_testFilePath)))
            {
                var worksheet = package.Workbook.Worksheets["Records"];
                Assert.IsNotNull(worksheet, "Worksheet should exist.");
                // Row 1 is header; row 2 is the record.
                Assert.AreEqual(2, worksheet.Dimension.End.Row, "There should be one data row plus the header row.");
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

        [TestMethod]
        public void GetArtistSuggestions_ShouldReturnCorrectSuggestions()
        {
            // Arrange: Add two records
            _excelManager.AddRecord("Alpha Artist", "Album1", 2021, "Jazz", "LP", 1, true, false, "NM+", "VG");
            _excelManager.AddRecord("Beta Artist", "Album2", 2022, "Rock", "EP", 2, false, true, "VG", "G");

            // Act: Get suggestions for "Al" - should return "Alpha Artist"
            List<string> suggestions = _excelManager.GetArtistSuggestions("Al");

            // Assert
            Assert.IsNotNull(suggestions, "Suggestions list should not be null.");
            Assert.AreEqual(1, suggestions.Count, "There should be one suggestion.");
            Assert.AreEqual("Alpha Artist", suggestions[0], "Suggestion should be 'Alpha Artist'.");
        }
    }
}
