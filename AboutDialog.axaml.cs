using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Diagnostics;

namespace TMGSSaveEditor.Core
{
    public partial class AboutDialog : Window
    {
        public AboutDialog(string windowTitle, string projectName)
        {
            InitializeComponent();

            //AboutTitleTextBlock.Text = windowTitle;

            if (!string.IsNullOrEmpty(projectName))
            {
                try
                {
                    string logoPath = $"avares://{projectName}/Assets/logo.png";
                    PictureBoxLogo.Source = new Bitmap(AssetLoader.Open(new Uri(logoPath)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load dialog logo: {ex.Message}");
                }
            }
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