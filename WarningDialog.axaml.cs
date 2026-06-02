using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace TMGSSaveEditor.Core
{
    public partial class WarningDialog : Window
    {
        public WarningDialog(string title, string message, string projectName)
        {
            InitializeComponent();

            this.Title = title;
            MessageTextBlock.Text = message;

            if (!string.IsNullOrEmpty(projectName))
            {
                try
                {
                    string logoPath = $"avares://{projectName}/Assets/character_shock.png";
                    PictureBoxLogo.Source = new Bitmap(AssetLoader.Open(new Uri(logoPath)));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load dialog logo: {ex.Message}");
                }
            }
        }

        public WarningDialog()
        {
            InitializeComponent();
        }

        private void Yes_Click(object sender, RoutedEventArgs e)
        {
            Close(true);
        }

        private void No_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}