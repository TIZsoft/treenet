using NLog;
using NLog.Config;
using NLog.Targets;

namespace Tizsoft.Log
{
    public static class GLogger
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Reference https://gist.github.com/1162357/930f9c124b9ba05c819474da26551a5aadf35aec#file-gistfile1-txt-L21 to create NLog config programmatically.
        /// </summary>
        static LoggingConfiguration CreateDefaultRichTextBoxConfig(string name, string layout, bool autoScroll, int maxLines, string controlName, string formName)
        {
            var config = new LoggingConfiguration();
            var target = new RichTextBoxTarget
            {
                Name = name,
                Layout = layout,
                AutoScroll = autoScroll,
                MaxLines = maxLines,
                ControlName = controlName,
                FormName = formName,
                UseDefaultRowColoringRules = true
            };
            target.RowColoringRules.Add(new RichTextBoxRowColoringRule("level == LogLevel.Info", "Empty", "Black"));
            config.AddTarget(name, target);
            var rule = new LoggingRule("*", LogLevel.Info, target);
            config.LoggingRules.Add(rule);
            return config;

            // using config like below: 
            //_logger = LogManager.GetCurrentClassLogger();
            //_logger.Debug("using programmatic config");
        }

        public static void Trace(string msg)
        {
            _logger.Trace(msg);
        }

        public static void Trace(string msg, params object[] args)
        {
            _logger.Trace(msg, args);
        }

        public static void Trace(object msg)
        {
            _logger.Trace(msg);
        }

        public static void Debug(string msg)
        {
            _logger.Debug(msg);
        }

        public static void Debug(string msg, params object[] args)
        {
            _logger.Debug(msg, args);
        }

        public static void Debug(object msg)
        {
            _logger.Debug(msg);
        }

        public static void Info(string msg)
        {
            _logger.Info(msg);
        }

        public static void Info(string msg, params object[] args)
        {
            _logger.Info(msg, args);
        }

        public static void Info(object msg)
        {
            _logger.Info(msg);
        }

        public static void Warn(string msg)
        {
            _logger.Warn(msg);
        }

        public static void Warn(string msg, params object[] args)
        {
            _logger.Warn(msg, args);
        }

        public static void Warn(object msg)
        {
            _logger.Warn(msg);
        }

        public static void Error(string msg)
        {
            _logger.Error(msg);
        }

        public static void Error(string msg, params object[] args)
        {
            _logger.Error(msg, args);
        }

        public static void Error(object msg)
        {
            _logger.Error(msg);
        }

        public static void Fatal(string msg)
        {
            _logger.Fatal(msg);
        }

        public static void Fatal(string msg, params object[] args)
        {
            _logger.Fatal(msg, args);
        }

        public static void Fatal(object msg)
        {
            _logger.Fatal(msg);
        }
    }
}