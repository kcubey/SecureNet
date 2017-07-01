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

        }

        //Navigation : Back Button
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));
        }

        private int getUserId()
        {
            return 1; 
        }
        private void ButtonSubmit_Click(object sender, RoutedEventArgs e)
        {


            string name = TextBoxName.Text;
            string url = TextBoxUrl.Text;
            string password = TextBoxPassword.Text;
            string username = TextBoxUsername.Text;
            string notes = TextBoxNotes.Text;


            if (!testEmpty(name) && !testEmpty(url) && !testEmpty(password) && !testEmpty(username))
            {

                //regex for password
                //regex for username
                //regex for notes
                //Check for duplicates
                //progressBar

                if (testName(name) && testUrl(url))
                {



                    Service service = new Service();

                    service.name = name;
                    service.url = url;
                    service.username = username;
                    service.password = password;

                    if (!testEmpty(notes))
                    {
                        service.notes = notes;
                    }
                    else
                    {
                        service.notes = null;
                    }
                    try
                    {
                    Service.genKeyIv(service, getUserId());
                    errorMsg.Content = "Successfully added!";
                    }
                    catch(Exception ex)
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

        //Test Empty Fields
        protected bool testEmpty(string testText)
        {
            return String.IsNullOrEmpty(testText) || String.IsNullOrWhiteSpace(testText);
        }

        //Text Max Chars
        protected bool testMax(string text, int max)
        {
            return text.Length > max;

        }

        protected bool validUrl(string uriName)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(uriName, UriKind.Absolute, out uriResult)
           && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        protected bool testName(string name)
        {

            Regex rgxName = new Regex("^[a-zA-Z0-9\x20]*$");

            bool result = rgxName.IsMatch(name);
            if (testMax(name, 50))
            {

                return false;

            }
            else if (!rgxName.IsMatch(name))
            {


                return false;
            }
            else
            {

                return true;
            }
        }

        protected bool testUrl(string url)
        {

            bool test = validUrl(url);


            if (testMax(url, 150))
            {

                return false;

            }
            else if (!validUrl(url))
            {


                return false;

            }
            else
            {

                return true;
            }
        }





    }
}
