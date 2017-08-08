using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
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
    /// Interaction logic for PhoneOTP.xaml
    /// </summary>
    public partial class PhoneOTP : Page
    {
        public PhoneOTP()
        {
            InitializeComponent();
            txtPhoneNumber.Text = Application.Current.Properties["SessionPhone"].ToString();
        }
        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private void btnSendOTP_Click(object sender, RoutedEventArgs e)
        {
            using (SqlCommand cmd3 = new SqlCommand
             ("UPDATE [Users] SET [OTPCode] = @OTPCode, [OTPTime] = @OTPTime where [userEmail] = @userEmail", GetConnection()))
            {
                DateTime currentOTPTime = DateTime.Now;
                Random OTPCode = new Random();
                //ensures always a 6 digit number
                String newOTP = OTPCode.Next(0, 1000000).ToString("D6");

                cmd3.Parameters.AddWithValue("@userEmail", Application.Current.Properties["SessionEmail"]);
                cmd3.Parameters.AddWithValue("@OTPCode", newOTP);
                cmd3.Parameters.AddWithValue("@OTPTime", currentOTPTime);
                cmd3.ExecuteNonQuery();

                MessageBoxResult otpCode = MessageBox.Show("OTP Code Sent. Please check your phone.", "Success");

                using (
                SqlCommand cmd4 = new SqlCommand("SELECT OTPCode FROM [Users] WHERE userEmail=@userEmail", GetConnection()))
                {
                    cmd4.Parameters.AddWithValue("@userEmail", Application.Current.Properties["SessionEmail"]);
                    SqlDataReader dr3 = cmd4.ExecuteReader();
                    while (dr3.Read())
                    {
                        using (System.Net.WebClient client = new System.Net.WebClient())
                        {
                            try
                            {
                                string username = "securenetnyp6@outlook.com";
                                string password = "7082v";
                                //string txtMessage = "hello";
                                string updatedOTP = Convert.ToString(dr3["OTPCode"]);

                                // Build the URL request for sending SMS.
                                string url = "http://smsc.vianett.no/v3/send.ashx?" +
                                "src=" + Application.Current.Properties["SessionPhone"].ToString() + "&" +
                                "dst=" + Application.Current.Properties["SessionPhone"].ToString() + "&" +
                                "msg=" + System.Web.HttpUtility.UrlEncode(newOTP, System.Text.Encoding.GetEncoding("ISO-8859-1")) + "&" +
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
                            catch (Exception ex)

                            {

                            }
                        }
                    }

                }
            }
        }



        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string verifyText = txtPin.Text;
            using (SqlCommand cmd = new SqlCommand
            ("SELECT otpCode, otpTime FROM [Users] WHERE [userEmail] = @userEmail", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@userEmail", Application.Current.Properties["SessionEmail"].ToString());
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string otpCode = Convert.ToString(dr["otpCode"]);
                    string timeSavedOTP = Convert.ToString(dr["otpTime"]);

                    Console.WriteLine(otpCode);
                    otpCode = otpCode.Trim();

                    //get current time of computer and compare with time in database
                    DateTime currentTime = DateTime.Now;
                    currentTime.ToString();
                    DateTime codeTime = DateTime.Parse(timeSavedOTP);

                    TimeSpan span = currentTime.Subtract(codeTime);
                    Console.WriteLine(span);
                    int minutes = span.Minutes;
                    int seconds = span.Seconds;


                    bool test = minutes < 2 && otpCode == verifyText;
                    if (minutes < 2 && otpCode == verifyText)
                    {
                        MessageBoxResult verifiedSuccess = MessageBox.Show("Success!", "Success");
                        
                        this.NavigationService.Navigate(new Uri("/Pages/Home.xaml", UriKind.Relative));

                        MessageBoxResult codeExpired = MessageBox.Show("Successful!", "Error");
                        using (SqlCommand cmd5 = new SqlCommand
                        ("UPDATE [Users] SET [OTPCode] = @OTPCode where [userEmail] = @userEmail", GetConnection()))
                        {
                            Random OTPCode = new Random();
                            //ensures always a 6 digit number
                            String r = OTPCode.Next(0, 1000000).ToString("D6");

                            cmd5.Parameters.AddWithValue("@userEmail", Application.Current.Properties["SessionEmail"]);
                            cmd5.Parameters.AddWithValue("@OTPCode", r);
                            cmd5.ExecuteNonQuery();

                        }

                    }
                    else if (minutes > 2)
                    {
                        //code expired after 15 minutes
                        MessageBoxResult codeExpired = MessageBox.Show("Your verification code has expired or is wrong. Please re-send a new one.", "Error");
                        using (SqlCommand cmd2 = new SqlCommand
                        ("UPDATE [Users] SET [OTPCode] = @OTPCode where [userEmail] = @userEmail", GetConnection()))
                        {
                            Random OTPCode = new Random();
                            //ensures always a 6 digit number
                            String r = OTPCode.Next(0, 1000000).ToString("D6");

                            cmd2.Parameters.AddWithValue("@userEmail", Application.Current.Properties["SessionEmail"]);
                            cmd2.Parameters.AddWithValue("@OTPCode", r);
                            cmd2.ExecuteNonQuery();

                        }
                    }
                    else
                    {
                        MessageBoxResult resultFail = MessageBox.Show("Failed to verify!", "Error");
                    }
                }
            }
        }
    }
}