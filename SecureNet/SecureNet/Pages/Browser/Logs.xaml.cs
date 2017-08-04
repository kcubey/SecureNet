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

        public Logs()
        {
            Console.WriteLine("** Navigate success");
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
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

        /*
         * 
         * there is a need to start printing to the logs page
         * immediately from when the app starts
        private void DisplayUI()
        {
            FiddlerService.PrintResults(oSession);
        }

            */

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            string getLongUrl = oSession.url; //Mostly url+port
            string getUrl = null;

            int delimiterColon = getLongUrl.IndexOf(':');
            int delimiterSlash = getLongUrl.IndexOf('/');

            //Gets URL only
            if (delimiterColon != -1)
            {
                getUrl = getLongUrl.Substring(0, delimiterColon);
            }
            else if (delimiterSlash != -1)
            {
                getUrl = getLongUrl.Substring(0, delimiterSlash);
            }
            else
            {
                getUrl = getLongUrl;
            }

            Console.WriteLine("** Long Url: " + getLongUrl);
            Console.WriteLine("** Shortened url: " +getUrl);

            //EnterStanleyCode();

            bool stanley = true;

            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                dataGrid1.Items.Add(new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = "Checking" });

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
                    dataGrid1.Items.Add(new DataObject()
                    { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() });

                }));
            }
            
        }

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

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            dataGrid1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                dataGrid1.Items.Add(new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname , D=oSession.fullUrl, E= oSession.state.ToString()});
                
            }));

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
