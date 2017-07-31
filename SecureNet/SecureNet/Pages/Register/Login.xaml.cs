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

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var userEmail = txtEmailLogin.Text;
            var masterPass = txtMasterLogin.Password.ToString();

            //gets method from UserDatabase
            int userId = Users.GetUserIdByEmailAndPassword(userEmail, masterPass);

            if (userId > 0)
            {
                Console.WriteLine("Successss");
            }
            else
            {
                Console.WriteLine("bad");
            }
        }
    }
}
