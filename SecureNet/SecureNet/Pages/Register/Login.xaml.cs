using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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

namespace SecureNet.Pages.Register
{
    /// <summary>
    /// Interaction logic for Login.xaml
    /// </summary>
    public partial class Login : Page
    {
        public Login()
        {
            InitializeComponent();
        }

        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {

            string uEmail;
            var userEmail = txtEmailLogin.Text;
            var masterPass = txtMasterLogin.Password.ToString();

            //gets method from Users
            int userId = Users.GetUserIdByEmailAndPassword(userEmail, masterPass);

            if (userId > 0)
            {
                Console.WriteLine("Successfully login.");
                this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));

                using (
                   SqlCommand cmd =
                       new SqlCommand("SELECT userID, userEmail FROM [Users] WHERE [userEmail] = @userEmail", GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        int uID = Convert.ToInt32(dr["userID"]);
                        uEmail = Convert.ToString(dr["userEmail"]);
                        //assigning userID to session
                        Application.Current.Properties["SessionID"] = uID;
                        //changing session to string
                        int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());
                        Console.WriteLine(mySession);
                    }
                }
            }


            else
            {
                MessageBoxResult result = MessageBox.Show("Incorrect email or password!", "Error");

            }
        }
    }
}