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
using System.Resources;
using System.Management;
using System.Windows.Markup;
using System.Diagnostics;
using System.Reflection;
using System.Collections;
using System.Text.RegularExpressions;

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
    public static class Themes
    {
        public const  String CustomTheme = "CustomTheme";
        public const  String Windows10 = "Windows10";
        public const String Windows11 = "Windows11";
        public static String[] BtnStyleProperties = new String[] {
            "Btn.Color",
            "Btn.HoverColor",
            "Btn.BorderColor",
            "Btn.MouseDownColor",
            "Btn.BorderMouseDownColor",
            "Btn.BorderHoverColor",
            "Btn.TextColor",
        };
    }

    public partial class MainWindow : Window
    {

        private string? Namespace;
        private const String FALLBACKTHEME = Themes.Windows10;
        private const string REGISTRYROOT = @"HKLM:\SOFTWARE\NotificationBanner\";
        public string BackgroundColor = "Red";


        public MainWindow()
        {
            InitializeComponent();
            this.Namespace = this.GetType().Namespace;
            Loaded += OnLoaded;
            //Closing += OnClosing;
            //StateChanged += OnStateChanged;
            //Deactivated += (sender, args) => Activate();
            //LostFocus += (sender, args) => Focus();
            //LostMouseCapture += (sender, args) => Mouse.Capture(this);
            //LostKeyboardFocus += (sender, args) => Keyboard.Focus(this);
            //PreviewLostKeyboardFocus += (sender, args) => Keyboard.Focus(this);


        }

        public double[] GetDpiScale()
        {
            Matrix m = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice;
            double dpiX = m.M11;
            double dpiY = m.M22;

            return new[] { dpiX, dpiY };

        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;


            OverScroll.MaxHeight = LayBodyArea.ActualHeight * .65;

            NoticeTitle.FontSize *= 1.25;
            NoticeTitle.MaxWidth *= 1.25;
            NoticeText.FontSize *= 1.25;
            NoticeText.MaxWidth *= 1.25;

            NoticeBanner.Visibility = Visibility.Visible;

            string? title = Reg.GetString(@"HKLM:\SOFTWARE\NotificationBanner", "Title");
            NoticeTitle.Content = title;

            //Retrieve Notification Text from Registry (Set via Group Policy), Handle new lines (\n)
            string[]? text = Reg.GetMultiString(@"HKLM:\SOFTWARE\NotificationBanner", "Text");
            string? noticeText = String.Join(System.Environment.NewLine, value: text);
            NoticeText.Text = noticeText.Replace("\\n", Environment.NewLine);


            AcceptBtn.Height *= 1.1;

            //AcceptBtn.Foreground = Utils.AdjustColorBrightness(NoticeBanner.Background.,0.25);


            //WindowStyle = WindowStyle.None;
            //WindowState = WindowState.Maximized;
            //Topmost = true;
            // Other stuff here

        }

        private void AcceptBtn_Loaded(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;

            string? theme = Reg.GetString(@"HKLM:\SOFTWARE\NotificationBanner", "Theme");
            string? ButtonColor = Reg.GetString($@"HKLM:\SOFTWARE\NotificationBanner\Style\{theme}", "ButtonColor");
            string? BackgroundColor = Reg.GetString($@"HKLM:\SOFTWARE\NotificationBanner\Style\{theme}", "BackgroundColor");
            string? ButtonTextColor = Reg.GetString($@"HKLM:\SOFTWARE\NotificationBanner\Style\{theme}", "TextColor");




            var str = GetThemeStyle();

            button.Style = (Style)XamlReader.Parse(str);
            //button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ButtonColor));
            //button.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(ButtonTextColor));
            //NoticeBanner.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(BackgroundColor));


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
            Debug.WriteLine(AcceptBtn.Template.ToString());
            this.Close();
        }
        private string GetTheme()
        {
            string? theme = Reg.GetString(@"HKLM:\SOFTWARE\NotificationBanner", "Theme");
            bool customTheme = Reg.GetBoolFromInt(@"HKLM:\SOFTWARE\NotificationBanner", "CustomTheme");

            if (null == theme)
            {
                theme = MainWindow.FALLBACKTHEME;
            }

            if (customTheme == true)
            {
                return theme;
            }
            else
            {

                return theme;
            }

            //ResourceManager rm = new ResourceWriter($"{Namespace}.Windows10Theme", Assembly.GetExecutingAssembly());
            // foreach (DictionaryEntry entry in resourceSet)
        }
        private string GetThemeResource(String themeName) {

            return $"{Namespace}.{themeName}Theme";
        }
        private dynamic GetThemeStyle()
        {
            string configuredTheme = this.GetTheme();
            string themeXaml = "";
            SolidColorBrush TextColor, BackgroundColor, BtnTextColor;
            if (configuredTheme == Themes.CustomTheme)
            {
                //Get Dictionary of custom style values from registry
                var themeStyles = Reg.GetProperties($"{REGISTRYROOT}Style\\{configuredTheme}", Themes.BtnStyleProperties);
                themeXaml = ButtonStyle.Tpl;
                foreach (KeyValuePair<string, dynamic> style in themeStyles)
                {
                    themeXaml = themeXaml.Replace($"{{style.Key}}", style.Value);
                }
                TextColor = Utils.GetColorBrush(themeStyles["TextColor"]);
                BackgroundColor = Utils.GetColorBrush(themeStyles["BackgroundColor"]);
                BtnTextColor = Utils.GetColorBrush(themeStyles["Btn.TextColor"]);
            }
            else
            {
                themeXaml = ButtonStyle.Tpl;
                ResourceManager themeResource = new ResourceManager(GetThemeResource(configuredTheme), Assembly.GetExecutingAssembly());
                //Replace curly brace values in template with values stored in theme resource file which are button related
                
                foreach (string styleProperty in Themes.BtnStyleProperties)
                {
                   // if (btnPattern.Matches(styleProperty))
                    themeXaml = themeXaml.Replace($"[{styleProperty}]",themeResource.GetString(styleProperty));
                }
                TextColor = Utils.GetColorBrush(themeResource.GetString("TextColor"));
                BackgroundColor = Utils.GetColorBrush(themeResource.GetString("BackgroundColor"));
                BtnTextColor = Utils.GetColorBrush(themeResource.GetString("Btn.TextColor"));

            }

            NoticeBanner.Background = BackgroundColor;
            NoticeTitle.Foreground = TextColor;
            NoticeText.Foreground = TextColor;
            AcceptBtn.Foreground = BtnTextColor;

            if (themeXaml == "")
            {
                //Handle error message
                this.Close();
            }
            return themeXaml;
        }





    }

}
