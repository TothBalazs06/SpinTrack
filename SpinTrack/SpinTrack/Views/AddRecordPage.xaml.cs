using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpinTrack.Views
{
    /// <summary>
    /// Interaction logic for AddRecordPage.xaml
    /// </summary>
    public partial class AddRecordPage : Page
    {
        public AddRecordPage()
        {
            InitializeComponent();
        }

        private void SaveRecord(object sender, RoutedEventArgs e)
        {
            string artist = ArtistTextBox.Text;
            string albumTitle = AlbumTitleTextBox.Text;
            int releaseYear;
            if (!int.TryParse(ReleaseYearTextBox.Text, out releaseYear))
            {
                new CustomMessageBox("Please enter a valid release year.", "OK").ShowDialog();
                return;
            }
            string category = ((ComboBoxItem)CategoryComboBox.SelectedItem)?.Content.ToString()!;
            string length = LengthStackPanel.Children.OfType<RadioButton>().FirstOrDefault(r => r.IsChecked == true)?.Content.ToString()!;
            int quantity;
            if (!int.TryParse(QuantityTextBox.Text, out quantity) || quantity <= 0)
            {
                new CustomMessageBox("Please enter a valid quantity.", "OK").ShowDialog();
                return;
            }
            bool outerCover = OuterCoverCheckBox.IsChecked == true;
            bool innerCover = InnerCoverCheckBox.IsChecked == true;
            string vinylQuality = ((ComboBoxItem)VinylQualityComboBox.SelectedItem)?.Content.ToString()!;
            string sleeveQuality = ((ComboBoxItem)SleeveQualityComboBox.SelectedItem)?.Content.ToString()!;

            // Validate required fields
            if (string.IsNullOrWhiteSpace(artist) || string.IsNullOrWhiteSpace(albumTitle) ||
                string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(length) ||
                string.IsNullOrWhiteSpace(vinylQuality) || string.IsNullOrWhiteSpace(sleeveQuality))
            {
                new CustomMessageBox("Please fill in all required fields.", "OK").ShowDialog();
                return;
            }

            // Save to Excel
            try
            {
                var excelManager = new ExcelManager();
                excelManager.AddRecord(artist, albumTitle, releaseYear, category, length, quantity,
                    outerCover, innerCover, vinylQuality, sleeveQuality);
                new CustomMessageBox("Record saved successfully!", "OK").ShowDialog();
                ClearFields();
            }
            catch (Exception ex)
            {
                new CustomMessageBox($"An error occurred while saving the record: {ex.Message}", "OK").ShowDialog();
            }
        }


        private void ClearFields()
        {
            ArtistTextBox.Text = "";
            AlbumTitleTextBox.Text = "";
            ReleaseYearTextBox.Text = "";
            CategoryComboBox.SelectedIndex = -1;
            foreach (var radio in LengthStackPanel.Children.OfType<RadioButton>())
                radio.IsChecked = false;
            QuantityTextBox.Text = "1";
            OuterCoverCheckBox.IsChecked = false;
            InnerCoverCheckBox.IsChecked = false;
        }

        private void ArtistTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string input = ArtistTextBox.Text;
            if (string.IsNullOrWhiteSpace(input))
            {
                SuggestionsPopup.IsOpen = false; // Hide suggestions if no input
                return;
            }

            // Fetch artist suggestions
            var excelManager = new ExcelManager();
            var suggestions = excelManager.GetArtistSuggestions(input);

            if (suggestions.Any())
            {
                SuggestionsListBox.ItemsSource = suggestions; // Set suggestions
                SuggestionsPopup.IsOpen = true;              // Show suggestions
            }
            else
            {
                SuggestionsPopup.IsOpen = false; // Hide suggestions if none match
            }
        }

        private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem != null)
            {
                ArtistTextBox.Text = SuggestionsListBox.SelectedItem.ToString();
                SuggestionsPopup.IsOpen = false; // Close the popup
            }
        }


    }
}
