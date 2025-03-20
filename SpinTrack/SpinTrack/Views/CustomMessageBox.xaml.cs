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
    /// Interaction logic for CustomMessageBox.xaml
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public bool ConfirmationResult { get; private set; } = false;

        public CustomMessageBox(string message, string primaryButtonText = "OK", string secondaryButtonText = null!)
        {
            InitializeComponent();
            MessageTextBlock.Text = message;
            PrimaryButton.Content = primaryButtonText;

            if (!string.IsNullOrEmpty(secondaryButtonText))
            {
                SecondaryButton.Content = secondaryButtonText;
                SecondaryButton.Visibility = Visibility.Visible;
            }
        }

        private void PrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationResult = true;
            this.Close();
        }

        private void SecondaryButton_Click(object sender, RoutedEventArgs e)
        {
            ConfirmationResult = false;
            this.Close();
        }
    }


}
