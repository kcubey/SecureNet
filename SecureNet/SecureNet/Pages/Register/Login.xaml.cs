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
using SecureNet.Classes;

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
            //string phoneNumber;
            string uEmail;
            var userEmail = txtEmailLogin.Text;
            var masterPass = txtMasterLogin.Password.ToString();
            int attempts;
            string timeLocked;
            string isVerified;

            //gets method from Users
            int userId = Users.GetUserIdByEmailAndPassword(userEmail, masterPass);

            if (userId > 0)
            {
                Console.WriteLine("Successfully login.");

                PrivateKey.masterpass = masterPass;

                using (
                   SqlCommand cmd =
                       new SqlCommand("SELECT userID, userEmail, userPhone, userVerified, lockedAttempts, timeLockedOut FROM [Users] WHERE [userEmail] = @userEmail", GetConnection()))
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
                                Application.Current.Properties["SessionPhone"] = decryptedPhone;
                            }

                            int uID = Convert.ToInt32(dr["userID"]);
                            uEmail = Convert.ToString(dr["userEmail"]);
                            attempts = Convert.ToInt32(dr["lockedAttempts"]);
                            timeLocked = Convert.ToString(dr["timeLockedOut"]);
                            isVerified = Convert.ToString(dr["userVerified"]);

                            //get current time of computer and compare with time in database
                            DateTime currentTime = DateTime.Now;
                            currentTime.ToString();
                            DateTime timeLockedOut = DateTime.Parse(timeLocked);

                            //getting the minutes component from total time
                            TimeSpan span = currentTime.Subtract(timeLockedOut);
                            //assigning variable = minutes to the minutes component
                            int minutes = span.Minutes;
                            
                            if (attempts != 3 && isVerified == "0")
                            {
                                //assigning userID to session
                                Application.Current.Properties["SessionID"] = uID;
                                //assining uEmail to session to display in notverified page
                                Application.Current.Properties["SessionEmail"] = uEmail;                              
                                //changing session to string
                                int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());
                                Console.WriteLine(mySession);
                                this.NavigationService.Navigate(new Uri("/Pages/Register/NotVerified.xaml", UriKind.Relative));
                            }

                            else if (attempts !=3 && isVerified == "1")
                            {
                                //assigning userID to session
                                Application.Current.Properties["SessionID"] = uID;
                                //assining uEmail to session to display in notverified page
                                Application.Current.Properties["SessionEmail"] = uEmail;                                                           
                                //changing session to string
                                int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());
                                Console.WriteLine(mySession);
                                PrivateKey privKey = new PrivateKey();
                                privKey.genPrivKey(mySession);
                                TOTP.Totp.encryptSeed(mySession);
                                using (System.Net.WebClient client = new System.Net.WebClient())
                                {
                                    try
                                    {
                                        string username = "securenetnyp5@outlook.com";
                                        string password = "12345";
                                        //string txtMessage = "hello";

                                        using (SqlCommand cmd3 = new SqlCommand
                                            ("UPDATE [Users] SET [otpCode] = @otpCode, [OTPTime] = @OTPTime where [userEmail] = @userEmail", GetConnection()))
                                        {
                                            DateTime currentOTPTime = DateTime.Now;

                                            Random OTPCode = new Random();
                                            //ensures always a 6 digit number
                                            String r = OTPCode.Next(0, 1000000).ToString("D6");

                                            cmd3.Parameters.AddWithValue("@userEmail", userEmail);
                                            cmd3.Parameters.AddWithValue("@otpCode", r);
                                            cmd3.Parameters.AddWithValue("@OTPTime", currentOTPTime);
                                            cmd3.ExecuteNonQuery();

                                            MessageBoxResult otpCode = MessageBox.Show("OTP Code Sent", "Success");

                                            using (
                                            SqlCommand cmd4 = new SqlCommand("SELECT otpCode FROM [Users] WHERE userEmail=@userEmail", GetConnection()))
                                            {
                                                cmd4.Parameters.AddWithValue("@userEmail", userEmail);
                                                SqlDataReader dr3 = cmd4.ExecuteReader();
                                                while (dr3.Read())
                                                {
                                                    string newOTP = Convert.ToString(dr3["otpCode"]);

                                                    // Build the URL request for sending SMS.
                                                    string url = "http://smsc.vianett.no/v3/send.ashx?" +
                                                    "src=" + Application.Current.Properties["SessionPhone"].ToString() + "&" +
                                                    "dst=" + Application.Current.Properties["SessionPhone"].ToString() + "&" +
                                                    "msg=" + System.Web.HttpUtility.UrlEncode("Your OTP code for SecureNet is: "+ newOTP, System.Text.Encoding.GetEncoding("ISO-8859-1")) + "&" +
                                                    "username=" + System.Web.HttpUtility.UrlEncode(username) + "&"
                                                    + "password=" + System.Web.HttpUtility.UrlEncode(password);

                                                    string result = client.DownloadString(url);
                                                    if (result.Contains("OK"))
                                                    {
                                                        MessageBoxResult resultSend = MessageBox.Show("Please check your phone for the OTP Code.");
                                                    }
                                                    else
                                                    {
                                                        MessageBoxResult resultFail = MessageBox.Show("OTP code didn't successfully sent.");
                                                    }

                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)

                                    {

                                    }
                                }
                                this.NavigationService.Navigate(new Uri("/Pages/Register/PhoneOTP.xaml", UriKind.Relative));
                            }
                            //checks if account is already locked but user has waited for more than 30 minutes
                            else if (attempts >= 3 && minutes >= 10)
                            {
                                int resetLocked = 0;
                                using (SqlCommand cmd3 = new SqlCommand
                                     ("UPDATE [Users] SET [lockedAttempts] = @lockedAttempts where [userEmail] = @userEmail", GetConnection()))
                                {
                                    cmd3.Parameters.AddWithValue("@userEmail", userEmail);
                                    cmd3.Parameters.AddWithValue("@lockedAttempts", resetLocked);
                                    cmd3.ExecuteNonQuery();
                                }
                            }

                            //checks if account is already locked and time locked out is less than 10 minutes since last attempt
                            else if (attempts >= 3)
                            {
                                MessageBoxResult result = MessageBox.Show("Your account is locked. Please wait 10 minutes before you can log in again.", "Error");
                            }
                          
                        }
                    }
                }
            }
            else
            {
                MessageBoxResult result = MessageBox.Show("Incorrect email or password!", "Error");
                using (
                     SqlCommand cmd4 =
                         new SqlCommand("SELECT lockedAttempts FROM [Users] WHERE [userEmail]=@userEmail", GetConnection()))
                {
                    cmd4.Parameters.AddWithValue("@userEmail", userEmail);
                    SqlDataReader dr = cmd4.ExecuteReader();
                    while (dr.Read())
                    {
                        int dbLockedAttempts = Convert.ToInt32(dr["lockedAttempts"]);
                        if (dbLockedAttempts != 3)
                        {
                            //count + 1 for locked attempts
                            dbLockedAttempts++;
                            using (SqlCommand cmd5 = new SqlCommand
                                ("UPDATE [Users] SET [lockedAttempts] = @lockedAttempts where [userEmail] = @userEmail", GetConnection()))
                            {
                                cmd5.Parameters.AddWithValue("@userEmail", userEmail);
                                cmd5.Parameters.AddWithValue("@lockedAttempts", dbLockedAttempts);
                                cmd5.ExecuteNonQuery();

                                //update locked out time of attempted login to compare when user tries to sign in again
                                using (SqlCommand cmd6 = new SqlCommand
                                  ("UPDATE [Users] SET [timeLockedOut] = @timeLockedOut where [userEmail] = @userEmail", GetConnection()))
                                {
                                    DateTime dateLocked = DateTime.Now;
                                    dateLocked.ToString();
                                    cmd6.Parameters.AddWithValue("@userEmail", userEmail);
                                    cmd6.Parameters.AddWithValue("@timeLockedOut", dateLocked);
                                    cmd6.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
            }
        }

        private void btnRegisterLink_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Register/Register.xaml", UriKind.Relative));
        }
    }

}