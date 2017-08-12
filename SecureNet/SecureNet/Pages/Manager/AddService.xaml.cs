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
using System.Timers;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;


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
            Mouse.OverrideCursor = Cursors.Wait;
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
            int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());

            return mySession;
        }


        //StartUp Load
        private void startUp()
        {

            populateSelection();

            pgHeader.Content = "Login Credentials";

            saButt.Content = "Add";

            errorMsg.Content = null;

            Mouse.OverrideCursor = null;



        }


        //Combobox change
        private void selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selection.SelectedIndex != -1)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                string selectedService = selection.SelectedItem.ToString();

                List<Service> userServices = Service.retrieveRecords(getUserId());

                foreach (Service j in userServices)
                {
                    if (j.name == selectedService)
                    {
                       if (j.otp == 1)
                        {
                            bool otp = popup();

                           if (otp == true)
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
                                changeDisplay();
                            }
                            else
                            {
                                MessageBox.Show("Wrong OTP!");
                                selection.SelectedIndex = -1;
                            }
                        }
                       else
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
                            changeDisplay();
                        }

                    }
                }

                Service.logCommand(TextBoxName.Text, 2, null, getUserId());




            }

        }


        private void changeDisplay()
        {

            uneditable();
            svcForm.Visibility = Visibility.Visible;

            suButt.Visibility = Visibility.Visible;
            suButt.Content = "Update";

            dcButt.Visibility = Visibility.Visible;
            dcButt.Content = "Delete";

            errorMsg.Content = null;
            Mouse.OverrideCursor = null;

        }
        //Add/Submit
        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {
            string command = saButt.Content.ToString();

            if (command == "Submit")
            {
                Mouse.OverrideCursor = Cursors.Wait;
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
                    string serviceName = TextBoxName.Text;
                    Service.deleteService(serviceId);
                    Service.logCommand(serviceName, 5, null, getUserId());

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
                Mouse.OverrideCursor = Cursors.Wait;
                Add(0);

            }

        }

        private bool popup()
        {
            Mouse.OverrideCursor = null;
            OTP hello = new OTP();
            hello.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            hello.ShowDialog();

            if (hello.DialogResult == true)
            {
                return true;
            }
            else
            {
                return false;
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
                                
                                    Mouse.OverrideCursor = Cursors.Wait;
                                    Service.genKeyIv(service, getUserId(), -1);
                                    resetfields();
                                    Service.logCommand(service.name, 3, null, getUserId());
                                    errorMsg.Content = "Successfully added!";
                         
                            }
                            else
                            {
                                errorMsg.Content = "Error with Inputs. Please relook at the requirements";
                            }
                        }
                        else
                        {
                           
                                Mouse.OverrideCursor = Cursors.Wait;
                                int serviceId = Convert.ToInt32(TextBoxId.Text);
                                Service.genKeyIv(service, getUserId(), serviceId);
                                Service.logCommand(service.name, 4, null, getUserId());
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

            Mouse.OverrideCursor = null;
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
            string header = pgHeader.Content.ToString();
            if (header == "Add Service")
            {
                TextBoxName.IsReadOnly = false;
            }

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

        //Disable Copy & Cut Comm
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut)
            {
                e.Handled = true;
            }
        }

        //ShowPassword
        private void ShowPass_Click(object sender, RoutedEventArgs e)
        {
            string header = pgHeader.Content.ToString();
            if (header == "Update Service" || header == "Login Credentials")
            {

                Service.logCommand(TextBoxName.Text, 6, null, getUserId());
            }

            MessageBox.Show(TextBoxPassword.Password);
        }

        //Copy content
        private void Copyname_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            string serviceName = TextBoxName.Text;
            string type;
            string header = pgHeader.Content.ToString();
            if (header == "Update Service" || header == "Login Credentials")
            {
                if (button.Name == "Copyname")
                {
                    Clipboard.SetText(TextBoxName.Text);
                    type = "Service Name";
                }
                else if (button.Name == "Copyurl")
                {
                    Clipboard.SetText(TextBoxUrl.Text);
                    type = "URL";
                }
                else if (button.Name == "Copyusername")
                {
                    Clipboard.SetText(TextBoxUsername.Text);
                    type = "Username";
                }
                else if (button.Name == "Copypassword")
                {
                    Clipboard.SetText(TextBoxPassword.Password);
                    type = "Password";
                }
                else
                {
                    Clipboard.SetText(TextBoxNotes.Text);
                    type = "Notes";
                }

                Service.logCommand(serviceName, 1, type, getUserId());

            }


            MessageBox.Show("Copied Successfully!");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            bool otp = popup();

            if (otp == true)
            {
                this.NavigationService.Navigate(new Uri("/Pages/Manager/ManageOtp.xaml", UriKind.Relative));
            }
            else
            {
                MessageBox.Show("Wrong OTP!");
            }
        }
    }
}
