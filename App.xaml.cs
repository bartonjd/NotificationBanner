using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Tasks;
using System.Windows;

namespace LogonAcceptanceWindow
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        protected override void OnStartup(StartupEventArgs e)
        {
            Boolean exists = Reg.PropertyExists(@"HKLM\SOFTWARE\NotificationBanner", "Banner");
            base.OnStartup(e);


        }

    }
}
