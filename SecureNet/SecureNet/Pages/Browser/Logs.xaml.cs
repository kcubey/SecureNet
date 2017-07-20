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
        }


        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("Redirect to " +redirectAdd);
        }

        

        public void FiddlerApplication_AfterSessionComplete(Session oSession)
        {
            listBox1.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox1.Items.Add(oSession.url);
            }));

            listBox2.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox2.Items.Add(oSession.hostname);
            }));

            listBox3.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox3.Items.Add(oSession.id);
            }));

            listBox4.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox4.Items.Add(oSession.isHTTPS);
            }));

            listBox5.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox5.Items.Add(oSession.LocalProcessID);
            }));

            listBox6.Dispatcher.Invoke(new UpdateUI(() =>
            {
                listBox6.Items.Add(oSession.port);
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
