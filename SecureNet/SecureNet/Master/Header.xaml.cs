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

namespace SecureNet.Master
{
    /// <summary>
    /// Interaction logic for Header.xaml
    /// </summary>
    public partial class Header : Page
    {
        public Header()
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
    }
}
