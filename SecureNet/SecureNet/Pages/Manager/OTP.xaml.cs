using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TOTP;
namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for OTP.xaml
    /// </summary>
    public partial class OTP : Window
    {
      
        public OTP()
        {
            InitializeComponent();
            
        }

 
        private void otpButt_Click(object sender, RoutedEventArgs e)
        {
            string otp = Totp.newOTP();

           
           
            if(TextOTP.Text == otp)
            {

                DialogResult = true;

                
            }
          
            else
            {
                DialogResult = false;
            }
        }
    }
}
