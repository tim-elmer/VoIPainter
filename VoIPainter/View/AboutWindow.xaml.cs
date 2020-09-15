using System;
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

        private void CGIExecute_Click(object sender, RoutedEventArgs e) => UICommon.OpenLink(new Uri("https://usecallmanager.nz/cgi-execute-xml.html"));

        private void BackgroundImages_Click(object sender, RoutedEventArgs e) => UICommon.OpenLink(new Uri("https://usecallmanager.nz/image-list-xml.html"));

        private void ImageSharp_Click(object sender, RoutedEventArgs e) => UICommon.OpenLink(new Uri("https://github.com/SixLabors/ImageSharp"));

        private void NetCore_Click(object sender, RoutedEventArgs e) => UICommon.OpenLink(new Uri("https://github.com/dotnet/core"));
    }
}
