using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
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
using VirusTotalNET;
using VirusTotalNET.Objects;
using VirusTotalNET.ResponseCodes;
using VirusTotalNET.Results;

namespace SecureNet.Pages.Browser
{
    /// <summary>
    /// Interaction logic for BrowserHome.xaml
    /// </summary>
    public partial class BrowserHome : Page
    {
        public BrowserHome()
        {
            Console.WriteLine("navigate success");
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));

            var list = new ObservableCollection<DataObject>();
            list.Add(new DataObject() { A = 6, B = 7, C = 5 });
            list.Add(new DataObject() { A = 5, B = 8, C = 4 });
            list.Add(new DataObject() { A = 4, B = 3, C = 0 });
            this.dataGrid1.ItemsSource = list;
        }

        public class DataObject
        {
            public int A { get; set; }
            public int B { get; set; }
            public int C { get; set; }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("redirecting now");
        }

        private void tempVT(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("WIP","SecureNet");
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    lbFiles.Items.Add(System.IO.Path.GetFileName(filename));
                    FileInfo fileinfo = new FileInfo(System.IO.Path.GetFileName(filename));
                    lbFiles.Items.Add(fileinfo);
                    //File.WriteAllBytes(fileinfo.FullName, fileinfo.GetObjectData());
                }

            }
        }

        private void istruebool(object sender, RoutedEventArgs e)
        {
            if (lbFiles.HasItems == true)
            {
                istrue.Content = "True";
            }
            else if (lbFiles.HasItems == false)
            {
                istrue.Content = "False";
            }

        }

        private void startVT(object sender, RoutedEventArgs e)
        {
            VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
            vt.UseTLS = true;

            //vt.ScanFile();

        }

//======================TESTING
#region VT test
        private const string ScanUrl = "http://www.google.com/";

        private void VT(object sender, RoutedEventArgs e)
        {
            VTasync().Wait();

        }

        private async Task VTasync()
        {
            VirusTotal virusTotal = new VirusTotal("6272a916af066e5af228854976345296bbb9a55bde176fc4752c9532899054d6");

            //Use HTTPS instead of HTTP
            virusTotal.UseTLS = true;

            //Create the EICAR test virus. See http://www.eicar.org/86-0-Intended-use.html
            FileInfo fileInfo = new FileInfo("EICAR.txt");
            File.WriteAllText(fileInfo.FullName, @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*");

            //Check if the file has been scanned before.
            FileReport fileReport = await virusTotal.GetFileReport(fileInfo);

            bool hasFileBeenScannedBefore = fileReport.ResponseCode == ReportResponseCode.Present;

            Console.WriteLine("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));

            //If the file has been scanned before, the results are embedded inside the report.
            if (hasFileBeenScannedBefore)
            {
                PrintScan(fileReport);
            }
            else
            {
                ScanResult fileResult = await virusTotal.ScanFile(fileInfo);
                PrintScan(fileResult);
            }

            Console.WriteLine();

            UrlReport urlReport = await virusTotal.GetUrlReport(ScanUrl);

            bool hasUrlBeenScannedBefore = urlReport.ResponseCode == ReportResponseCode.Present;
            Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));

            //If the url has been scanned before, the results are embedded inside the report.
            if (hasUrlBeenScannedBefore)
            {
                PrintScan(urlReport);
            }
            else
            {
                UrlScanResult urlResult = await virusTotal.ScanUrl(ScanUrl);
                PrintScan(urlResult);
            }
        }

        private static void PrintScan(UrlScanResult scanResult)
        {
            Console.WriteLine("Scan ID: " + scanResult.ScanId);
            Console.WriteLine("Message: " + scanResult.VerboseMsg);
            Console.WriteLine();
        }

        private static void PrintScan(ScanResult scanResult)
        {
            Console.WriteLine("Scan ID: " + scanResult.ScanId);
            Console.WriteLine("Message: " + scanResult.VerboseMsg);
            Console.WriteLine();
        }

        private static void PrintScan(FileReport fileReport)
        {
            Console.WriteLine("Scan ID: " + fileReport.ScanId);
            Console.WriteLine("Message: " + fileReport.VerboseMsg);

            if (fileReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }

        private static void PrintScan(UrlReport urlReport)
        {
            Console.WriteLine("Scan ID: " + urlReport.ScanId);
            Console.WriteLine("Message: " + urlReport.VerboseMsg);

            if (urlReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in urlReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }
    }
#endregion
}
