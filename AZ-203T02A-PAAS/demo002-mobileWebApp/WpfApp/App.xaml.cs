﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.WindowsAzure.MobileServices;

namespace WpfApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static MobileServiceClient MobileService = new MobileServiceClient("https://mobileapp002.azurewebsites.net");
    }
}
