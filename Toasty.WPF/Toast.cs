using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Toasty.WPF;

public class Toast
{
    public const int LENGTH_SHORT = 2000;
    public const int LENGTH_LONG = 3500;

    private static readonly Queue<Toast> toastQueue = new();
    private static bool isToastShowing = false;

    public string? Message;
    public int Duration;
    private Window? ParentWindow;
    public System.Windows.Media.FontFamily? FontFamily = null;
    public System.Windows.Media.Brush BackgroundColor = System.Windows.Media.Brushes.Black;
    public System.Windows.Media.Brush TextColor = System.Windows.Media.Brushes.White;
    public double FontSize = 15;

    public Toast(Window window)
    {
        ParentWindow = window;
    }

    public static Toast MakeText(Window window, string message, int duration)
    {
        return new Toast(window)
        {
            Message = message,
            Duration = duration
        };
    }

    public void Show()
    {
        if (Duration <= 0)
        {
            Duration = LENGTH_SHORT;
        }

        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        {
            toastQueue.Enqueue(this);

            if (!isToastShowing)
            {
                ShowNextToast();
            }
        });
    }

    private ToastWindow CreateToastWindow(string message)
    {
        return new ToastWindow(ParentWindow ?? System.Windows.Application.Current.MainWindow, message,
            BackgroundColor, TextColor, FontFamily, FontSize);
    }

    private void ShowNextToast()
    {
        if (toastQueue.Count > 0)
        {
            var nextToast = toastQueue.Dequeue();

            isToastShowing = true;

            var toastWindow = CreateToastWindow(nextToast.Message ?? "");

            toastWindow.Closed += (sender, args) =>
            {
                isToastShowing = false;
                ShowNextToast();
            };

            toastWindow.Show();

            var timer = new DispatcherTimer();
            timer.Tick += (sender, args) =>
            {
                CloseWithFadeout(toastWindow, timer);
            };
            timer.Interval = TimeSpan.FromMilliseconds(nextToast.Duration);
            timer.Start();
        }
    }

    private static void CloseWithFadeout(ToastWindow toastWindow, DispatcherTimer timer)
    {
        DoubleAnimation animation = new(1.0, 0.0, TimeSpan.FromMilliseconds(500));
        animation.Completed += (sender, args) =>
        {
            toastWindow.Close();
            timer.Stop();
        };
        toastWindow.BeginAnimation(UIElement.OpacityProperty, animation);
    }
}

public class ToastWindow : Window
{
    public ToastWindow(Window parentWindow, string message,
        System.Windows.Media.Brush backgroundColor, System.Windows.Media.Brush textColor,
        System.Windows.Media.FontFamily? fontFamily, double fontSize)
    {
        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = System.Windows.Media.Brushes.Transparent;
        Topmost = true;
        ShowInTaskbar = false;

        TextBlock textBlock = new()
        {
            Text = message,
            Background = System.Windows.Media.Brushes.Transparent,
            Foreground = textColor,
            Padding = new Thickness(10),
            TextAlignment = TextAlignment.Center,
            Opacity = 0.9,
            FontSize = fontSize,
            TextWrapping = TextWrapping.Wrap,
        };
        if (fontFamily != null)
        {
            FontFamily = fontFamily;
        }

        Border border = new()
        {
            Background = backgroundColor,
            CornerRadius = new CornerRadius(20),
            Opacity = 0.0,
            MinWidth = 300,
            MaxWidth = 600,
            Child = textBlock
        };

        SizeToContent = SizeToContent.WidthAndHeight;

        Content = border;

        WindowStartupLocation = WindowStartupLocation.Manual;

        Loaded += (sender, e) =>
        {
            var parentScreen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(parentWindow).Handle);

            Left = parentScreen.WorkingArea.Left + (parentScreen.WorkingArea.Width / 2) - (Width / 2);
            Top = parentScreen.WorkingArea.Top + parentScreen.WorkingArea.Height - Height - 10;

            DoubleAnimation fadeInAnimation = new(0.0, 0.9, TimeSpan.FromMilliseconds(500));
            border.BeginAnimation(UIElement.OpacityProperty, fadeInAnimation);
        };
    }
}