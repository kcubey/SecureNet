﻿using System;
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

using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using Fiddler;
using System.Collections.ObjectModel;
using SecureNet.Classes;
using VirusTotalNET;
using VirusTotalNET.Results;
using VirusTotalNET.ResponseCodes;
using System.IO;
using VirusTotalNET.Objects;
using System.Text.RegularExpressions;

namespace SecureNet.Pages.Browser
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class Logs : Page
    {
        delegate void UpdateUI();
        public static List<DataObject> DataObjects { get; set; }
        public static List<ListObject> ListObjects { get; set; }
        public static bool checkIsNotSafe;
        public static int checkSafeInt; // 1 = safe, 2 = suspicious, 3 = malicious
        public static List<CheckedHostList> URLHostList;

        public Logs()
        {
            Console.WriteLine("** Navigate success");
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));

            if (DataObjects == null)
            {
                DataObjects = new List<DataObject>();
            }
            foreach (DataObject dataObject in DataObjects)
            {
                dataGrid1.Items.Add(dataObject);
            }

            if (ListObjects == null)
            {
                ListObjects = new List<ListObject>();
                ListObject newListObject = new ListObject()
                { website = "yahoo.com" };
                ListObjects.Add(newListObject);
                listBox1.Items.Add(newListObject.website.ToString());
            }
            foreach (ListObject listObject in ListObjects)
            {
                listBox1.Items.Add(listObject.website.ToString());
            }
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("** Redirect to " + redirectAdd);
        }

        public class DataObject
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string E { get; set; }
        }

        public class ListObject
        {
            public string website { get; set; }
        }

        public void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string longUrl = oSession.url; //Mostly url+port
            string shortUrl = null;
            string hostname = oSession.hostname;
            string dateTimeString = DateTime.Now.ToLocalTime().ToString();


            int delimiterColon = longUrl.IndexOf(':');

            //Gets URL only
            if (delimiterColon != -1)
            {
                shortUrl = longUrl.Substring(0, delimiterColon);
            }
            else
            {
                shortUrl = longUrl;
            }

            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = "Checking", E = dateTimeString };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);
                Console.WriteLine("Add to DataObject");

            }));
            
            bool malicious = checkBlacklist(oSession.hostname);

            if (malicious == true) //site is unsafe
            {
                oSession.Abort();
                Console.WriteLine("** Session Aborted");

                //update datagrid of failure
                dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
                {
                    DataObject newDataObject = new DataObject()
                    { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.state.ToString(), E = dateTimeString };
                    DataObjects.Add(newDataObject);
                    dataGrid1.Items.Add(newDataObject);
                    Console.WriteLine("Add to DataObject");

                }));
            }
        }

        private bool checkBlacklist(string hostname)
        {
            foreach (ListObject yo in ListObjects)
            {
                if (hostname == yo.website)
                    return true;
            }
            return false;
        }

        private void AddWebsite(object sender, RoutedEventArgs e)
        {
            AddBlacklistWebsitePopup inputDialog = new AddBlacklistWebsitePopup();
            if (inputDialog.ShowDialog() == true)
            {
                string website = inputDialog.Answer;

                ListObject newListObject = new ListObject()
                    { website =  website};
                    ListObjects.Add(newListObject);
                    listBox1.Items.Add(newListObject.website.ToString());
            }
        }

        
        private void RemoveWebsite(object sender, RoutedEventArgs e)
        {
            int index = listBox1.SelectedIndex;
            listBox1.Items.Remove(listBox1.SelectedItem);

            ListObjects.RemoveAt(index);
        }

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            string dateTimeString = DateTime.Now.ToLocalTime().ToString();

            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.state.ToString(), E = dateTimeString };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);
            }));
        }

        public void ExportToFile()
        {
            System.IO.Directory.CreateDirectory("Traffic Logs");

            string currentTime = DateTime.Now.ToLocalTime().ToString();
            currentTime = currentTime.Replace('/', '-');
            currentTime = currentTime.Replace(':', '-');

            string saveFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString()+ "Traffic Logs\\" + currentTime + ".csv";
            
            dataGrid1.SelectAllCells();
            dataGrid1.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dataGrid1);
            String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            dataGrid1.UnselectAllCells();
            System.IO.StreamWriter file = new System.IO.StreamWriter(saveFile);
            file.WriteLine(resultat);
            file.Close();
            Console.WriteLine("File exported");

        }

        //==================== UNUSED CODE====================

        #region Unused code

        #region malicious/suspicious checking; add to beforeRequest before/after checkBlackList
        /*
        AddtoHostList(hostname); //hardcoding of suspicious & malicious hosts

        #region checking if urlhostlist has checked host before
        for (int i = 0; i < URLHostList.Count; i++)
        {
            if (URLHostList.ToString().Contains(hostname))
            {
                string element = URLHostList[i].ToString();
                if (element.Contains("1"))
                {
                    checkSafeInt = 1; //safe
                }
                else if (element.Contains("2"))
                {
                    checkSafeInt = 2; //suspicious
                }
                else if (element.Contains("3"))
                {
                    checkSafeInt = 3; //malicious
                }
                else
                    checkSafeInt = 0; //not checked
            }
        }
#endregion

        if (checkSafeInt == 3) //site is unsafe
        {
            oSession.Abort();
            Console.WriteLine("** Session Aborted");

            //update datagrid of failure
            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);
                Console.WriteLine("Add to DataObject");

            }));
        }
        else if (checkSafeInt == 2)//site may be compromised
        {
            //pause thread to ask to proceed
            MessageBoxResult result = MessageBox.Show(
                "This URL is potentially compromised, do you wish to proceed?",
                "SecureNet",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    {

                    }
                    //user assume is safe
                    break;
                case MessageBoxResult.No:
                    {
                        oSession.Abort();
                        Console.WriteLine("** Session Aborted");

                        //update datagrid of failure
                        dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
                        {
                            DataObject newDataObject = new DataObject()
                            {
                                A = oSession.id.ToString(),
                                B = oSession.url,
                                C = oSession.hostname,
                                D = oSession.fullUrl,
                                E = oSession.state.ToString()
                            };
                            DataObjects.Add(newDataObject);
                            dataGrid1.Items.Add(newDataObject);
                            Console.WriteLine("Add to DataObject");
                        }));
                        break;
                    }
            }
        }
        else //site is safe
        {
            //do nothing, proceed to after session
            return;
        }*/
        #endregion

        #region VirusTotal Scanning -> Not completed as unable to check if it works due to public API constraints
        public async void VirusTotalURLScan(string shortUrl, string hostname)
        {
            VirusTotal vt = new VirusTotal(ConfigurationManager.AppSettings["virusTotalAPIKey"].ToString());
            vt.UseTLS = true;
            UrlReport urlReport = await vt.GetUrlReport(shortUrl);

            bool hasUrlBeenScannedBefore = urlReport.ResponseCode == ReportResponseCode.Present;

            if (hasUrlBeenScannedBefore)
            {
                ReviewScan(urlReport);
            }
            else
            {
                UrlScanResult urlResult = await vt.ScanUrl(shortUrl);
                NewScan(urlResult, hostname);
            }
        }

        private static void NewScan(UrlScanResult scanResult, string hostname)
        {
            checkSafe(scanResult.VerboseMsg, hostname);
        }

        public static void checkSafe(string resultString, string hostname)
        {
            int numTrue = Regex.Matches(resultString, "True").Count;
            if (numTrue > 2 && numTrue < 10)
            {
                checkSafeInt = 2;
            }
            else if (numTrue > 10)
            {
                checkSafeInt = 3;
            }
            else
                checkSafeInt = 1;

            URLHostList.Add(new CheckedHostList(hostname, checkSafeInt));
        }

        public static void ReviewScan(UrlReport urlReport)
        {
            string allLines = null;

            if (urlReport.ResponseCode == ReportResponseCode.Present)
            {
                foreach (KeyValuePair<string, ScanEngine> scan in urlReport.Scans)
                {
                    string currentLine = string.Format("{0,-25} Detected: {1}", scan.Key, scan.Value.Detected);
                    allLines += currentLine + Environment.NewLine; // Adds to string, so it can be written to file later
                }
            }
        }
        #endregion

        #region VirusTotal URL check logic; not completed as unable to check if it works due to public API constraints; added to beforeRequest
        /*
        if (URLHostList.ToString().Contains(hostname))
        {
            URLHostList.
            checkSafeInt = URLHostList
        }

        VirusTotalURLScan(shortUrl,hostname);

        if (checkSafeInt==3) //site is unsafe
        {
            oSession.Abort();
            Console.WriteLine("** Session Aborted");

            //update datagrid of failure
            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);
            Console.WriteLine("Add to DataObject");

            }));
        }
        else if (checkSafeInt==2)//site may be compromised
        {
            //pause thread to ask to proceed
            MessageBoxResult result = MessageBox.Show(
                "This URL is potentially compromised, do you wish to proceed?", 
                "SecureNet", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Warning);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    //user assume is safe
                    break;
                case MessageBoxResult.No:
                    {
                        oSession.Abort();
                        Console.WriteLine("** Session Aborted");

                        //update datagrid of failure
                        dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
                        {
                            DataObject newDataObject = new DataObject()
                            {
                                A = oSession.id.ToString(),
                                B = oSession.url,
                                C = oSession.hostname,
                                D = oSession.fullUrl,
                                E = oSession.state.ToString()
                            };
                            DataObjects.Add(newDataObject);
                            dataGrid1.Items.Add(newDataObject);
                            Console.WriteLine("Add to DataObject");
                        }));
                        break;
                    }
            }
        }
        else //site is safe
        {
            //do nothing, proceed to after session
            return;
        }
        */
        #endregion

        //hardcoding of suspicious & malicious hosts
        public void AddtoHostList(string hostname)
        {
            if (hostname == "learn.nyp.edu.sg") //suspicious host
            {
                checkSafeInt = 2;
                URLHostList.Add(new CheckedHostList(hostname, checkSafeInt));
            }
            else if (hostname == "myportal.nyp.edu.sg") //malicious host
            {
                checkSafeInt = 3;
                URLHostList.Add(new CheckedHostList(hostname, checkSafeInt));
            }
            else //safe host
                checkSafeInt = 1;
            URLHostList.Add(new CheckedHostList(hostname, checkSafeInt));
        }

        public class CheckedHostList
        {
            private string hostname;
            private int checkSafeInt;

            public CheckedHostList(string hostname, int checkSafeInt)
            {
                this.hostname = hostname;
                this.checkSafeInt = checkSafeInt;
            }
        }

        #endregion


    }
}
