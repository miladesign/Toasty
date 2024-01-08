using System.Diagnostics;
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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NormalToast_Click(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            int randomNumber = random.Next(1, 101);
            Toast.MakeText(this, $"این یک پیام تست است - {randomNumber}", Toast.LENGTH_SHORT).Show();
        }

        private void CustomToast_Click(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            int randomNumber = random.Next(1, 101);

            string message = "This is a test message.\n" +
                            "این یک پیام تست است.\n" +
                            "C'est un message de test.\n" +
                            "यह एक परीक्षण संदेश है.\n" +
                            "这是一条测试消息。\n" +
                            "Curabitur eget justo ut ex dapibus convallis.\n" +
                            randomNumber;

            var toast = new Toast(this)
            {
                BackgroundColor = GetRandomDarkBrush(),
                TextColor = Brushes.Yellow,
                Message = message,
                Duration = Toast.LENGTH_LONG,
                FontFamily = new FontFamily("IRANYekanFN"),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                FontStyle = FontStyles.Italic,
                Direction = FlowDirection.RightToLeft,
                Tag = randomNumber
            };
            toast.OnShown += () => Debug.WriteLine($"Toast is shown! TAG: {toast.Tag}");
            toast.OnHidden += () => Debug.WriteLine($"Toast is hidden! TAG: {toast.Tag}");
            toast.Show();
        }

        private static SolidColorBrush GetRandomDarkBrush()
        {
            var random = new Random();

            byte red = (byte)random.Next(0, 128);
            byte green = (byte)random.Next(0, 128);
            byte blue = (byte)random.Next(0, 128);

            return new SolidColorBrush(Color.FromRgb(red, green, blue));
        }
    }
}