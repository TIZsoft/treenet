using System;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace Tizsoft.Log
{
    public static class LoggerManager
    {
        const string ConfigFile = @"log4net.config";

        public static void DefaultSetup()
        {
            var hierarchy = (Hierarchy)LogManager.GetRepository();

            var patternLayout = new PatternLayout
            {
                ConversionPattern =
                    "%date [%thread] %-5level %logger [%property{NDC}] [%property{MDC}] (%file:%line) %stacktrace{5} - %message%newline"
            };
            patternLayout.ActivateOptions();

            var roller = new RollingFileAppender
            {
                AppendToFile = true,
                File = @"logs\log.txt",
                Layout = patternLayout,
                MaximumFileSize = "1GB",
                MaxSizeRollBackups = 5,
                RollingStyle = RollingFileAppender.RollingMode.Size,
                StaticLogFileName = true
            };
            roller.ActivateOptions();
            hierarchy.Root.AddAppender(roller);

            hierarchy.Root.Level = Level.Info;
            hierarchy.Configured = true;
        }

        public static bool LoadConfig(string path)
        {
            var filepath = path.EndsWith("/")
                ? string.Format("{0}{1}", path, ConfigFile)
                : string.Format("{0}/{1}", path, ConfigFile);

            if (File.Exists(filepath))
            {
                log4net.Config.XmlConfigurator.Configure(new FileInfo(filepath));
                return true;
            }

            return false;
        }

        public static int GetCurrentLoggersCount()
        {
            return LogManager.GetCurrentLoggers().Length;
        }
    }

    public class Logger : ILogger
    {
        readonly ILog _log;

        public Logger(Type type)
        {
            if (LoggerManager.GetCurrentLoggersCount() == 0)
            {
                var filepath = System.Reflection.Assembly.GetAssembly(typeof (Logger)).Location;
                filepath = Path.GetDirectoryName(filepath);
                if (!LoggerManager.LoadConfig(filepath))
                {
                    LoggerManager.DefaultSetup();
                }
            }
            _log = LogManager.GetLogger(type);
        }

        public void Debug(object message)
        {
            if (_log.IsDebugEnabled)
            {
                _log.Debug(message);
            }
        }

        public void DebugFormat(string format, params object[] args)
        {
            if (_log.IsDebugEnabled)
            {
                _log.DebugFormat(format, args);
            }
        }

        public void Error(object message)
        {
            if (_log.IsErrorEnabled)
            {
                _log.Error(message);
            }
        }

        public void ErrorFormat(string format, params object[] args)
        {
            if (_log.IsErrorEnabled)
            {
                _log.ErrorFormat(format, args);
            }
        }

        public void Fatal(object message)
        {
            if (_log.IsFatalEnabled)
            {
                _log.Fatal(message);
            }
        }

        public void FatalFormat(string format, params object[] args)
        {
            if (_log.IsFatalEnabled)
            {
                _log.FatalFormat(format, args);
            }
        }

        public void Info(object message)
        {
            if (_log.IsInfoEnabled)
            {
                _log.Info(message);
            }
        }

        public void InfoFormat(string format, params object[] args)
        {
            if (_log.IsInfoEnabled)
            {
                _log.InfoFormat(format, args);
            }
        }

        public void Warn(object message)
        {
            if (_log.IsWarnEnabled)
            {
                _log.Warn(message);
            }
        }

        public void WarnFormat(string format, params object[] args)
        {
            if (_log.IsWarnEnabled)
            {
                _log.WarnFormat(format, args);
            }
        }
    }

    public static class GLogger
    {
        static readonly ILogger Log =  new Logger(typeof(GLogger));

        public static void Debug(object message)
        {
            Log.Debug(message);
        }

        public static void Debug(string format)
        {
            Log.DebugFormat(format);
        }

        public static void Debug(string format, params object[] args)
        {
            Log.DebugFormat(format, args);
        }

        public static void Error(object message)
        {
            Log.Error(message);
        }

        public static void Error(string format)
        {
            Log.ErrorFormat(format);
        }

        public static void Error(string format, params object[] args)
        {
            Log.ErrorFormat(format, args);
        }

        public static void Fatal(object message)
        {
            Log.Fatal(message);
        }

        public static void Fatal(string format)
        {
            Log.FatalFormat(format);
        }

        public static void Fatal(string format, params object[] args)
        {
            Log.FatalFormat(format, args);
        }

        public static void Info(object message)
        {
            Log.Info(message);
        }

        public static void Info(string format)
        {
            Log.InfoFormat(format);
        }

        public static void Info(string format, params object[] args)
        {
            Log.InfoFormat(format, args);
        }

        public static void Warn(object message)
        {
            Log.Warn(message);
        }

        public static void Warn(string format)
        {
            Log.WarnFormat(format);
        }

        public static void Warn(string format, params object[] args)
        {
            Log.WarnFormat(format, args);
        }
    }
}