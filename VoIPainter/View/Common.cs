using System.Diagnostics;
using System.Runtime.InteropServices;

namespace VoIPainter.View
{
    /// <summary>
    /// Provides common UI functions
    /// </summary>
    public static class Common
    {
        /// <summary>
        /// Open the specified link in the default browser
        /// </summary>
        /// <param name="url"></param>
        public static void OpenLink(string url)
        {
            // Via https://stackoverflow.com/a/43232486
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
    }
}
