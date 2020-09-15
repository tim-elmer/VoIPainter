using System.Collections.ObjectModel;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles logging
    /// </summary>
    public class LogController
    {
        public ObservableCollection<Entry> LogEntries { get; private set; } = new ObservableCollection<Entry>();

        public void Log(Entry entry)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() => LogEntries.Add(entry));
            System.IO.File.AppendAllText(".\\VoIPainter.log", string.Format($"{Strings.LogFormat}\r\n", entry.Time, entry.Severity, entry.Message));
        }
    }
}
