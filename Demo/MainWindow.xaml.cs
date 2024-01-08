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
using Toasty.WPF;

namespace Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Generate a random integer between 1 and 100 (you can adjust the range)
            Random random = new Random();
            int randomNumber = random.Next(1, 101);

            // Create the message with the random number
            string message = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. " +
                          "Pellentesque habitant morbi tristique senectus et netus et malesuada fames ac turpis egestas. \n" +
                          $"این یک پیام تست است - {randomNumber}. \n" +
                          "Curabitur eget justo ut ex dapibus convallis.";

            // Show the toast with the modified message
            //Toast.MakeText(this, message, Toast.LENGTH_LONG).Show();

            var toast = new Toast(this)
            {
                BackgroundColor = Brushes.Red,
                TextColor = Brushes.Yellow,
                Message = message,
                Duration = Toast.LENGTH_LONG
            };
            toast.Show();
        }
    }
}