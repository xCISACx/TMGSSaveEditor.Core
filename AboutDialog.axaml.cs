using Avalonia.Controls;
using Avalonia.Interactivity;
using System.Diagnostics;
using System;

namespace TMGSSaveEditor.Core
{
    public partial class AboutDialog : Window
    {
        public AboutDialog(string windowTitle)
        {
            InitializeComponent();

            AboutTitleTextBlock.Text = windowTitle;
        }

        private void OpenLink_Click(object sender, RoutedEventArgs e)
        {
            string targetUrl = "https://discord.gg/Kw6mRY96hY";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = targetUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Could not open link: {ex.Message}");
            }
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}