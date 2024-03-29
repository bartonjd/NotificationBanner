﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Resources;
using System.Windows.Markup;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

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
                ScaleTransform dpiTransform = new (1 / m.M11, 1 / m.M22);
                if (dpiTransform.CanFreeze)
                    dpiTransform.Freeze();
                this.LayoutTransform = dpiTransform;
            };
        }
    }
    public static class Themes
    {
        public const String CustomTheme = "CustomTheme";
        public const String Windows10 = "Windows10";
        public const String Windows11 = "Windows11";
        public static readonly String[] BtnStyleProperties = new String[] {
            "Btn.Color",
            "Btn.HoverColor",
            "Btn.BorderColor",
            "Btn.MouseDownColor",
            "Btn.BorderMouseDownColor",
            "Btn.BorderHoverColor",
            "Btn.TextColor"
        };
    }

    public partial class MainWindow : Window
    {

        private const string Namespace = "LogonAcceptanceWindow";
        private const String FALLBACKTHEME = Themes.Windows10;
        private const string REGISTRYROOT = @"HKLM:\SOFTWARE\";
        private const string KEYNAME = @"NotificationBanner";
        private const string REGISTRYPATH = $"{REGISTRYROOT}{KEYNAME}\\";


        public MainWindow()
        {
            InitializeComponent();
            //this.Namespace = this.GetType().Namespace;
            Loaded += OnLoaded;
/*            Closing += OnClosing;
            StateChanged += OnStateChanged;
            Deactivated += (sender, args) => Activate();
            LostFocus += (sender, args) => Focus();
            LostMouseCapture += (sender, args) => Mouse.Capture(this);
            LostKeyboardFocus += (sender, args) => Keyboard.Focus(this);*/
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
            bool isEnabled = Reg.GetBoolFromInt($"{REGISTRYPATH}", "EnableBanner");
            //If banner is disabled in group policy/registry exit now
            if (!isEnabled) {
                this.Exit();
                return;
            }
            this.Focus();
            string? title = Reg.GetString($"{REGISTRYPATH}", "Title");
            Width = SystemParameters.PrimaryScreenWidth;
            Height = SystemParameters.PrimaryScreenHeight;


            OverScroll.MaxHeight = LayBodyArea.ActualHeight * .65;

            NoticeTitle.FontSize *= 1.25;
            NoticeTitle.MaxWidth *= 1.25;
            NoticeText.FontSize *= 1.25;
            NoticeText.MaxWidth *= 1.25;

            NoticeBanner.Visibility = Visibility.Visible;

            NoticeTitle.Content = title;

            //Retrieve Notification Text from Registry (Set via Group Policy), Handle new lines (\n)
            string[]? text = Reg.GetMultiString(REGISTRYPATH, "Text") ?? new string[] { "Please configure notification text" };

            string? noticeText = String.Join(System.Environment.NewLine, value: text);
            NoticeText.Text = noticeText.Replace("\\n", Environment.NewLine);


            AcceptBtn.Height *= 1.1;

            //AcceptBtn.Foreground = Utils.AdjustColorBrightness(NoticeBanner.Background.,0.25);


            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            //Topmost = true;
            //Other stuff here

        }

        private void AcceptBtn_Loaded(object sender, RoutedEventArgs e)
        {

            if (!(SetThemeStyle())) {
                SetThemeStyle(true);
            }

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
            this.Exit();
        }

        private string GetTheme()
        {
            //true if using a custom theme
            bool customTheme = Reg.GetBoolFromInt(REGISTRYPATH, "CustomTheme");
            string? theme = Reg.GetString(REGISTRYPATH, "Theme") ?? MainWindow.FALLBACKTHEME;

            if (customTheme == true)
            {
                return Themes.CustomTheme;
            }
            else
            {
                return theme;
            }
            
        }
        private string GetThemeResource(String themeName) {

            return $"{Namespace}.{themeName}Theme";
        }
/*        private void SetThemeStyle()
        {
            _SetThemeStyle(false);
        }*/

        private Boolean SetThemeStyle(Boolean useFallbackTheme = false)
        {

            string configuredTheme;
            if (!useFallbackTheme)
            {
                configuredTheme = this.GetTheme();
            } else
            {
                configuredTheme = FALLBACKTHEME;
            }
            string themeXaml;
            SolidColorBrush? TextColor, BackgroundColor, BtnColor, BtnTextColor;
            if (configuredTheme == Themes.CustomTheme)
            {
                //Get Dictionary of custom style values from registry
                var themeStyles = Reg.GetProperties($"{REGISTRYPATH}\\CustomTheme", Themes.BtnStyleProperties);
                themeXaml = CommonStrings.ButtonTpl;
                foreach (KeyValuePair<string, dynamic> style in themeStyles)
                {
                    themeXaml = themeXaml.Replace($"{{style.Key}}", style.Value);
                }
                TextColor = Utils.GetColorBrush(themeStyles["TextColor"]);
                BackgroundColor = Utils.GetColorBrush(themeStyles["BackgroundColor"]);
                BtnTextColor = Utils.GetColorBrush(themeStyles["Btn.TextColor"]);
                BtnColor = Utils.GetColorBrush(themeStyles["Btn.Color"]);
                //If any colors in the custom theme are null, return false and re-run with fallback theme
                if ((TextColor is null) || (BackgroundColor is null) || (BtnTextColor is null) || (BtnColor is null))
                {
                    return false;

                }
            }
            else
            {
                themeXaml = CommonStrings.ButtonTpl;
                ResourceManager themeResource = new (GetThemeResource(configuredTheme), Assembly.GetExecutingAssembly());
                //Replace square brace values in template with values stored in theme resource file which are button related
                
                foreach (String styleProperty in Themes.BtnStyleProperties)
                {
                    themeXaml = themeXaml.Replace($"[{styleProperty}]",themeResource.GetString(styleProperty));
                }
                TextColor = Utils.GetColorBrush(themeResource.GetString("TextColor"));
                BackgroundColor = Utils.GetColorBrush(themeResource.GetString("BackgroundColor"));
                BtnTextColor = Utils.GetColorBrush(themeResource.GetString("Btn.TextColor"));
                BtnColor = Utils.GetColorBrush(themeResource.GetString("Btn.Color"));
            }

            NoticeBanner.Background = BackgroundColor;
            NoticeTitle.Foreground = TextColor;
            NoticeText.Foreground = TextColor;
            AcceptBtn.Foreground = BtnTextColor;
            AcceptBtn.Background = BtnColor;

            if (themeXaml == "")
            {
                //Handle error message
                this.Exit();
            }
            AcceptBtn.Style = (Style)XamlReader.Parse(themeXaml);
            return true;
        }
        private static void Window_Show() {

        }
        private void Exit()
        {

            Process.Start("userinit.exe");
            this.Close();
            System.Environment.Exit(0);
        }
    }

}
