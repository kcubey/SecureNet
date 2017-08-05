﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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
using SecureNet.Classes;

namespace SecureNet.Pages.Browser
{
    /// <summary>
    /// Interaction logic for Scan.xaml
    /// </summary>
    public partial class Scan : Page
    {
        public Scan()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
        }

        //Change cursor when waiting
        public class WaitCursor : IDisposable
        {
            private Cursor _previousCursor;

            public WaitCursor()
            {
                _previousCursor = Mouse.OverrideCursor;

                Mouse.OverrideCursor = Cursors.Wait;
            }

            #region IDisposable Members

            public void Dispose()
            {
                Mouse.OverrideCursor = _previousCursor;
            }

            #endregion
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("** Redirect to " + redirectAdd);
        }

        private void CheckVT(object sender, RoutedEventArgs e)
        {
            startVTAsyncURL(ScanTxtBox.Text);
            ScanTxtBox.Text = String.Empty;
        }

        private void CheckVTFile(object sender, RoutedEventArgs e)
        {
            startVTAsyncFile();
            ScanFile.Text = String.Empty;
        }

        private void btnOpenFiles_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ScanFile.Text))
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "All files (*.*)|*.*";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (openFileDialog.ShowDialog() == true)
                {
                    String path = openFileDialog.FileName; // get name of file
                    String size = Convert.ToString(openFileDialog.FileName.Length); // get size of file
                    if (openFileDialog.FileName.Length > 104857600) // 100mb, may want to change to something smaller
                    {
                        ScanFile.Text = "File exceeds upload limit";
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(new FileStream(path, FileMode.Open), new UTF8Encoding())) // do anything you want, e.g. read it
                        {
                            ScanFile.Text = path;
                        }
                    }
                }
            }

            else
            {
                MessageBox.Show("File has been uploaded");
            }
        }
        private void btnClearFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(ScanFile.Text))
                ScanFile.Text = "";
        }




        /// <summary>
        /// URL Scan
        /// </summary>
        /// <param name="urlText"></param>
        private async void startVTAsyncURL(string urlText)
        {
            using (new WaitCursor())
            {

                //If textbox empty, won't scan
                if (string.IsNullOrEmpty((ScanTxtBox.Text))) return;

                VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
                vt.UseTLS = true;
                UrlReport urlReport = await vt.GetUrlReport(urlText);

                bool hasUrlBeenScannedBefore = urlReport.ResponseCode == ReportResponseCode.Present;

                Console.WriteLine(hasUrlBeenScannedBefore);
                Console.WriteLine("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));
                MessageBox.Show("URL has been scanned before: " + (hasUrlBeenScannedBefore ? "Yes" : "No"));
                MessageBox.Show(urlText + " scanned");

                //If the url has been scanned before, the results are embedded inside the report.
                if (hasUrlBeenScannedBefore)
                {
                    PrintScan(urlReport);
                }
                else
                {
                    UrlScanResult urlResult = await vt.ScanUrl(urlText);
                    PrintScan(urlResult);
                }
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
            MessageBox.Show("Scan ID: " + urlReport.ScanId);

            if (urlReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in urlReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }



        private async void startVTAsyncFile()
        {
            using (new WaitCursor())
            {

                if (string.IsNullOrEmpty((ScanFile.Text)))

                    return;

                VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
                vt.UseTLS = true;
                FileInfo fileInfo = new FileInfo(ScanFile.Text);
                byte[] byteArray;
                using (StreamReader reader = new StreamReader(new FileStream(ScanFile.Text, FileMode.Open), new UTF8Encoding())) // do anything you want, e.g. read it
                {
                    byteArray = Encoding.UTF8.GetBytes(reader.ReadToEnd());
                }

                File.WriteAllText(fileInfo.FullName, System.Text.Encoding.UTF8.GetString(byteArray));
                //MessageBox.Show(System.Text.Encoding.UTF8.GetString(byteArray));
                FileReport fileReport = await vt.GetFileReport(fileInfo);
                bool hasFileBeenScannedBefore = fileReport.ResponseCode == ReportResponseCode.Present;

                Console.WriteLine("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));
                MessageBox.Show("File has been scanned before: " + (hasFileBeenScannedBefore ? "Yes" : "No"));

                //If the file has been scanned before, the results are embedded inside the report.
                if (hasFileBeenScannedBefore)
                {
                    PrintScan(fileReport);
                }
                else
                {
                    ScanResult fileResult = await vt.ScanFile(fileInfo);
                    PrintScan(fileResult);


                }
            }
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
            MessageBox.Show("Scan ID: " + fileReport.ScanId);



            if (fileReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in fileReport.Scans)
                {
                    Console.WriteLine("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                }
            }

            Console.WriteLine();
        }

      
    }
}
