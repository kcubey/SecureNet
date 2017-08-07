using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
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
    /// Interaction logic for NotVerified.xaml
    /// </summary>
    public partial class NotVerified : Page
    {
        public NotVerified()
        {
            InitializeComponent();
            txtDisplay.Text = "Hi! " + Application.Current.Properties["SessionEmail"].ToString() + "! Your account isn't verified yet.";
            txtEmailSend.Text = Application.Current.Properties["SessionEmail"].ToString();


        }

        public static SqlConnection GetConnection()
        {
            SqlConnection connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["SecureNetCon"].ConnectionString);
            connection.Open();
            return connection;
        }

        private void btnSendEmail_Click(object sender, RoutedEventArgs e)
        {
            string email = txtEmailSend.Text;
            using (SqlCommand cmd2 = new SqlCommand
                       ("UPDATE [Users] SET [codeGuid] = @codeGuid, [timeSavedGuid] = @timeSavedGuid where [userEmail] = @userEmail", GetConnection()))
            {
                DateTime currentTime = DateTime.Now;
                Guid newCode = System.Guid.NewGuid();
                cmd2.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                cmd2.Parameters.AddWithValue("@codeGuid", newCode);
                cmd2.Parameters.AddWithValue("@timeSavedGuid", currentTime);
                cmd2.ExecuteNonQuery();
                MessageBoxResult result = MessageBox.Show("New verification code sent. Please check your email including the spam/junk folder.", "Success");

                using (
                SqlCommand cmd3 =
                new SqlCommand("SELECT codeGuid FROM [Users] WHERE userEmail=@userEmail", GetConnection()))
                {
                    cmd3.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                    SqlDataReader dr2 = cmd3.ExecuteReader();
                    while (dr2.Read())
                    {
                        string newGuid = Convert.ToString(dr2["codeGuid"]);

                        try
                        {
                            MailMessage mail = new MailMessage();
                            SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                            mail.From = new MailAddress("securenetnyp@gmail.com");
                            mail.To.Add(txtEmailSend.Text);
                            mail.Subject = "Key in this verification code in the verification page. ";
                            mail.Body = "Your verification code is: " + newGuid;
                            SmtpServer.Port = 587;
                            SmtpServer.Credentials = new System.Net.NetworkCredential("securenetnyp@gmail.com", "Securen3t");
                            SmtpServer.EnableSsl = true;
                            SmtpServer.Send(mail);
                            Console.WriteLine("Email sent.");

                        }
                        catch (Exception ex)
                        {
                            //
                        }
                    }
                }
            }
        }

        private void btnSubmit_Click(object sender, RoutedEventArgs e)
        {
            string verifyText = txtVerifyEnter.Text;

            using (SqlCommand cmd = new SqlCommand
            ("SELECT codeGuid, timeSavedGuid FROM [Users] WHERE [userEmail] = @userEmail", GetConnection()))
            {
                cmd.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    string uVerifyGuid = Convert.ToString(dr["codeGuid"]);
                    string timeSavedGuid = Convert.ToString(dr["timeSavedGuid"]);

                    //get current time of computer and compare with time in database
                    DateTime currentTime = DateTime.Now;
                    currentTime.ToString();
                    DateTime codeTime = DateTime.Parse(timeSavedGuid);

                    TimeSpan span = currentTime.Subtract(codeTime);
                    int minutes = span.Minutes;


                    if (minutes > 5)
                    {
                        //code expired after 5 minutes
                        MessageBoxResult codeExpired = MessageBox.Show("Your verification code has expired or is wrong. Please re-send a new one.", "Error");
                        using (SqlCommand cmd2 = new SqlCommand
                        ("UPDATE [Users] SET [codeGuid] = @codeGuid, [timeSavedGuid] = @timeSavedGuid where [userEmail] = @userEmail", GetConnection()))
                        {
                            Guid newCode = System.Guid.NewGuid();
                            cmd2.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                            cmd2.Parameters.AddWithValue("@codeGuid", newCode);
                            cmd2.Parameters.AddWithValue("@timeSavedGuid", currentTime);
                            cmd2.ExecuteNonQuery();

                            using (
                            SqlCommand cmd3 =
                            new SqlCommand("SELECT codeGuid FROM [Users] WHERE userEmail=@userEmail", GetConnection()))
                            {
                                cmd3.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                                SqlDataReader dr2 = cmd3.ExecuteReader();
                                while (dr2.Read())
                                {
                                    string newGuid = Convert.ToString(dr["codeGuid"]);

                                    try
                                    {
                                        MailMessage mail = new MailMessage();
                                        SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                                        mail.From = new MailAddress("securenetnyp@gmail.com");
                                        mail.To.Add(txtEmailSend.Text);
                                        mail.Subject = "Key in this verification code in the verification page. ";
                                        mail.Body = "Your verification code is: " + newGuid;
                                        SmtpServer.Port = 587;
                                        SmtpServer.Credentials = new System.Net.NetworkCredential("securenetnyp@gmail.com", "Securen3t");
                                        SmtpServer.EnableSsl = true;
                                        SmtpServer.Send(mail);
                                        Console.WriteLine("Email sent.");
                                        MessageBoxResult result = MessageBox.Show("Email sent. Please check your email including the spam/junk folder.", "Error");

                                    }
                                    catch (Exception ex)
                                    {
                                        //
                                    }
                                }
                            }
                        }
                    }
                    else if (minutes < 5 && txtVerifyEnter.Text == uVerifyGuid)
                    {
                        MessageBoxResult verifiedSuccess = MessageBox.Show("Your account has been successfully verified. You may now log in.", "Success");
                        using (SqlCommand cmd4 = new SqlCommand
                        ("UPDATE [Users] SET [userVerified] = @userVerified where [userEmail] = @userEmail", GetConnection()))
                        {
                            string verifiedNum = "1";
                            cmd4.Parameters.AddWithValue("@userEmail", txtEmailSend.Text);
                            cmd4.Parameters.AddWithValue("@userVerified", verifiedNum);
                            cmd4.ExecuteNonQuery();
                        }
                            this.NavigationService.Navigate(new Uri("/Pages/Register/Login.xaml", UriKind.Relative));
                    }

                    else
                    {
                    }
                }
            }
        }
    }
}