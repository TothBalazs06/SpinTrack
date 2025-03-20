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

        public RecordListPage()
        {
            InitializeComponent();
            LoadRecords();
        }

        private void LoadRecords()
        {
            var records = new List<Record>();

            using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
            {
                var worksheet = package.Workbook.Worksheets["Records"];
                if (worksheet?.Dimension == null || worksheet.Dimension.End.Row < 2) // No data rows
                {
                    RecordListView.ItemsSource = records; // Set empty list if no data
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

            // Set the ListView's ItemsSource
            RecordListView.ItemsSource = records;
        }



        private void DeleteSelectedRecord(object sender, RoutedEventArgs e)
        {
            if (RecordListView.SelectedItem is Record selectedRecord)
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
                            worksheet.DeleteRow(row);
                            break;
                        }
                    }

                    package.Save();
                }

                // Reload the ListView
                LoadRecords();
                MessageBox.Show("Record deleted successfully!");
            }
            else
            {
                MessageBox.Show("Please select a record to delete.");
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
                MessageBox.Show("Please select a record to edit.");
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
                        MessageBox.Show("The selected file does not have a 'Records' worksheet.");
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
                            MessageBox.Show($"Column mismatch: Expected '{expectedColumns[col - 1]}', but got '{worksheet.Cells[1, col].Text}'.");
                            return;
                        }
                    }

                    // Merge records
                    using (var currentPackage = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                    {
                        var currentWorksheet = currentPackage.Workbook.Worksheets["Records"];
                        if (currentWorksheet == null)
                        {
                            MessageBox.Show("Database file is corrupted or missing 'Records' worksheet.");
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
                    }

                    MessageBox.Show("Records imported successfully!");
                }
            }
        }


        public void ExportRecords(object sender, RoutedEventArgs e)
        {
            // Open save file dialog
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                Title = "Export Records",
                FileName = $"SpinTrack_Records_{DateTime.Now:yyyy-MM-dd}.xlsx"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                string exportFilePath = saveFileDialog.FileName;

                using (var package = new ExcelPackage(new System.IO.FileInfo(_filePath)))
                using (var exportPackage = new ExcelPackage(new System.IO.FileInfo(exportFilePath)))
                {
                    var worksheet = package.Workbook.Worksheets["Records"];
                    if (worksheet == null)
                    {
                        MessageBox.Show("No data to export.");
                        return;
                    }

                    var exportWorksheet = exportPackage.Workbook.Worksheets.Add("Records");
                    worksheet.Cells[1, 1, worksheet.Dimension.End.Row, worksheet.Dimension.End.Column]
                        .Copy(exportWorksheet.Cells[1, 1]);

                    exportPackage.Save();
                }

                MessageBox.Show("Records exported successfully!");
            }
        }


    }
}