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
using SecureNet.Classes;

namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for ManageOtp.xaml
    /// </summary>
    public partial class ManageOtp : Page
    {
        private int serviceId;
        public ManageOtp()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            populateSelection();
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/AddService.xaml", UriKind.Relative));
        }

        //Retrieve Session userID
        private int getUserId()
        {
            int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());

            return mySession;
        }

        private void populateSelection()
        {

            List<Service> userServices = Service.retrieveRecords(getUserId());

            if (userServices != null)
            {
                selection.Items.Clear();
                foreach (Service e in userServices)
                {
                    selection.Items.Add(e.name);
                }
            }
        }

        //Combobox change
        private void selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selection.SelectedIndex != -1)
            {

                string selectedService = selection.SelectedItem.ToString();

                List<Service> userServices = Service.retrieveRecords(getUserId());

                foreach (Service j in userServices)
                {
                    if (j.name == selectedService)
                    {
                        serviceId = j.serviceId;
                        if(j.otp == 1)
                        {
                            chkbox.IsChecked = true;
                        }
                        else
                        {
                            chkbox.IsChecked = false;
                        }
                    }
                }

                chkbox.Visibility = Visibility.Visible;
                sbutt.Visibility = Visibility.Visible;

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            int updatevalue;
           

            if (chkbox.IsChecked == true)
            {
                updatevalue = 1;
            }
            else{
                updatevalue = 0;
            }

            Service.updateOtp(serviceId, updatevalue);
            MessageBox.Show("Successfully updated!");
        }
    }
}
