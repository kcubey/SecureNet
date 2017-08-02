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

        //StartUp
        public ActivityLog()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            fillTable();
            List<int> selectedItems = new List<int>();
            Application.Current.Resources["logIdList"] = selectedItems;



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
            int mySession = int.Parse(Application.Current.Properties["SessionID"].ToString());
            return mySession;
        }

        private void Report_Click(object sender, RoutedEventArgs e)
        {


            string command = Report.Content.ToString();

            if (command == "Report Suspicious Activity")
            {
                LogTable.HeadersVisibility = DataGridHeadersVisibility.All;
                errorMsg.Content = "To add, click the left buttons of the suspicious entries from the above table to add into the bottom table. If you clicked the wrong entry, remove it by clicking the left button" +
                    "of the wrong entry from the bottom table   ";
                ReportTable.Items.Clear();
                Reporting.Visibility = Visibility.Visible;
                Report.Content = "Submit";


            }
            else
            {


                List<int> test = (List<int>)Application.Current.Resources["logIdList"];
                if (test.Count != 0)
                {
                    OTP hello = new OTP();
                    hello.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                    hello.ShowDialog();


                    if (hello.DialogResult == true)
                    {

                        Passlog.lockDown(test, getUserId());
                        LogTable.HeadersVisibility = DataGridHeadersVisibility.Column;
                        errorMsg.Content = null;
                        test.Clear();
                        Application.Current.Resources["logIdList"] = test;
                        Reporting.Visibility = Visibility.Collapsed;
                        ReportTable.Items.Clear();
                        Report.Content = "Report Suspicious Activity";
                      
                    }

                    else
                    {
                        MessageBox.Show("OTP not entered correctly");
                    }
                }
                else
                {
                    MessageBox.Show("Please select entries before submitting.");
                }

            }
            


        }



        private void LogTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {


            Passlog selectedLog = (Passlog)LogTable.SelectedItem;
            List<int> test = (List<int>)Application.Current.Resources["logIdList"];
            

            if (!test.Contains(selectedLog.logId))
            {
                ReportTable.Items.Add(selectedLog);
                test.Add(selectedLog.logId);
            }
            Application.Current.Resources["logIdList"] = test;



        }

        private void ReportTable_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (ReportTable.SelectedItems.Count > 0)
            {
                Passlog selectedLog = (Passlog)ReportTable.SelectedItem;
                int logId = selectedLog.logId;
                List<int> test = (List<int>)Application.Current.Resources["logIdList"];
                test.Remove(logId);
                ReportTable.Items.Remove(ReportTable.SelectedItem);

                Application.Current.Resources["logIdList"] = test;

            }
                

        }
    }
}
