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
using System.Text.RegularExpressions;
using System.IO;
using SecureNet.Classes;
using System.Data;
using System.Data.SqlClient;


namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for AddService.xaml
    /// </summary>
    public partial class AddService : Page
    {

        public AddService()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            startUp();

        }

        //Navigataion
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));
        }

        //Retrieve Session userID
        private int getUserId()
        {
            return 1;
        }


        //StartUp Load
        private void startUp()
        {

            pgHeader.Content = "Login Credentials";

            populateSelection();

            saButt.Content = "Add";

            errorMsg.Content = null;

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
                        TextBoxId.Text = j.serviceId.ToString();
                        TextBoxName.Text = j.name;
                        TextBoxUrl.Text = j.url;
                        TextBoxUsername.Text = j.username;
                        TextBoxPassword.Password = j.password;
                        if (j.notes == null)
                        {
                            TextBoxNotes.Text = null;
                        }
                        else
                        {
                            TextBoxNotes.Text = j.notes;
                        }
                    }
                }

                uneditable();
                svcForm.Visibility = Visibility.Visible;

                suButt.Visibility = Visibility.Visible;
                suButt.Content = "Update";

                dcButt.Visibility = Visibility.Visible;
                dcButt.Content = "Delete";

                errorMsg.Content = null;
            }

        }

        //Add/Submit
        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            string command = saButt.Content.ToString();

            if (command == "Submit")
            {
                Add(1);


            }
            else
            {
                pgHeader.Content = "Add Service";

                selection.Visibility = Visibility.Collapsed;

                Req.Visibility = Visibility.Visible;

                editable();
                resetfields();
                svcForm.Visibility = Visibility.Visible;

                saButt.Content = "Submit";

                dcButt.Visibility = Visibility.Visible;
                dcButt.Content = "Cancel";

                suButt.Visibility = Visibility.Collapsed;

                errorMsg.Content = null;

            }

        }

        //Delete/Cancel
        private void dcButt_Click(object sender, RoutedEventArgs e)
        {
            string command = dcButt.Content.ToString();
            if (command == "Cancel")
            {
                pgHeader.Content = "Login Credentials";

                selection.Visibility = Visibility.Visible;
                selection.SelectedIndex = -1;

                Req.Visibility = Visibility.Collapsed;

                svcForm.Visibility = Visibility.Collapsed;
                resetfields();

                saButt.Content = "Add";
                saButt.Visibility = Visibility.Visible;

                suButt.Visibility = Visibility.Collapsed;

                dcButt.Content = "Delete";
                dcButt.Visibility = Visibility.Collapsed;

                errorMsg.Content = null;


            }
            else
            {
                int serviceId = Convert.ToInt32(TextBoxId.Text);

                Service.deleteService(serviceId);

                selection.SelectedIndex = -1;
                populateSelection();

                svcForm.Visibility = Visibility.Collapsed;
                resetfields();

                suButt.Visibility = Visibility.Collapsed;

                dcButt.Visibility = Visibility.Collapsed;

                errorMsg.Content = "Successfully deleted record";


            }
        }

        //Update/Submit
        private void uButt_Click(object sender, RoutedEventArgs e)
        {
            string command = suButt.Content.ToString();

            if (command == "Update")
            {
                pgHeader.Content = "Update Service";

                selection.Visibility = Visibility.Collapsed;

                Req.Visibility = Visibility.Visible;

                editable();

                saButt.Visibility = Visibility.Collapsed;

                suButt.Content = "Submit";

                dcButt.Content = "Cancel";

                Req.Visibility = Visibility.Visible;


            }
            else
            {

                Add(0);
            }

        }


        //Add/Update Service
        private void Add(int command)
        {
            string name = TextBoxName.Text;
            string url = TextBoxUrl.Text;
            string password = TextBoxPassword.Password.ToString();
            string username = TextBoxUsername.Text;
            string notes = TextBoxNotes.Text;


            if (!Service.testEmpty(name) && !Service.testEmpty(url) &&
                !Service.testEmpty(password) && !Service.testEmpty(username))
            {

                //regex for password
                //regex for username
                //regex for notes
                //Check for duplicates

                if (Service.testName(name) && Service.testUrl(url))
                {



                    Service service = new Service();

                    service.name = name;
                    service.url = url;
                    service.username = username;
                    service.password = password;

                    if (!Service.testEmpty(notes))
                    {
                        service.notes = notes;
                    }
                    else
                    {
                        service.notes = null;
                    }
                    try
                    {


                        if (command == 1)
                        {
                            if (!sameName(name))
                            {
                                Service.genKeyIv(service, getUserId(), -1);
                                resetfields();
                                errorMsg.Content = "Successfully added!";
                            }
                            else
                            {
                                errorMsg.Content = "Error with Inputs. Please relook at the requirements";
                            }
                        }
                        else
                        {
                            int serviceId = Convert.ToInt32(TextBoxId.Text);
                            Service.genKeyIv(service, getUserId(), serviceId);
                            errorMsg.Content = "Successfully updated!";

                        }
                        populateSelection();
                    }
                    catch (Exception ex)
                    {
                        errorMsg.Content = "Operation Error.Contact Tech Support.";
                    }
                }

                else
                {
                    errorMsg.Content = "Error with Inputs. Please relook at the requirements";
                }
            }
            else
            {
                errorMsg.Content = "Service Name, URL, Username and Password are compulsory fields.";

            }
        }

        //Populate ComboBox
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

        //Reset TextBoxes to null
        private void resetfields()
        {
            TextBoxName.Text = null;
            TextBoxUrl.Text = null;
            TextBoxNotes.Text = null;
            TextBoxPassword.Password = null;
            TextBoxUsername.Text = null;
        }

        //TextBox Uneditable
        private void uneditable()
        {
            TextBoxName.IsReadOnly = true;
            TextBoxUrl.IsReadOnly = true;
            TextBoxNotes.IsReadOnly = true;
            TextBoxPassword.IsEnabled = false;
            TextBoxUsername.IsReadOnly = true;
        }

        //TextBox editable
        private void editable()
        {
            TextBoxName.IsReadOnly = false;
            TextBoxUrl.IsReadOnly = false;
            TextBoxNotes.IsReadOnly = false;
            TextBoxPassword.IsEnabled = true;
            TextBoxUsername.IsReadOnly = false;
        }

        //Name Validation
        private bool sameName(string name)
        {
            List<Service> userServices = Service.retrieveRecords(getUserId());

            if (userServices != null)
            {
                foreach (Service e in userServices)
                {
                    string test = e.name;
                    string testwo = name;
                    bool result = string.Equals(name, e.name, StringComparison.OrdinalIgnoreCase);

                    if (result == true)
                    {
                        return true;

                    }

                }
            }
            return false;
        }

    }
}
