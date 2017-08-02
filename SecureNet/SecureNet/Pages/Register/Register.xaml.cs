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
using System.Security.Cryptography;
using System.IO;

namespace SecureNet.Pages.Register
{
    /// <summary>
    /// Interaction logic for Page1.xaml
    /// </summary>
    public partial class Page1 : Page
    {
        public Page1()
        {
            InitializeComponent();
        }

        //Connection
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmail.Text; //phone is encrypted in varbinary
            string masterPass = txtMasterPass.Text;
            string phone = txtPhone.Text;

            //checks if any textbox is empty before proceeding
            if (!string.IsNullOrEmpty(txtEmail.Text) || (!string.IsNullOrEmpty(txtMasterPass.Text) || (!string.IsNullOrEmpty(txtPhone.Text))))

            {
                SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
                con.Open();
                SqlCommand cmd = new SqlCommand("select * from Users where userEmail= @userEmail", con);
                cmd.Parameters.AddWithValue("@userEmail", txtEmail.Text);
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    MessageBoxResult errorEmail = MessageBox.Show("Email already exists. Please choose a different one.", "Error");
                }
                else
                {
                    bool result = Users.addUser(email, phone, masterPass);
                    this.NavigationService.Navigate(new Uri("/Pages/Register/Login.xaml", UriKind.Relative));
                    Console.WriteLine("Successfully registered.");
                }
              
            }

            else
            {
                MessageBoxResult errorRegistering = MessageBox.Show("Register error", "Error");
            }
        }
    }
}