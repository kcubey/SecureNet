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
            Console.WriteLine("Redirect to " +redirectAdd);
        }

        private void CheckVT(object sender, RoutedEventArgs e)
        {
            startVTAsync(ScanTxtBox.Text);
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

        private async void startVTAsync(string scanText)
        {

            //If textbox empty, won't scan
            if (string.IsNullOrEmpty((ScanTxtBox.Text))) return;
            
            VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
            vt.UseTLS = true;
            UrlReport urlReport = await vt.GetUrlReport(scanText);

            bool hasUrlBeenScannedBefore = urlReport.ResponseCode == ReportResponseCode.Present;
            Console.WriteLine(hasUrlBeenScannedBefore);
            Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));
            MessageBox.Show("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));

            //If the url has been scanned before, the results are embedded inside the report.
            if (hasUrlBeenScannedBefore)
            {
                PrintScan(urlReport);
            }
            else
            {
                UrlScanResult urlResult = await vt.ScanUrl(scanText);
                PrintScan(urlResult);
            }

        }
        private static void PrintScan(UrlScanResult scanResult)
        {
            Console.WriteLine("Scan ID: " + scanResult.ScanId);
            Console.WriteLine("Message: " + scanResult.VerboseMsg);
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
}
