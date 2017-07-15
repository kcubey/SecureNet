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
using SecureNet.Classes;
using System.Data;

namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for ActivityLog.xaml
    /// </summary>
    public partial class ActivityLog : Page
    {
        string selectedIds = null;
        //StartUp
        public ActivityLog()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            fillTable();
        }


        //Navigation : Back Button
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));
        }

        private void fillTable()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            List<Passlog> logEntries = Passlog.retrieveLogs(getUserId());

            
            if (logEntries != null)
            {
                LogTable.ItemsSource = logEntries;
                
                errorMsg.Content = null;

            }
            else
            {
                LogTable.Visibility = Visibility.Collapsed;
              
                errorMsg.Content = "No entries yet";
            }

             Mouse.OverrideCursor = null;

        }

        private int getUserId()
        {
            return 1;
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {
            string command = Report.Content.ToString();

            if (command == "Report Suspicious Activity") { 
            LogTable.HeadersVisibility = DataGridHeadersVisibility.All;
            errorMsg.Content = "Click the small button on the left  to add into entries that indicate suspicious activity. After all entries has been selected, click submit and your data will be locked down. " +
                    "Note that it is irreversible. ";
            resultPassword.Visibility = Visibility.Visible;
            Report.Content = "Submit";
                selectedIds = null;
                resultPassword.Text = null;
            }
            else
            {
                LogTable.HeadersVisibility = DataGridHeadersVisibility.Column;
                errorMsg.Content = null;
                resultPassword.Visibility = Visibility.Collapsed;
                //Retreieve SelectedIds and Store into DB
                MessageBox.Show(selectedIds);
                Report.Content = "Report Suspicious Activity";
            }

        }

       

        private void LogTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            Passlog selectedLog = (Passlog)LogTable.SelectedItem;

            int logId = selectedLog.logId;
            DateTime logDateTime = selectedLog.logDateTime;
            string logDetails = selectedLog.logDetails;


            selectedIds += logId.ToString() + ";";

            StringBuilder builder = new StringBuilder();

            string logEntry = logDateTime.ToString() + "\t" + logDetails;
            builder.Append(logEntry);
            builder.Append(Environment.NewLine);
            resultPassword.Text += builder.ToString();


        }

        
    }
}
