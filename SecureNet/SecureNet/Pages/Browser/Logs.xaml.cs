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
            Console.WriteLine("navigate success");
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.BeforeRequest += FiddlerApplication_BeforeRequest;
            
        }

        

        public class DataObject
        {
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
            public string D { get; set; }
            public string E { get; set; }
        }

        private void FiddlerApplication_BeforeRequest(Session oSession)
        {
            oSession.bBufferResponse = true;
            oSession.utilDecodeResponse();
            Console.WriteLine(oSession.GetRequestBodyAsString());
            oSession.utilReplaceInResponse("google", "yahoo");

            dataGrid2.Dispatcher.Invoke(new UpdateUI(() =>
            {
                dataGrid2.Items.Add(new DataObject()
                { A = oSession.id.ToString(), B = oSession.url, C = oSession.hostname, D = oSession.fullUrl, E = oSession.state.ToString() });

            }));
        }
        


        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("Redirect to " +redirectAdd);
        }

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
