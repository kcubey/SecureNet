﻿using SecureNet.Pages;
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
using Fiddler;
using System.Configuration;

namespace SecureNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            EncryptConnString();
            Style = (Style)FindResource(typeof(Window));

            /*
            if (!string.IsNullOrEmpty(App.Configuration.UrlCapture.Cert))
            {
                FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.key", App.Configuration.UrlCapture.Key);
                FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.cert", App.Configuration.UrlCapture.Cert);
            }
            */
            StartFiddler();

            //         MainFrame.NavigationService.GoBack();
            //       MainFrame.NavigationService.GoForward();
            //     MainFrame.NavigationService.Refresh();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
            //this.MainFrame.Navigate(typeof(Page), ((Button)sender).CommandParameter.ToString());
        }

        void StartFiddler()
        {
            //CONFIG.IgnoreServerCertErrors = false;

            //FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            //FiddlerApplication.Startup(8877, true, true);

            var cert = InstallCertificate();

            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            Console.WriteLine("Fiddler Start");

            FiddlerApplication.OnNotification += delegate(object sender, NotificationEventArgs oNEA)
            {
                Console.WriteLine("** NotifyUser: " + oNEA.NotifyString);
            };

            FiddlerApplication.Log.OnLogString += delegate (object sender, LogEventArgs oLEA)
            {
                Console.WriteLine("** LogString: " + oLEA.LogString);
            };

            

        }

        private static void RequestDetails(Session oSession)
        {
            Console.WriteLine("Request URL {0}", oSession.fullUrl);// getting only http traffic details

        }


        public static bool InstallCertificate()
        {
            if (!CertMaker.rootCertExists())
            {
                if (!CertMaker.createRootCert())
                    return false;

                if (!CertMaker.trustRootCert())
                    return false;
                /*
                App.Configuration.UrlCapture.Cert =
                    FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.cert", null);
                App.Configuration.UrlCapture.Key =
                    FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.key", null);
                    */
            }

            return true;
        }

        private void EncryptConnString()
        {
            // Open the app.config file.
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Get the section in the file.
            ConfigurationSection section = config.GetSection("connectionStrings");

            // If the section exists and the section is not readonly, then protect the section.
            if (section != null)
            {
                if (!section.IsReadOnly())
                {
                    // Protect the section.
                    section.SectionInformation.ProtectSection("RsaProtectedConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    // Save the change.
                    config.Save(ConfigurationSaveMode.Modified);

                }
            }
        }

        private void FiddlerApplication_OnNotification(object sender, NotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnClosed(EventArgs e)
        {
            FiddlerApplication.oProxy.Detach();
            FiddlerApplication.Prefs.SetBoolPref("fiddler.certmaker.CleanupServerCertsOnExit", true);
            FiddlerApplication.Shutdown();
            Console.WriteLine("Fiddler Closed");
        }


    }
}
