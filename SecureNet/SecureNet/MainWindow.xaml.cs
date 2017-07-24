using SecureNet.Pages;
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

namespace SecureNet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Window));
            StartFiddler();

            //         MainFrame.NavigationService.GoBack();
            //       MainFrame.NavigationService.GoForward();
            //     MainFrame.NavigationService.Refresh();
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
            //this.MainFrame.Navigate(typeof(Page), ((Button)sender).CommandParameter.ToString());
        }

        void StartFiddler()
        {
            FiddlerApplication.Startup(0, FiddlerCoreStartupFlags.Default);
            //FiddlerApplication.Startup(8877, true, true);
            Console.WriteLine("Fiddler Start");
        }

        protected override void OnClosed(EventArgs e)
        {
            Fiddler.FiddlerApplication.oProxy.Detach();
            Fiddler.FiddlerApplication.Shutdown();
            Console.WriteLine("Fiddler Closed");
        }


    }
}
