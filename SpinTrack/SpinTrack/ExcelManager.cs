using System.IO;
using OfficeOpenXml; // Include EPPlus NuGet Package

public class ExcelManager
{
    private readonly string _filePath = "SpinTrackRecords.xlsx";

    public ExcelManager()
    {
        // Check if the file exists; if not, create it
        if (!File.Exists(_filePath))
        {
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Records");
                worksheet.Cells[1, 1].Value = "Artist";
                worksheet.Cells[1, 2].Value = "Album Title";
                worksheet.Cells[1, 3].Value = "Release Year";
                worksheet.Cells[1, 4].Value = "Category";
                worksheet.Cells[1, 5].Value = "Length";
                worksheet.Cells[1, 6].Value = "Quantity";
                worksheet.Cells[1, 7].Value = "Outer Cover";
                worksheet.Cells[1, 8].Value = "Inner Cover";

                package.SaveAs(new FileInfo(_filePath));
            }
        }
    }

    public string FilePath => _filePath;

    public void AddRecord(string artist, string albumTitle, int releaseYear, string category, string length, int quantity, bool outerCover, bool innerCover, string vinylQuality, string sleeveQuality)
    {
        using (var package = new ExcelPackage(new FileInfo(_filePath)))
        {
            var worksheet = package.Workbook.Worksheets["Records"];

            // Find the next empty row (if worksheet is empty, start at row 2)
            int row = worksheet.Dimension?.End.Row + 1 ?? 2;

            // Add data to each column
            worksheet.Cells[row, 1].Value = artist;
            worksheet.Cells[row, 2].Value = albumTitle;
            worksheet.Cells[row, 3].Value = releaseYear;
            worksheet.Cells[row, 4].Value = category;
            worksheet.Cells[row, 5].Value = length;
            worksheet.Cells[row, 6].Value = quantity;
            worksheet.Cells[row, 7].Value = outerCover ? "Yes" : "No";
            worksheet.Cells[row, 8].Value = innerCover ? "Yes" : "No";
            worksheet.Cells[row, 9].Value = vinylQuality;
            worksheet.Cells[row, 10].Value = sleeveQuality;

            // Save changes to the Excel file
            package.Save();
        }
    }


    public List<string> GetArtistSuggestions(string input)
    {
        var suggestions = new List<string>();

        using (var package = new ExcelPackage(new FileInfo(FilePath)))
        {
            var worksheet = package.Workbook.Worksheets["Records"];
            int rowCount = worksheet.Dimension.End.Row;

            for (int row = 2; row <= rowCount; row++) // Skip header row
            {
                var artist = worksheet.Cells[row, 1].Text;
                if (!string.IsNullOrEmpty(artist) && artist.StartsWith(input, StringComparison.OrdinalIgnoreCase))
                {
                    if (!suggestions.Contains(artist))
                        suggestions.Add(artist);
                }
            }
        }

        return suggestions;
    }

}
