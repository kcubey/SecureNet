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

namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for PassHome.xaml
    /// </summary>
    public partial class PassHome : Page
    {
        //StartUp
        public PassHome()
        {
            Console.WriteLine("navigate success");
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));

        }

        //Navigate to Login Credentials
        private void ButtonCreden_Click(object sender, RoutedEventArgs e)
        {
            navigation("/Pages/Manager/AddService.xaml");
        }

        //Navigate to Password Generator
        private void ButtonGen_Click(object sender, RoutedEventArgs e)
        {
            navigation("/Pages/Manager/PassGen.xaml");
        }

        //Navigate to Activity Log
        private void ButtonAlog_Click(object sender, RoutedEventArgs e)
        {
            navigation("/Pages/Manager/ActivityLog.xaml");
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            navigation("/Pages/Home.xaml");
        }

        //Navigation Method
        private void navigation(string url)
        {
            this.NavigationService.Navigate(new Uri(url, UriKind.Relative));
        }
    }
}
