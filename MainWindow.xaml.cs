using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
using System.Management;
using System.Diagnostics;

namespace LogonAcceptanceWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public class DpiDecorator : Decorator
    {
        public DpiDecorator()
        {
            this.Loaded += (s, e) =>
            {
                System.Windows.Media.Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
                ScaleTransform dpiTransform = new ScaleTransform(1 / m.M11, 1 / m.M22);
                if (dpiTransform.CanFreeze)
                    dpiTransform.Freeze();
                this.LayoutTransform = dpiTransform;
            };
        }
    }
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            //Closing += OnClosing;
            //StateChanged += OnStateChanged;
            //Deactivated += (sender, args) => Activate();
            //LostFocus += (sender, args) => Focus();
            //LostMouseCapture += (sender, args) => Mouse.Capture(this);
            //LostKeyboardFocus += (sender, args) => Keyboard.Focus(this);
            //PreviewLostKeyboardFocus += (sender, args) => Keyboard.Focus(this);
            

        }

        public double[] GetDpiScale() {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiX = m.M11;
            double dpiY = m.M22;

            return new[] { dpiX , dpiY};
            
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;
            double sr = Width / Height;
            double br = LayBodyArea.ActualWidth / 280;
            OverScroll.MaxHeight = LayBodyArea.ActualHeight * .6;

            NoticeTitle.FontSize *= 1.25;
            NoticeTitle.MaxWidth *= 1.25;
            NoticeText.FontSize *= 1.25;
            NoticeText.MaxWidth *= 1.25;

            NoticeBanner.Visibility = Visibility.Visible;

            string? title = Reg.GetString(@"HKLM:\SOFTWARE\LawBanner", "NotificationTitle");
            NoticeTitle.Content = title;

            //Retrieve Notification Text from Registry (Set via Group Policy), Handle new lines (\n)
            string[]? text = Reg.GetMultiString(@"HKLM:\SOFTWARE\LawBanner", "NotificationText");
            string? noticeText = String.Join(System.Environment.NewLine, value: text);
            NoticeText.Text = noticeText.Replace("\\n", Environment.NewLine);

            //Get background color, lighten and use as button color 
            Color bg = (NoticeBanner.Background as SolidColorBrush).Color;
            Color newBg = Utils.AdjustColorBrightness((System.Drawing.Color.FromArgb(bg.A, bg.R, bg.G, bg.B)), .18);
            AcceptBtn.Background = new SolidColorBrush(newBg);
            AcceptBtn.Height *= 1.1;
            
            //AcceptBtn.Foreground = Utils.AdjustColorBrightness(NoticeBanner.Background.,0.25);


            //WindowStyle = WindowStyle.None;
            //WindowState = WindowState.Maximized;
            //Topmost = true;
            // Other stuff here

        }

        private void OnClosing(object sender, CancelEventArgs e)
        {

        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
                WindowState = WindowState.Maximized;
        }

        private void AcceptBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }

}
