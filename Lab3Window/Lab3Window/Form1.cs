using System;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lab3Window
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            toolStripComboBoxKeywords.Items.AddRange(blockedKeywords.Cast<object>().ToArray());

            //webBrowser1.ScriptErrorsSuppressed = true;

            // toolStripTextBox1.KeyDown += toolStripTextBox1_KeyDown;

            //Handel Enter in the keyword textBox
            toolStripTextBox2.KeyDown += toolStripTextBoxKeyword_KeyDown;

            Trace.Listeners.Add(new TextWriterTraceListener(logFile));

            Trace.AutoFlush = true;

            webBrowser1.Navigating += webBrowser1_Navigating;

            //Handel Enter in the keyword textBox
            
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            string url = toolStripTextBox1.Text.Trim();

            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            webBrowser1.Navigate(url);
        }
        

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            webBrowser1.GoHome();
        }
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
        }
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string url = toolStripTextBox1.Text.Trim();

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }
                _ = LogEventAsync("User navigated to: " + url);
                webBrowser1.Navigate(url);

                e.SuppressKeyPress = true;

            }
        }
        //Blocked keywords list
        private List<string> blockedKeywords = new List<string>
        {
            "facebook",
            "tiktok",
            "gambling",
            "instagram"
        };

        /* private void webBrowser1_Navigating(object sender,WebBrowserNavigatingEventArgs e)
         {
             string url = e.Url.ToString().ToLower();
             foreach(string keyword in blockedKeywords)
             {
                 if (url.Contains(keyword.ToLower()))
                 {
                     e.Cancel = true;

                     MessageBox.Show(
                "Access to this page is blocked.",
                "Blocked",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
              );

                     return;
                 }*/
       //Using LINQ 
       /* private void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            
        
            string url = e.Url.ToString().ToLower();

            bool blocked = blockedKeywords.Any(keyword =>
                url.Contains(keyword.ToLower()));

            if (blocked)
            {
                e.Cancel = true;

                MessageBox.Show(
                    "This page is blocked!",
                    "Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }*/
       //Using LINQ + AsyncTask
        private async Task<bool> IsUrlBlockedAsync(string url)
        {
            return await Task.Run(() =>blockedKeywords.Any(keyword =>
                    url.Contains(keyword.ToLower())));
        }

        private async void webBrowser1_Navigating(object sender,WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString().ToLower();
            //won't let blocked urls to open
            bool blocked = blockedKeywords.Any(keyword =>url.IndexOf(keyword,StringComparison.OrdinalIgnoreCase) >= 0);

            //if the url is blocked shows this message box 
            if (blocked)
            {
                e.Cancel = true;

                MessageBox.Show(
                    "This page is blocked!",
                    "Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
            }
        }

        //Writes log events into a txt file
        private readonly string logFile = "browser_log.txt";

        private async Task LogEventAsync(string message)
        {
            await Task.Run(() =>
            {
                //Adds the curent time for the log event
                DateTime now = DateTime.Now;

                string timestamp = now.ToString("dd.MM.yyyy HH:mm:ss.fff");

                Trace.WriteLine($"{timestamp} - {message}");
            });
        }
        private void toolStripTextBoxKeyword_KeyDown (object sender, KeyEventArgs e)
        {//Adding keywords and using Enter
            if (e.KeyCode == Keys.Enter)
            {
                string keyword = toolStripTextBox2.Text.Trim();
                //Checks if the text box is empty or not and if we adding just spaces it won't let the program to add it to the combo box 
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    e.SuppressKeyPress = true;
                    return;
                }
                //Looking for duplicate keywords
                bool exists = blockedKeywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    MessageBox.Show("This keyword is already exists!");
                }
                else
                {
                    //This adds keywords to the combo box by pressing Enter
                    blockedKeywords.Add(keyword);
                    toolStripComboBoxKeywords.Items.Add(keyword);
                }
                //Clears the text box after we pressed Enter
                toolStripTextBox2.Clear();
                e.SuppressKeyPress = true;
            }


        }
        }
}













       
   
