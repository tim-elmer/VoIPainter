using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace VoIPainter.Model.Logging
{
    public class Entry
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

        public static ReadOnlyDictionary<LogSeverity, string> LogSeverityStrings { get; } = new ReadOnlyDictionary<LogSeverity, string>(new Dictionary<LogSeverity, string>()
        {
            { LogSeverity.Info, Strings.LogSeverityInfo },
            { LogSeverity.Warning, Strings.LogSeverityWarning },
            { LogSeverity.Error, Strings.LogSeverityError }
        });

        public override string ToString() => string.Format(CultureInfo.InvariantCulture, Strings.LogFormat, Time.ToString(CultureInfo.CurrentUICulture.DateTimeFormat.ShortTimePattern, CultureInfo.InvariantCulture), Severity, Message);
    }
}
