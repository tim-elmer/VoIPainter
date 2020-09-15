using System;
using System.Collections.ObjectModel;
using System.Globalization;
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
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            System.Windows.Application.Current.Dispatcher.Invoke(() => LogEntries.Add(entry));
            System.IO.File.AppendAllText(".\\VoIPainter.log", string.Format(CultureInfo.InvariantCulture, $"{Strings.LogFormat}\r\n", entry.Time, entry.Severity, entry.Message));
        }
    }
}
