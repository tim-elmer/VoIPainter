using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace VoIPainter.View
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }

        private void CGIExecute_Click(object sender, RoutedEventArgs e) => OpenLink("https://usecallmanager.nz/cgi-execute-xml.html");

        // Via https://stackoverflow.com/a/43232486
        public static void OpenLink(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void BackgroundImages_Click(object sender, RoutedEventArgs e) => OpenLink("https://usecallmanager.nz/image-list-xml.html");

        private void ImageSharp_Click(object sender, RoutedEventArgs e) => OpenLink("https://github.com/SixLabors/ImageSharp");

        private void NetCore_Click(object sender, RoutedEventArgs e) => OpenLink("https://github.com/dotnet/core");
    }
}
