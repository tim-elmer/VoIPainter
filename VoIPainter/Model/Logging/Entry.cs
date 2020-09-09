using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VoIPainter.Model.Logging
{
    public class Entry: EventArgs
    {   
        public DateTime Time { get; }
        public LogSeverity Severity { get; }
        public string Message { get; }

        public Entry(LogSeverity severity, string message)
        {
            Time = DateTime.Now;
            Severity = severity;
            Message = message;
        }

        public static ReadOnlyDictionary<LogSeverity, string> LogSeverityStrings = new ReadOnlyDictionary<LogSeverity, string>(new Dictionary<LogSeverity, string>()
        {
            { LogSeverity.Info, Strings.LogSeverityInfo },
            { LogSeverity.Warning, Strings.LogSeverityWarning },
            { LogSeverity.Error, Strings.LogSeverityError }
        });

        public override string ToString() => string.Format(Strings.LogFormat, Time.ToString(System.Globalization.CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern), Severity, Message);
    }
}
