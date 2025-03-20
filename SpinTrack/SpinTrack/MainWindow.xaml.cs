using SpinTrack.Views;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpinTrack
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Navigate to Add Record Page by default
            MainFrame.Navigate(new AddRecordPage());
        }

        private void NavigateToAddRecord(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new AddRecordPage());
        }

        private void NavigateToRecordList(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new RecordListPage());
        }

        private void NavigateToCreditsPage(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new CreditsPage());
        }
    }

}