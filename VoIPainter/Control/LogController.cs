using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using VoIPainter.Model.Logging;

namespace VoIPainter.Control
{
    /// <summary>
    /// Handles logging
    /// </summary>
    public class LogController : IDisposable
    {
        private readonly StreamWriter _logStreamWriter;

        /// <summary>
        /// Entries in the log
        /// </summary>
        public ObservableCollection<Entry> LogEntries { get; private set; } = new ObservableCollection<Entry>();

        public LogController()
        {
            _logStreamWriter = new StreamWriter(File.OpenWrite(".\\VoIPainter.log"));
        }

        /// <summary>
        /// Log an entry
        /// </summary>
        /// <param name="entry">The entry</param>
        public void Log(Entry entry)
        {
            if (entry is null)
                throw new ArgumentNullException(nameof(entry));

            System.Windows.Application.Current.Dispatcher.Invoke(() => LogEntries.Add(entry));
            _logStreamWriter.WriteLine(string.Format(CultureInfo.InvariantCulture, $"{Strings.LogFormat}\r\n", entry.Time, entry.Severity, entry.Message));
        }

        protected virtual void Dispose(bool disposeManaged)
        {
            if (disposeManaged)
                _logStreamWriter.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
