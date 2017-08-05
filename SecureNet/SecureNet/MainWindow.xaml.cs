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
        protected string saveFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString() + "PFX.PFX";
        SecureNet.Pages.Browser.Logs logsPage = new SecureNet.Pages.Browser.Logs();

        public static List<DataObject> DataObjects { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            EncryptConnString();
            Style = (Style)FindResource(typeof(Window));
            EncryptConnString();
                
            StartFiddler();

            FiddlerApplication.BeforeRequest += logsPage.FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += logsPage.FiddlerApplication_AfterSessionComplete;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
        }

        public class DataObject
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string E { get; set; }
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
            //FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            //FiddlerApplication.Startup(8877, true, true);

            if (FiddlerApplication.IsStarted())
            {
                FiddlerApplication.Shutdown();
                Console.WriteLine("** Closing previous instance of Fiddler");
            }

            //INstallation of cert
            try
            {
                Console.WriteLine("** Retrieving Cert...");
                RetrieveCertificate();
            }
            catch
            {
                Console.WriteLine("** Installing Cert...");
                InstallCertificate();
            }

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

        public void CheckCertificate()
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
        }

        private void RequestDetails(Session oSession)
        {
            Console.WriteLine("Request URL {0}", oSession.fullUrl);// getting only http traffic details

        }

        public void InstallCertificate() //Create & trust cert, saves PFX file
        {
            CertMaker.createRootCert();
            CertMaker.trustRootCert();

            var certX = CertMaker.oCertProvider.GetCertificateForHost("<Machine Name>");
            File.WriteAllBytes(saveFile, certX.Export(X509ContentType.SerializedCert));
            //File.WriteAllBytes(@"C:\PFX.PFX", certX.Export(X509ContentType.SerializedCert));
        }

        public void RetrieveCertificate()
        {
            Console.WriteLine("** PFX file is at: " + saveFile);

            X509Store certStore = new X509Store(StoreName.Root, StoreLocation.CurrentUser);
            // Try to open the store.

            certStore.Open(OpenFlags.ReadOnly);
            // Find the certificate that matches the name.
            X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindBySubjectName, "DO_NOT_TRUST_FiddlerRoot", false);

            X509Certificate2 certTry = new X509Certificate2(saveFile, "1", X509KeyStorageFlags.UserKeySet |
                                        X509KeyStorageFlags.PersistKeySet |
                                        X509KeyStorageFlags.Exportable);

#region verify trusted cert
            /*
                       bool checktrust = certTry.Verify();

                       if (checktrust == true)
                       {
                           Console.WriteLine("** Cert verified");
                       }
                       else if (checktrust == false)
                       {
                           Console.WriteLine("** Cert UNverified");

                       }


                       bool checktrust = CertMaker.rootCertIsMachineTrusted();
                       if (checktrust == true)
                       {
                           Console.WriteLine("** cert is trusted");
                       }
                       else if (checktrust == false)
                       {
                           Console.WriteLine("** cert not trusted");
                           InstallCertificate();
                       }*/
#endregion
        }

        private void FiddlerApplication_OnNotification(object sender, NotificationEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override void OnClosed(EventArgs e)
        {

            FiddlerApplication.oProxy.Detach();
            Console.WriteLine("** Detached proxy");

            //removeFiddler();

            FiddlerApplication.Shutdown();
            Console.WriteLine("** Fiddler Closed");

            logsPage.ExportToFile();
        }

        private async void reemoveFiddler()
        {
            Console.WriteLine("Waiting for 5s");
            await Task.Delay(500000000);
        }

    }
}
