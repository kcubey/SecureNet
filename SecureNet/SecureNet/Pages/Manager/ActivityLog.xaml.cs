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
    /// Interaction logic for ActivityLog.xaml
    /// </summary>
    public partial class ActivityLog : Page
    {

        //StartUp
        public ActivityLog()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
        }


        //Navigation : Back Button
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));
        }
    }
}
