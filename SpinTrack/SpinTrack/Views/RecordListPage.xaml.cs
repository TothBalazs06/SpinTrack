namespace SpinTrack.Views
{
    using System.Collections.Generic;
    using System.Windows;
    using System.Windows.Controls;
    using Microsoft.Win32;
    using OfficeOpenXml;

    public partial class RecordListPage : Page
    {
        private readonly string _filePath = "SpinTrackRecords.xlsx";

        private List<Record> _allRecords = new List<Record>();

        public RecordListPage()
        {
            InitializeComponent();
            this.Loaded += RecordListPage_Loaded;
            FilterComboBox.SelectedIndex = 0; // Select the first item ("All Genres")
            FilterLengthComboBox.SelectedIndex = 0; // Select the first item ("All Genres")
        }

        private void RecordListPage_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRecords();
        }


        private void LoadRecords()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Set the license context

            var records = new List<Record>(); // Temporary list for current records

            using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Records"];
                if (worksheet?.Dimension == null || worksheet.Dimension.End.Row < 2)
                {
                    _allRecords = new List<Record>(); // Update _allRecords even if empty
                    RecordListView.ItemsSource = _allRecords;
                    return;
                }

                int rowCount = worksheet.Dimension.End.Row;
                for (int row = 2; row <= rowCount; row++) // Skip header row
                {
                    var record = new Record
                    {
                        Artist = worksheet.Cells[row, 1].Text,
                        AlbumTitle = worksheet.Cells[row, 2].Text,
                        ReleaseYear = worksheet.Cells[row, 3].Text,
                        Category = worksheet.Cells[row, 4].Text,
                        Length = worksheet.Cells[row, 5].Text,
                        Quantity = worksheet.Cells[row, 6].Text,
                        HasOuterCover = worksheet.Cells[row, 7].Text == "Yes",
                        HasInnerCover = worksheet.Cells[row, 8].Text == "Yes",
                        VinylQuality = worksheet.Cells[row, 9].Text,
                        SleeveQuality = worksheet.Cells[row, 10].Text
                    };
                    records.Add(record);
                }
            }

            _allRecords = records;
            RecordListView.ItemsSource = records;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.ToLower();

            var filteredRecords = _allRecords.Where(record =>
                record.Artist.ToLower().Contains(searchText) ||
                record.AlbumTitle.ToLower().Contains(searchText) ||
                record.ReleaseYear.ToLower().Contains(searchText)).ToList();


            RecordListView.ItemsSource = filteredRecords;
        }

        private void FilterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters(); // Combine filters
        }

        private void FilterLengthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters(); // Combine filters
        }

        private void ApplyFilters()
        {
            // Start with the full list of records
            var filteredRecords = _allRecords;

            // Filter by genre
            string genreFilter = (FilterComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()!;
            if (!string.IsNullOrEmpty(genreFilter) && genreFilter != "All Genres")
            {
                filteredRecords = filteredRecords.Where(record => record.Category == genreFilter).ToList();
            }

            // Filter by length
            string lengthFilter = (FilterLengthComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()!;
            if (!string.IsNullOrEmpty(lengthFilter) && lengthFilter != "All Lengths")
            {
                filteredRecords = filteredRecords.Where(record => record.Length == lengthFilter).ToList();
            }

            // Update the ListView with the filtered data
            RecordListView.ItemsSource = filteredRecords;
        }



        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            // Reset ComboBoxes
            FilterComboBox.SelectedIndex = 0; // Select "All Genres"
            FilterLengthComboBox.SelectedIndex = 0; // Select "All Lengths"

            // Reset the ListView
            RecordListView.ItemsSource = _allRecords; // Show all records
        }

        private void DeleteSelectedRecord(object sender, RoutedEventArgs e)
        {
            if (RecordListView.SelectedItem is Record selectedRecord)
            {
                // Show the confirmation dialog
                CustomMessageBox confirmDialog = new CustomMessageBox("Are you sure you want to delete this record?", "Yes", "No");
                confirmDialog.ShowDialog();

                // Check the result of the dialog
                if (confirmDialog.ConfirmationResult) // Only proceed if the user clicked "Yes"
                {
                    using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                    {
                        var worksheet = package.Workbook.Worksheets["Records"];
                        int rowCount = worksheet.Dimension.End.Row;

                        for (int row = 2; row <= rowCount; row++)
                        {
                            if (worksheet.Cells[row, 1].Text == selectedRecord.Artist &&
                                worksheet.Cells[row, 2].Text == selectedRecord.AlbumTitle)
                            {
                                worksheet.DeleteRow(row); // Delete the row
                                break;
                            }
                        }

                        package.Save(); // Save changes
                    }

                    // Show success message
                    new CustomMessageBox("Record deleted successfully.", "OK").ShowDialog();

                    // Reload the ListView
                    LoadRecords();
                }
            }
            else
            {
                // Show an error message if no record is selected
                new CustomMessageBox("Please select a record to delete.", "OK").ShowDialog();
            }
        }





        private void EditSelectedRecord(object sender, RoutedEventArgs e)
        {
            if (RecordListView.SelectedItem is Record selectedRecord)
            {
                // Remove the record from Excel first
                using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Records"];
                    int rowCount = worksheet.Dimension.End.Row;

                    for (int row = 2; row <= rowCount; row++)
                    {
                        if (worksheet.Cells[row, 1].Text == selectedRecord.Artist &&
                            worksheet.Cells[row, 2].Text == selectedRecord.AlbumTitle)
                        {
                            worksheet.DeleteRow(row);
                            break;
                        }
                    }

                    package.Save();
                }

                // Navigate to Add Record Page for editing
                var addRecordPage = new AddRecordPage
                {
                    ArtistTextBox = { Text = selectedRecord.Artist },
                    AlbumTitleTextBox = { Text = selectedRecord.AlbumTitle },
                    ReleaseYearTextBox = { Text = selectedRecord.ReleaseYear },
                    QuantityTextBox = { Text = selectedRecord.Quantity },
                    OuterCoverCheckBox = { IsChecked = selectedRecord.HasOuterCover },
                    InnerCoverCheckBox = { IsChecked = selectedRecord.HasInnerCover }
                };

                // Set Category and Length
                foreach (ComboBoxItem item in addRecordPage.CategoryComboBox.Items)
                {
                    if (item.Content.ToString() == selectedRecord.Category)
                    {
                        addRecordPage.CategoryComboBox.SelectedItem = item;
                        break;
                    }
                }

                foreach (RadioButton radioButton in addRecordPage.LengthStackPanel.Children)
                {
                    if (radioButton.Content.ToString() == selectedRecord.Length)
                    {
                        radioButton.IsChecked = true;
                        break;
                    }
                }

                NavigationService.Navigate(addRecordPage);
            }
            else
            {
                new CustomMessageBox("Please select a record to edit.").ShowDialog();
            }

        }

        public void ImportRecords(object sender, RoutedEventArgs e)
        {
            // Open file dialog to select the Excel file
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Select an Excel File to Import"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string importFilePath = openFileDialog.FileName;

                using (var package = new ExcelPackage(new System.IO.FileInfo(importFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Records"];
                    if (worksheet == null)
                    {
                        new CustomMessageBox("The selected file does not have a 'Records' worksheet.", "OK").ShowDialog();
                        return;
                    }

                    // Validate the column layout
                    var expectedColumns = new[] { "Artist", "Album Title", "Release Year", "Category", "Length",
                                          "Quantity", "Outer Cover", "Inner Cover",
                                          "Vinyl Quality", "Sleeve Quality" };
                    for (int col = 1; col <= expectedColumns.Length; col++)
                    {
                        if (worksheet.Cells[1, col].Text != expectedColumns[col - 1])
                        {
                            new CustomMessageBox($"Column mismatch: Expected '{expectedColumns[col - 1]}', but got '{worksheet.Cells[1, col].Text}'.", "OK").ShowDialog();
                            return;
                        }
                    }

                    // Merge records
                    using (var currentPackage = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                    {
                        var currentWorksheet = currentPackage.Workbook.Worksheets["Records"];
                        if (currentWorksheet == null)
                        {
                            new CustomMessageBox("Database file is corrupted or missing 'Records' worksheet.", "OK").ShowDialog();
                            return;
                        }

                        int currentRowCount = currentWorksheet.Dimension?.End.Row ?? 1;
                        int importRowCount = worksheet.Dimension?.End.Row ?? 1;

                        for (int row = 2; row <= importRowCount; row++)
                        {
                            var artist = worksheet.Cells[row, 1].Text;
                            var albumTitle = worksheet.Cells[row, 2].Text;

                            // Check for duplicates
                            bool isDuplicate = false;
                            for (int currentRow = 2; currentRow <= currentRowCount; currentRow++)
                            {
                                if (currentWorksheet.Cells[currentRow, 1].Text == artist &&
                                    currentWorksheet.Cells[currentRow, 2].Text == albumTitle)
                                {
                                    isDuplicate = true;
                                    break;
                                }
                            }

                            if (!isDuplicate)
                            {
                                // Append the new record
                                currentRowCount++;
                                for (int col = 1; col <= expectedColumns.Length; col++)
                                {
                                    currentWorksheet.Cells[currentRowCount, col].Value = worksheet.Cells[row, col].Value;
                                }
                            }
                        }

                        currentPackage.Save();
                        LoadRecords();
                    }
                    new CustomMessageBox("Records imported successfully!", "OK").ShowDialog();
                }
            }
        }


        public void ExportRecords(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Export Records",
                FileName = $"SpinTrack_Records_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string exportFilePath = saveFileDialog.FileName;

                try
                {
                    using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                    using (var exportPackage = new ExcelPackage(new System.IO.FileInfo(exportFilePath)))
                    {
                        var worksheet = package.Workbook.Worksheets["Records"];
                        if (worksheet == null)
                        {
                            new CustomMessageBox("No data to export.", "OK").ShowDialog();
                            return;
                        }

                        var existingWorksheet = exportPackage.Workbook.Worksheets["Records"];
                        if (existingWorksheet != null)
                        {
                            exportPackage.Workbook.Worksheets.Delete(existingWorksheet);
                        }

                        var exportWorksheet = exportPackage.Workbook.Worksheets.Add("Records");

                        worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column]
                            .Copy(exportWorksheet.Cells[1, 1]);

                        exportPackage.Save();
                    }

                    new CustomMessageBox("Records exported successfully!", "OK").ShowDialog();
                }
                catch (Exception ex)
                {
                    new CustomMessageBox($"An error occurred during export: {ex.Message}", "OK").ShowDialog();
                }
            }
        }



    }
}