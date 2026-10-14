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
    // Main window of the browser. It contains:
    //  - a WebBrowser control (webBrowser1) that displays the pages
    //  - a ToolStrip with navigation buttons and an address bar (toolStripTextBox1)
    //  - a keyword filter: a textbox (toolStripTextBox2) to add words and a combo box
    //    (toolStripComboBoxKeywords) that shows the blocked words
    public partial class Form1 : Form
    {
        public Form1()
        {
            // Creates and places all the controls designed in the Visual Studio designer
            InitializeComponent();

            // Fills the combo box with the keywords that are already in the blocked list.
            // AddRange needs an object[] so the list of strings is converted first.
            toolStripComboBoxKeywords.Items.AddRange(blockedKeywords.Cast<object>().ToArray());

            // Hides the script error popups that some websites cause (disabled for now)
            //webBrowser1.ScriptErrorsSuppressed = true;

            // Connects the Enter key of the address bar to its handler (disabled for now,
            // so at the moment only the Go button navigates)
            // toolStripTextBox1.KeyDown += toolStripTextBox1_KeyDown;

            // Handle Enter in the keyword textbox: pressing Enter adds the word to the block list
            toolStripTextBox2.KeyDown += toolStripTextBoxKeyword_KeyDown;

            // Sends everything written with Trace.WriteLine into the log file (browser_log.txt)
            Trace.Listeners.Add(new TextWriterTraceListener(logFile));

            // Writes to the file immediately instead of waiting for a buffer to fill up,
            // so no log lines are lost if the program closes
            Trace.AutoFlush = true;

            // Runs our check every time the browser is about to open a page,
            // before the page is loaded. This is where blocking happens.
            webBrowser1.Navigating += webBrowser1_Navigating;
        }

        // Back button: goes to the previous page in the history
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            webBrowser1.GoBack();
        }

        // Forward button: goes to the next page in the history
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            webBrowser1.GoForward();
        }

        // Go button: opens the address typed in the address bar
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            // Trim removes spaces at the beginning and the end of the text
            string url = toolStripTextBox1.Text.Trim();

            // If the user typed only "google.com", add the protocol so Navigate understands it
            if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            {
                url = "https://" + url;
            }

            webBrowser1.Navigate(url);
        }


        // Home button: opens the home page configured in Internet Explorer / Windows
        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            webBrowser1.GoHome();
        }

        // Empty handler created by the designer (nothing to do when the address bar is clicked)
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {
        }

        // Pressing Enter in the address bar does the same as the Go button, plus it writes
        // a line in the log. It only works if the line in the constructor is uncommented.
        private void toolStripTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string url = toolStripTextBox1.Text.Trim();

                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    url = "https://" + url;
                }

                // "_ =" means we start the async log but we don't wait for it to finish
                _ = LogEventAsync("User navigated to: " + url);
                webBrowser1.Navigate(url);

                // Stops the "ding" sound that Windows plays when Enter is pressed in a textbox
                e.SuppressKeyPress = true;
            }
        }

        // Blocked keywords list: any URL that contains one of these words is not opened.
        // It's a List (not an array) so we can add new words while the program runs.
        private List<string> blockedKeywords = new List<string>
        {
            "facebook",
            "tiktok",
            "gambling",
            "instagram"
        };

        // ---------- Version 1: blocking with a foreach loop ----------
        // Checks every keyword one by one and stops the navigation at the first match.
        // Kept as a comment to compare it with the LINQ versions below.
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

        // ---------- Version 2: blocking with LINQ ----------
        // Any() returns true if at least one keyword is found in the URL,
        // so the loop is replaced with a single line.
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

        // ---------- Version 3: LINQ + async Task ----------
        // Does the same check as above but on a background thread, so the window
        // doesn't freeze if the list gets very long. Returns true when the URL is blocked.
        // Note: this method is not called anywhere yet.
        private async Task<bool> IsUrlBlockedAsync(string url)
        {
            // Task.Run moves the work to another thread; await waits for the result
            return await Task.Run(() => blockedKeywords.Any(keyword =>
                    url.Contains(keyword.ToLower())));
        }

        // Runs before every page opens (connected in the constructor).
        // If the URL contains a blocked keyword, the navigation is canceled.
        // This version is the one that is actually used.
        private async void webBrowser1_Navigating(object sender, WebBrowserNavigatingEventArgs e)
        {
            string url = e.Url.ToString().ToLower();

            // Won't let blocked URLs open.
            // IndexOf with OrdinalIgnoreCase finds the word whatever the letter case is
            // (returns -1 when the word is not found, so >= 0 means "found").
            bool blocked = blockedKeywords.Any(keyword => url.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);

            // If the URL is blocked, cancel the navigation and show a message box
            if (blocked)
            {
                // Cancel = true tells the WebBrowser not to load the page
                e.Cancel = true;

                MessageBox.Show(
                    "This page is blocked!",   // message
                    "Blocked",                 // window title
                    MessageBoxButtons.OK,      // only an OK button
                    MessageBoxIcon.Warning     // yellow warning icon
                );
            }
        }

        // Name of the text file where the log is written (created next to the .exe)
        private readonly string logFile = "browser_log.txt";

        // Writes one line in the log file, with the date and time in front of it.
        // Runs on a background thread so writing to the disk doesn't block the window.
        private async Task LogEventAsync(string message)
        {
            await Task.Run(() =>
            {
                // Adds the current time for the log event
                DateTime now = DateTime.Now;

                // Format example: 14.10.2026 18:30:05.123 (day.month.year hours:minutes:seconds.milliseconds)
                string timestamp = now.ToString("dd.MM.yyyy HH:mm:ss.fff");

                // Trace has the file listener from the constructor, so this ends up in browser_log.txt
                Trace.WriteLine($"{timestamp} - {message}");
            });
        }

        // Adds a new blocked keyword when the user presses Enter in the keyword textbox
        private void toolStripTextBoxKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            // Only react to the Enter key, ignore any other key
            if (e.KeyCode == Keys.Enter)
            {
                // Read the typed word and remove the spaces around it
                string keyword = toolStripTextBox2.Text.Trim();

                // If the textbox is empty (or contains only spaces), don't add anything
                if (string.IsNullOrWhiteSpace(keyword))
                {
                    e.SuppressKeyPress = true;
                    return;
                }

                // Looks for a duplicate keyword (ignoring upper/lower case, so "TikTok" = "tiktok")
                bool exists = blockedKeywords.Any(k => k.Equals(keyword, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    // The word is already blocked, so we only warn the user
                    MessageBox.Show("This keyword already exists!");
                }
                else
                {
                    // Adds the keyword to the real list (used for blocking)
                    // and to the combo box (only shows it to the user)
                    blockedKeywords.Add(keyword);
                    toolStripComboBoxKeywords.Items.Add(keyword);
                }

                // Clears the textbox after Enter was pressed, ready for the next word
                toolStripTextBox2.Clear();

                // Stops the "ding" sound of the Enter key
                e.SuppressKeyPress = true;
            }
        }
    }
}