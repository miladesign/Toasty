using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Toasty.WPF;

/// <summary>
/// A toast is a view containing a quick little message for the user. The toast class helps you create and show those.
/// </summary>
/// <param name="window">Parent window</param>
public class Toast
{
    #region Constants
    /// <summary>
    /// Show the view or text notification for a short period of time.
    /// </summary>
    public const int LENGTH_SHORT = 2000;

    /// <summary>
    /// Show the view or text notification for a long period of time.
    /// </summary>
    public const int LENGTH_LONG = 3500;
    #endregion

    #region Variables
    private Window? ParentWindow;
    private static readonly Queue<Toast> toastQueue = new();
    private static bool isToastShowing = false;

    /// <summary>
    /// Gets or sets the text message to be displayed in the toast notification.
    /// </summary>
    public string? Message;

    /// <summary>
    /// Gets or sets the duration for which the toast message will be displayed (in milliseconds).
    /// </summary>
    public int Duration;

    /// <summary>
    /// Gets or sets the font family for the toast notification text.
    /// If set to null, the default font family will be used.
    /// </summary>
    public System.Windows.Media.FontFamily? FontFamily { get; set; } = null;

    /// <summary>
    /// Gets or sets the background color for the toast notification.
    /// The default color is black.
    /// </summary>
    public System.Windows.Media.Brush BackgroundColor { get; set; } = System.Windows.Media.Brushes.Black;

    /// <summary>
    /// Gets or sets the text color for the toast notification.
    /// The default color is white.
    /// </summary>
    public System.Windows.Media.Brush TextColor { get; set; } = System.Windows.Media.Brushes.White;

    /// <summary>
    /// Gets or sets the font size for the toast notification text.
    /// The default font size is 15.
    /// </summary>
    public double FontSize { get; set; } = 15;

    /// <summary>
    /// Gets or sets the font weight for the toast notification text.
    /// The default font weight is Normal.
    /// </summary>
    public FontWeight FontWeight { get; set; } = FontWeights.Normal;

    /// <summary>
    /// Gets or sets the font style for the toast notification text.
    /// The default font style is Normal.
    /// </summary>
    public System.Windows.FontStyle FontStyle { get; set; } = System.Windows.FontStyles.Normal;

    /// <summary>
    /// Occurs when the toast notification is shown.
    /// </summary>
    public event Action? OnShown;

    /// <summary>
    /// Occurs when the toast notification is hidden.
    /// </summary>
    public event Action? OnHidden;

    /// <summary>
    /// Gets or sets the flow direction for the toast notification text.
    /// The default flow direction is LeftToRight.
    /// </summary>
    public System.Windows.FlowDirection Direction { get; set; } = System.Windows.FlowDirection.LeftToRight;

    /// <summary>
    /// Gets or sets an optional tag (object) associated with the toast notification.
    /// This tag can be used to attach additional data or information to the notification.
    /// </summary>
    public object? Tag { get; set; } = null;

    #endregion

    #region Public Methods

    public Toast(Window parent)
    {
        ParentWindow = parent;
    }

    /// <summary>
    /// Make a standard toast that just contains text.
    /// </summary>
    /// <param name="window">Parent window</param>
    /// <param name="message">The text to show. Can be formatted text.</param>
    /// <param name="duration">How long to display the message. Either LENGTH_SHORT or LENGTH_LONG Value is LENGTH_SHORT, or LENGTH_LONG</param>
    /// <returns>Toast</returns>
    public static Toast MakeText(Window parent, string message, int duration)
    {
        return new Toast(parent)
        {
            Message = message,
            Duration = duration
        };
    }

    /// <summary>
    /// Show the view for the specified duration.
    /// </summary>
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

    #endregion

    #region Private Methods
    private static ToastWindow CreateToastWindow(System.Windows.Window? window, string message,
        System.Windows.Media.Brush backColor, System.Windows.Media.Brush textColor,
        System.Windows.Media.FontFamily? font, double fontSize, FontWeight fontWeight, System.Windows.FontStyle fontStyle,
        System.Windows.FlowDirection direction)
    {
        return new ToastWindow(window ?? System.Windows.Application.Current.MainWindow, message,
            backColor, textColor, font, fontSize, fontWeight, fontStyle, direction);
    }

    private static void ShowNextToast()
    {
        if (toastQueue.Count > 0)
        {
            var nextToast = toastQueue.Dequeue();

            isToastShowing = true;

            var toastWindow = CreateToastWindow(nextToast.ParentWindow, nextToast.Message ?? "", nextToast.BackgroundColor, nextToast.TextColor,
                nextToast.FontFamily, nextToast.FontSize, nextToast.FontWeight, nextToast.FontStyle, nextToast.Direction);

            toastWindow.Closed += (sender, args) =>
            {
                isToastShowing = false;
                ShowNextToast();
            };

            toastWindow.Show();

            nextToast.OnShown?.Invoke();

            var timer = new DispatcherTimer();
            timer.Tick += (sender, args) =>
            {
                CloseWithFadeout(toastWindow, timer, nextToast.OnHidden);
            };
            timer.Interval = TimeSpan.FromMilliseconds(nextToast.Duration);
            timer.Start();
        }
    }

    private static void CloseWithFadeout(ToastWindow toastWindow, DispatcherTimer timer, Action? onHidden)
    {
        DoubleAnimation animation = new(1.0, 0.0, TimeSpan.FromMilliseconds(500));
        animation.Completed += (sender, args) =>
        {
            onHidden?.Invoke();
            toastWindow.Close();
            timer.Stop();
        };
        toastWindow.BeginAnimation(UIElement.OpacityProperty, animation);
    }

    #endregion
}

internal class ToastWindow : Window
{
    public ToastWindow(Window parentWindow, string message,
        System.Windows.Media.Brush backgroundColor, System.Windows.Media.Brush textColor,
        System.Windows.Media.FontFamily? fontFamily, double fontSize, FontWeight fontWeight, System.Windows.FontStyle fontStyle,
        System.Windows.FlowDirection direction)
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
            FontWeight = fontWeight,
            FontStyle = fontStyle,
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
        FlowDirection = direction;
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