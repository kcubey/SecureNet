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

namespace SecureNet.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : Page
    {
        public Home()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));

        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            string redirectAdd = ((Button)sender).CommandParameter.ToString();
            this.NavigationService.Navigate(new Uri(redirectAdd, UriKind.Relative));
            Console.WriteLine("redirecting now");
        }

        
        
/* from net
        void StartFiddler()
        {
            if (tbIgnoreResources.Checked)
                CaptureConfiguration.IgnoreResources = true;
            else
                CaptureConfiguration.IgnoreResources = false;

            string strProcId = txtProcessId.Text;
            if (strProcId.Contains('-'))
                strProcId = strProcId.Substring(strProcId.IndexOf('-') + 1).Trim();

            strProcId = strProcId.Trim();

            int procId = 0;
            if (!string.IsNullOrEmpty(strProcId))
            {
                if (!int.TryParse(strProcId, out procId))
                    procId = 0;
            }
            CaptureConfiguration.ProcessId = procId;
            CaptureConfiguration.CaptureDomain = txtCaptureDomain.Text;

            FiddlerApplication.AfterSessionComplete += FiddlerApplication_AfterSessionComplete;
            FiddlerApplication.Startup(8888, true, true, true);

            //Other events include BeforeRequest, BeforeResponse, RequestHeadersAvailable, ResponseHeadersAvailable and so on.
        }*/
    
    }
}
