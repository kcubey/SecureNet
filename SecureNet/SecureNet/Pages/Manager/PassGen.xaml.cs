﻿using System;
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
using System.Security.Cryptography;

namespace SecureNet.Pages.Manager
{
    /// <summary>
    /// Interaction logic for PassGen.xaml
    /// </summary>
    public partial class PassGen : Page
    {
        //Password Characters
        private static string PASSWORD_CHARS_LCASE = "abcdefgijkmnopqrstwxyz";
        private static string PASSWORD_CHARS_UCASE = "ABCDEFGHJKLMNPQRSTWXYZ";
        private static string PASSWORD_CHARS_NUMERIC = "23456789";



        //StartUp
        public PassGen()
        {
            InitializeComponent();
            Style = (Style)FindResource(typeof(Page));
            MainVisible();
            PopulateCheckbox();

        }

        //Navigation : Back Button
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new Uri("/Pages/Manager/PassHome.xaml", UriKind.Relative));
        }


        //Generate Password
        private string Generate(int length, string finalchars)
        {

            char[] lcase = PASSWORD_CHARS_LCASE.ToCharArray();
            char[] ucase = PASSWORD_CHARS_UCASE.ToCharArray();
            char[] numeric = PASSWORD_CHARS_NUMERIC.ToCharArray();

            List<char[]> arrayList = new List<char[]>();

            if (chkboxLC.IsChecked == true)
                arrayList.Add(lcase);

            if (chkboxUC.IsChecked == true)
                arrayList.Add(ucase);

            if (chkboxNum.IsChecked == true)
                arrayList.Add(numeric);

            if (finalchars != null)
            {
                char[] special = finalchars.ToCharArray();

                if (chkboxSC.IsChecked == true)
                    arrayList.Add(special);
            }

            // Track the number of unused characters in each character group
            int[] charsLeftInGroup = new int[arrayList.Count];

            // Initially, all characters in each group are not used.
            for (int i = 0; i < charsLeftInGroup.Length; i++)
                charsLeftInGroup[i] = arrayList[i].Length;



            // Use this array to track (iterate through) unused character groups.
            int[] leftGroupsOrder = new int[arrayList.Count];

            // Initially, all character groups are not used.
            for (int i = 0; i < leftGroupsOrder.Length; i++)
                leftGroupsOrder[i] = i;



            /*Because we cannot use the default randomizer, which is based on the
            current time (it will produce the same "random" number within a
            second), we will use a random number generator to seed the
            randomizer.*/
            // Use a 4-byte array to fill it with random bytes and convert it then to an integer value.
            byte[] randomBytes = new byte[4];
            // Generate 4 random bytes.
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(randomBytes);
            // Convert 4 bytes into a 32-bit integer value.
            int seed = BitConverter.ToInt32(randomBytes, 0);
            // Now, this is real randomization.
            Random random = new Random(seed);


            // This array will hold password characters.
            char[] password = new char[length];


            // Index of the next character to be added to password.
            int nextCharIdx;
            // Index of the next character group to be processed.
            int nextGroupIdx;
            // Index which will be used to track not processed character groups.
            int nextLeftGroupsOrderIdx;
            // Index of the last non-processed character in a group.
            int lastCharIdx;
            // Index of the last non-processed group.
            int lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;



            // Generate password characters one at a time.
            for (int i = 0; i < password.Length; i++)
            {
                /* If only one character group remained unprocessed, process it;
                   otherwise, pick a random character group from the unprocessed
                   group list. To allow a special character to appear in the
                   first position, increment the second parameter of the Next
                   function call by one, i.e. lastLeftGroupsOrderIdx + 1.*/
                if (lastLeftGroupsOrderIdx == 0)
                    nextLeftGroupsOrderIdx = 0;
                else
                    nextLeftGroupsOrderIdx = random.Next(0, lastLeftGroupsOrderIdx);

                // Get the actual index of the character group, from which we will
                // pick the next character.
                nextGroupIdx = leftGroupsOrder[nextLeftGroupsOrderIdx];
                // Get the index of the last unprocessed characters in this group.
                lastCharIdx = charsLeftInGroup[nextGroupIdx] - 1;

                // If only one unprocessed character is left, pick it; otherwise,
                // get a random character from the unused character list.
                if (lastCharIdx == 0)
                    nextCharIdx = 0;
                else
                    nextCharIdx = random.Next(0, lastCharIdx + 1);

                // Add this character to the password.
                password[i] = arrayList[nextGroupIdx][nextCharIdx];

                // If we processed the last character in this group, start over.
                if (lastCharIdx == 0)
                    charsLeftInGroup[nextGroupIdx] = arrayList[nextGroupIdx].Length;


                // There are more unprocessed characters left.
                else
                {
                    /*Swap processed character with the last unprocessed character
                      so that we don't pick it until we process all characters in
                      this group.*/
                    if (lastCharIdx != nextCharIdx)
                    {
                        char temp = arrayList[nextGroupIdx][lastCharIdx];
                        arrayList[nextGroupIdx][lastCharIdx] = arrayList[nextGroupIdx][nextCharIdx];
                        arrayList[nextGroupIdx][nextCharIdx] = temp;
                    }
                    // Decrement the number of unprocessed characters in this group.
                    charsLeftInGroup[nextGroupIdx]--;
                }

                // If we processed the last group, start all over.
                if (lastLeftGroupsOrderIdx == 0)
                    lastLeftGroupsOrderIdx = leftGroupsOrder.Length - 1;
                // There are more unprocessed groups left.
                else
                {
                    /* Swap processed group with the last unprocessed group
                       so that we don't pick it until we process all groups.*/
                    if (lastLeftGroupsOrderIdx != nextLeftGroupsOrderIdx)
                    {
                        int temp = leftGroupsOrder[lastLeftGroupsOrderIdx];
                        leftGroupsOrder[lastLeftGroupsOrderIdx] =
                                    leftGroupsOrder[nextLeftGroupsOrderIdx];
                        leftGroupsOrder[nextLeftGroupsOrderIdx] = temp;
                    }
                    // Decrement the number of unprocessed groups.
                    lastLeftGroupsOrderIdx--;
                }
            }

            // Convert password characters into a string and return the result.
            return new string(password);




        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            resultPassword.Text = null;

            if (chkboxLC.IsChecked == false && chkboxUC.IsChecked == false &&
                chkboxNum.IsChecked == false && chkboxSC.IsChecked == false)
            {

                resultPassword.Text = "Error! Please select at least one password requirement";
            }
            else
            {
                string finalchars = CheckBoxTest();
                int length = Convert.ToInt32(requiredLength.Value);
                int noOfResults = Convert.ToInt32(number.Value);

                if (finalchars == null && chkboxSC.IsChecked == true)
                {
                    resultPassword.Text = "Error! Please select at least one special character \n from Advance Options";
                }
                else { 
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < noOfResults; i++)
                {
                    string password = Generate(length, finalchars);
                    builder.Append(password);
                    builder.Append(Environment.NewLine);


                }
                resultPassword.Text += builder.ToString();

                }





            }
        }

        private void PopulateCheckbox()
        {

            string[] specialchars = {"\u0021", "\u0022", "\u0023", "\u0024", "\u0025",
                                      "\u0026", "\u0027", "\u0028", "\u0029", "\u002A",
                                      "\u002B", "\u002C", "\u002D", "\u002E", "\u002F",
                                      "\u003A", "\u003B", "\u003C", "\u003D", "\u003E",
                                      "\u0040", "\u005B", "\u005C", "\u005D", "\u005E",
                                      "\u005F", "\u0060", "\u007B", "\u007C", "\u007D",
                                      "\u007E", "\u003F"
                                    };





            for (int i = 0; i < specialchars.Length; i++)
            {
                CheckBox c = new CheckBox();
                c.IsChecked = true;
                c.FontSize = 18;
                c.Margin = new Thickness(30, 10, 10, 10);
                c.Foreground = Brushes.DarkOrange;
                c.Content = specialchars[i];
                stackChars.Children.Add(c);

            }

        }

        private string CheckBoxTest()
        {
            string finalchars = null;
            foreach (CheckBox child in stackChars.Children)
            {
                if (child.IsChecked == true)
                {
                    finalchars += child.Content;
                }
            }
            return finalchars;
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            stackChars.Visibility = Visibility.Visible;
            stackMain.Visibility = Visibility.Hidden;
            CharBack.Visibility = Visibility.Visible;
            CharInstruct.Visibility = Visibility.Visible;
            selectOption.Visibility = Visibility.Visible;
        }

        private void MainVisible()
        {
            stackMain.Visibility = Visibility.Visible;
            stackChars.Visibility = Visibility.Hidden;
            CharBack.Visibility = Visibility.Hidden;
            CharInstruct.Visibility = Visibility.Hidden;
            selectOption.Visibility = Visibility.Hidden;
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            MainVisible();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox child in stackChars.Children)
            {
                child.IsChecked = true;
            }
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            foreach (CheckBox child in stackChars.Children)
            {
                child.IsChecked = false;
            }
        }
    }





}
