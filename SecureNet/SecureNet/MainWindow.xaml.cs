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
         //   _mainFrame.NavigationService.GoBack();
           // _mainFrame.NavigationService.GoForward();
            //_mainFrame.NavigationService.Refresh();
        }

        /*
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _mainFrame.Navigate(new Page1());
        }

            private void OnClick(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate("" + new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative));
            Console.WriteLine("navigating...");
        }
        
*/
        private void OnClick(object sender, RoutedEventArgs e)
        {
            //MainFrame.Source = new Uri(((Button)sender).CommandParameter.ToString(), UriKind.Relative);
            this.MainFrame.Navigate(typeof(Page), ((Button)sender).CommandParameter.ToString());
        }

        
    }
}
