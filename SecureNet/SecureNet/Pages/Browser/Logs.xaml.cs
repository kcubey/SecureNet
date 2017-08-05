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

namespace SecureNet.Pages.Browser
{
    /// <summary>
    /// Interaction logic for Logs.xaml
    /// </summary>
    public partial class Logs : Page
    {
        delegate void UpdateUI();
        public static List<DataObject> DataObjects { get; set; }

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
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;

            //FiddlerApplication.BeforeReturningError += FiddlerApplication_BeforeReturningError;
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

        public void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string getLongUrl = oSession.url; //Mostly url+port
            string getUrl = null;

            int delimiterColon = getLongUrl.IndexOf(':');

            //Gets URL only
            if (delimiterColon != -1)
            {
                getUrl = getLongUrl.Substring(0, delimiterColon);
            }
            else
            {
                getUrl = getLongUrl;
            }

            Console.WriteLine("** Long Url: " + getLongUrl);
            Console.WriteLine("** Short url: " + getUrl);

            //EnterStanleyCode();

            bool stanley = true;

            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = "Checking" };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);
                Console.WriteLine("Add to DataObject");

            }));

            if (oSession.HostnameIs("www.yahoo.com"))
            {
                stanley = false;
            }

            if (stanley == false) //site is unsafe
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


        }

        #region stanley's
        /*
        public bool enterStanleyCode()
        {

            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                dataGrid1.Items.Add(new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = "Checking" });

            }));
            bool safe = false;

            if (safe == false) //not safe
            {
                return false;
            }

            else if (safe == true) //safe
            {
                return true;
            }
            else //errors
                return false;
        }
        */
        #endregion

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                DataObject newDataObject = new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() };
                DataObjects.Add(newDataObject);
                dataGrid1.Items.Add(newDataObject);

            }));

        }

        private void FiddlerApplication_BeforeReturningError(Session oSession)
        {
            if (oSession.bHasResponse)
            {
                string sTitle = "Unable to load page";
                string sOriginalMessage = oSession.GetResponseBodyAsString().Trim().Replace("[Fiddler] ", String.Empty);
                oSession.oResponse["Content-Type"] = "text/html; charset=utf-8";
                oSession.oResponse["Cache-Control"] = "max-age=0, must-revalidate";
                string sEnhancedError =
                  String.Format("<!doctype html><html><head><title>{0}</title>\r\n<style>" +
                  "body {{ background-color: #CCDDDD; font-family: sans-serif }}\r\npre {{ max-width:600px; white-space:pre-wrap;}}\r\n" +
                  "</style></head>\r\n<body><h1>MyProxy - Page Unavailable</h1>The specified resource could not be loaded.<br /><pre>{1}</pre></body></html>", sTitle, sOriginalMessage);

                oSession.utilSetResponseBody(sEnhancedError);
            }
        }

        public void ExportToFile()
        {
            System.IO.Directory.CreateDirectory("Traffic Logs");

            string currentTime = DateTime.Now.ToLocalTime().ToString();
            currentTime=currentTime.Replace('/', '-');
            currentTime=currentTime.Replace(':', '-');

            string saveFile = System.AppDomain.CurrentDomain.BaseDirectory.ToString()+ "Traffic Logs\\" + currentTime + ".csv";
            
            dataGrid1.SelectAllCells();
            dataGrid1.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dataGrid1);
            String resultat = (string)Clipboard.GetData(DataFormats.CommaSeparatedValue);
            //String result = (string)Clipboard.GetData(DataFormats.Text);
            dataGrid1.UnselectAllCells();
            System.IO.StreamWriter file = new System.IO.StreamWriter(saveFile);
            //file.WriteLine(resultat.Replace('	', ','));
            file.WriteLine(resultat);
            file.Close();
            Console.WriteLine("File exported");
    }



    /*
    private void FillDataGrid()
    {
        string ConString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        string CmdString = string.Empty;
        using (SqlConnection con = new SqlConnection(ConString))
        {
            CmdString = "SELECT emp_id, fname, lname, hire_date FROM Employee";
            SqlCommand cmd = new SqlCommand(CmdString, con);
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable("Employee");
            sda.Fill(dt);
            gridLogs.ItemsSource = dt.DefaultView;
        }
    }*/
}
}
