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
using System.Security.Cryptography.X509Certificates;

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

            StartFiddler();
            EncryptConnString();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
        }

        void StartFiddler()
        {
            //FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            //FiddlerApplication.Startup(8877, true, true);

            if (FiddlerApplication.IsStarted())
                FiddlerApplication.Shutdown();

            DoCertificate();

            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            Console.WriteLine("Fiddler Start");

            FiddlerApplication.OnNotification += delegate (object sender, NotificationEventArgs oNEA)
            {
                Console.WriteLine("** NotifyUser: " + oNEA.NotifyString);
            };

            FiddlerApplication.Log.OnLogString += delegate (object sender, LogEventArgs oLEA)
            {
                Console.WriteLine("** LogString: " + oLEA.LogString);
            };

        }

        private void RequestDetails(Session oSession)
        {
            Console.WriteLine("Request URL {0}", oSession.fullUrl);// getting only http traffic details

        }


        public void DoCertificate()
        {
            var checkCert = CertMaker.GetRootCertificate();

            if (checkCert == null)
            {
                Console.WriteLine("Cert does not exist");
                CertMaker.createRootCert();
                CertMaker.trustRootCert();
                Console.WriteLine("Cert created & trusted");
            }

            else if (checkCert != null)
            {
                Console.WriteLine("Cert exists");
                bool checktrust = CertMaker.rootCertIsMachineTrusted();
                if (checktrust == true)
                {
                    Console.WriteLine("is trusted");
                }
                else if (checktrust == false)
                {
                    Console.WriteLine("not trusted");
                }
            }

            /*
            if (!CertMaker.rootCertExists())
            {
                Console.WriteLine("root cert does not exist");

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

        //does not work
        public void InstallCertificate()
        {
            X509Store certStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            // Try to open the store.

            certStore.Open(OpenFlags.ReadOnly);
            // Find the certificate that matches the name.
            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "DO_NOT_TRUST_FiddlerRoot", false);

            X509Certificate2 certTry = new X509Certificate2(@"D:\PFX.PFX", "1", X509KeyStorageFlags.UserKeySet |
                                        X509KeyStorageFlags.PersistKeySet |
                                        X509KeyStorageFlags.Exportable);
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
            CheckCertificate();
            Console.WriteLine("Detach proxy");

            CertMaker.removeFiddlerGeneratedCerts(false);
            //FiddlerApplication.Prefs.SetBoolPref("fiddler.certmaker.CleanupServerCertsOnExit", true);
            CheckCertificate();
            Console.WriteLine("Remove gen cert");

            FiddlerApplication.Shutdown();
            CheckCertificate();
            Console.WriteLine("Fiddler Closed");
        }


        public void CheckCertificate()
        {
            var checkCert = CertMaker.GetRootCertificate();

            if (checkCert == null) 
            {
                Console.WriteLine("Cert does not exist");
            }
            else if (checkCert != null)
            {
                Console.WriteLine("Cert exists");
            }
        }
    }
}
