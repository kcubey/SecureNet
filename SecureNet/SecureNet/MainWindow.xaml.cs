using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Fiddler;
using System.Configuration;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace SecureNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        SecureNet.Pages.Browser.Logs logsPage = new SecureNet.Pages.Browser.Logs();

        public MainWindow()
        {
            InitializeComponent();
            EncryptConnString();
            Style = (Style)FindResource(typeof(Window));

            StartFiddler();

            FiddlerApplication.BeforeRequest += logsPage.FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += logsPage.FiddlerApplication_AfterSessionComplete;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
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

        void StartFiddler()
        {
            if (FiddlerApplication.IsStarted())
            {
                FiddlerApplication.Shutdown();
                Console.WriteLine("** Closing previous instance of Fiddler");
            }

            //Retrieval of cert
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["FiddlerCert"]))
            {
                Console.WriteLine("** Retrieving certificate");
                FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.key", ConfigurationManager.AppSettings["FiddlerKey"]);
                FiddlerApplication.Prefs.SetStringPref("fiddler.certmaker.bc.cert", ConfigurationManager.AppSettings["FiddlerCert"]);
            }

            //Creation of cert
            InstallCertificate();
            
            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            Console.WriteLine("** Fiddler Start");

            FiddlerApplication.OnNotification += delegate (object sender, NotificationEventArgs oNEA)
            {
                Console.WriteLine("** NotifyUser: " + oNEA.NotifyString);
            };

            FiddlerApplication.Log.OnLogString += delegate (object sender, LogEventArgs oLEA)
            {
                Console.WriteLine("** LogString: " + oLEA.LogString);
            };

        }

        public static bool InstallCertificate()
        {
            if (!CertMaker.rootCertExists())
            {
                Console.WriteLine("** Rootcert does not exist");
                if (!CertMaker.createRootCert())
                {
                    Console.WriteLine("** Creating root cert");
                    return false;
                }

                if (!CertMaker.trustRootCert())
                {
                    Console.WriteLine("** Trusting root cert");
                    return false;
                }

                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

                config.AppSettings.Settings["FiddlerCert"].Value = FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.cert", null);
                config.AppSettings.Settings["FiddlerKey"].Value = FiddlerApplication.Prefs.GetStringPref("fiddler.certmaker.bc.key", null);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");
            }

            return true;
            
        }

        protected override void OnClosed(EventArgs e)
        {
            ShutdownFiddler();
        }

        private void ShutdownFiddler()
        {
            FiddlerApplication.oProxy.Detach();
            Console.WriteLine("** Detached proxy");

            FiddlerApplication.Shutdown();
            Console.WriteLine("** Fiddler closed");

            logsPage.ExportToFile();
            MessageBoxResult result = MessageBox.Show("Do you want to uninstall certificate?", "SecureNet", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    {
                        UninstallCertificate();
                    }
                    break;
                case MessageBoxResult.No:
                    {
                        break;
                    }
            }
        }

        public static bool UninstallCertificate()
        {
            if (CertMaker.rootCertExists())
            {
                if (!CertMaker.removeFiddlerGeneratedCerts(true))
                    return false;
            }

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            config.AppSettings.Settings["FiddlerCert"].Value = null;
            Console.WriteLine("Cert is: " + ConfigurationManager.AppSettings["FiddlerCert"]);

            config.AppSettings.Settings["FiddlerKey"].Value = null;
            Console.WriteLine("Key is: " + ConfigurationManager.AppSettings["FiddlerKey"]);

            config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

            Console.WriteLine("uninstall cert");
            return true;
        }

    }
}
