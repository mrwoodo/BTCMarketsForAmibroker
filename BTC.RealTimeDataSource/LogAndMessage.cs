using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using AmiBroker.Data;

namespace BTC.RealTimeDataSource
{
    /// <summary>
    /// Adapted with sample code from .Net for Amibroker http://www.dotnetforab.com/
    /// </summary>
    internal enum MessageType : int { Error = 0, Warning = 1, Info = 2, Trace = 3 }

    internal class QueuedMessage
    {
        internal MessageType MessageType;
        internal string Message;
        internal DateTime EntryTime;
        internal DateTime DisplayTime;

        internal QueuedMessage(MessageType messageType, string message)
        {
            MessageType = messageType;
            Message = message;
            EntryTime = DateTime.Now;
            DisplayTime = DateTime.MinValue;
        }
    }

    [DebuggerStepThrough]
    internal static class LogAndMessage
    {
        private static bool verboseLog;
        private static string logSource;
        private static List<QueuedMessage> queuedMessages;

        static LogAndMessage()
        {
            verboseLog = false;
            logSource = "DataSource";
            queuedMessages = new List<QueuedMessage>();
        }

        internal static string LogSource
        {
            get
            {
                return logSource;
            }
            set
            {
                logSource = value;
            }
        }

        internal static bool VerboseLog
        {
            get
            {
                return verboseLog;
            }
            set
            {
                verboseLog = value;
            }
        }

        internal static void Log(MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, message);
            }
        }

        internal static void Log(TickerData tickerData, MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, AddTickerToMessage(tickerData, message));
            }
        }

        internal static void LogAndQueue(MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                LogMessage(type, message);
                QueueMessage(type, message);
            }
        }

        internal static void LogAndQueue(TickerData tickerData, MessageType type, string message)
        {
            if (IsLoggable(type))
            {
                string msg = AddTickerToMessage(tickerData, message);

                LogMessage(type, msg);
                QueueMessage(type, msg);
            }
        }

        internal static string GetMessages()
        {
            StringBuilder msg = new StringBuilder(500);

            lock (queuedMessages)
            {
                if (queuedMessages.Count == 0)
                    return string.Empty;

                int waitUnit = 5000;
                if (queuedMessages.Count > 1)
                    waitUnit = 3000;
                if (queuedMessages.Count > 10)
                    waitUnit = 1000;

                while (queuedMessages.Count > 0
                    && queuedMessages[0].DisplayTime != DateTime.MinValue
                    && (queuedMessages[0].DisplayTime.AddMilliseconds(waitUnit) < DateTime.Now ||
                        queuedMessages[0].EntryTime.AddMilliseconds(10000) < DateTime.Now))
                    queuedMessages.RemoveAt(0);

                foreach (QueuedMessage m in queuedMessages)
                {
                    msg.AppendLine(m.Message);
                    if (m.DisplayTime == DateTime.MinValue)
                        m.DisplayTime = DateTime.Now;

                    if (msg.Length > 255)
                    {
                        msg.Length = 255;
                        msg[252] = '.';
                        msg[253] = '.';
                        msg[254] = '.';
                        msg[254] = '\0';
                        break;
                    }
                }
            }

            return msg.ToString();
        }


        private static bool IsLoggable(MessageType type)
        {
            return type == MessageType.Error
                || type == MessageType.Warning
                || type == MessageType.Info
                || type == MessageType.Trace && verboseLog;
        }

        private static bool IsDisplayable(MessageType type)
        {
            return type == MessageType.Error
                || type == MessageType.Warning
                || type == MessageType.Info && verboseLog;
        }

        internal static void LogMessage(MessageType type, string message)
        {
            switch (type)
            {
                case MessageType.Error:
                    DataSourceBase.DotNetLog(logSource, "Error", message);
                    break;

                case MessageType.Warning:
                    DataSourceBase.DotNetLog(logSource, "Warning", message);
                    break;

                case MessageType.Info:
                    DataSourceBase.DotNetLog(logSource, "Info", message);
                    break;

                case MessageType.Trace:
                    DataSourceBase.DotNetLog(logSource, "Trace", message);
                    break;
            }
        }

        private static void QueueMessage(MessageType type, string message)
        {
            lock (queuedMessages)
            {
                switch (type)
                {
                    case MessageType.Error:
                        queuedMessages.Add(new QueuedMessage(type, "ERROR! " + message));
                        break;

                    case MessageType.Warning:
                        queuedMessages.Add(new QueuedMessage(type, "Warning! " + message));
                        break;

                    case MessageType.Info:
                        queuedMessages.Add(new QueuedMessage(type, message));
                        break;

                    case MessageType.Trace:
                        queuedMessages.Add(new QueuedMessage(type, message));
                        break;
                }
            }
        }

        private static string AddTickerToMessage(TickerData tickerData, string message)
        {
            if (tickerData == null)
                return message;
            else
                return tickerData.ToString() + ": " + message;
        }
    }
}
