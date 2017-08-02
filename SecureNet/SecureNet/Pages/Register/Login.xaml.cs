using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
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

        // AES256 Decryption for Users Info
        public static byte[] DecryptAES256(byte[] content, string key, string iv)
        {
            byte[] encryptedtext = content;

            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            aes.BlockSize = 128;
            aes.KeySize = 256;
            aes.Key = System.Text.ASCIIEncoding.ASCII.GetBytes(key);
            aes.IV = System.Text.ASCIIEncoding.ASCII.GetBytes(iv);
            aes.Padding = PaddingMode.PKCS7;
            aes.Mode = CipherMode.CBC;

            ICryptoTransform crypto = aes.CreateDecryptor(aes.Key, aes.IV);
            byte[] plaintext = crypto.TransformFinalBlock(encryptedtext, 0, encryptedtext.Length);
            return plaintext;
        }
    
    public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string phoneNumber;
            string uEmail;
            var userEmail = txtEmailLogin.Text;
            var masterPass = txtMasterLogin.Password.ToString();

            //gets method from Users
            int userId = Users.GetUserIdByEmailAndPassword(userEmail, masterPass);

            if (userId > 0)
            {
                Console.WriteLine("Successfully login.");
               

                using (
                   SqlCommand cmd =
                       new SqlCommand("SELECT userID, userEmail, userPhone FROM [Users] WHERE [userEmail] = @userEmail", GetConnection()))
                {
                    cmd.Parameters.AddWithValue("@userEmail", userEmail);
                    SqlDataReader dr = cmd.ExecuteReader();

                    while (dr.Read())
                    {
                        using (
                        SqlCommand cmd2 = new SqlCommand("SELECT * FROM [UserValue] WHERE userEmail=@userEmail", GetConnection()))
                        {
                            cmd2.Parameters.AddWithValue("@userEmail", userEmail);

                            SqlDataReader dr2 = cmd2.ExecuteReader();
                            while (dr2.Read())
                            {
                                string userKey = Convert.ToString(dr2["userKey"]);
                                string userIV = Convert.ToString(dr2["userIV"]);
                                byte[] decryptPhone = DecryptAES256((byte[])dr["userPhone"], userKey, userIV);
                                string decryptedPhone = System.Text.Encoding.UTF8.GetString(decryptPhone);
                                Console.WriteLine(decryptedPhone);

                            }

                            int uID = Convert.ToInt32(dr["userID"]);
                            uEmail = Convert.ToString(dr["userEmail"]);

                            //assigning userID to session
                            Application.Current.Properties["SessionID"] = uID;
                            //changing session to string
                            int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());
                            Console.WriteLine(mySession);
                            this.NavigationService.Navigate(new Uri("/Pages/Register/NotVerified.xaml", UriKind.Relative));
                        }
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