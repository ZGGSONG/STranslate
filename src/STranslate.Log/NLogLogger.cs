//using NLog;
//using System;

//namespace STranslate.Log
//{
//    public class NLogLogger : BaseLogger
//    {
//        private readonly Logger _logger;

//        public NLogLogger(LogLevel minLevel = LogLevel.Debug)
//        {
//            var logFileName = $"logs/log{DateTime.Now:yyyyMMdd}.log";
//            var convLevel = ToNLogLevel(minLevel);

//            var conf = new NLog.Config.LoggingConfiguration();
//            conf.AddRule(
//            convLevel,
//            NLog.LogLevel.Fatal,
//                new NLog.Targets.FileTarget("logfile") { FileName = logFileName }
//            );
//            LogManager.Configuration = conf;
//            _logger = LogManager.GetCurrentClassLogger();
//        }

//        /// <summary>
//        /// 辅助方法将自定义的LogLevel转换为NLog的NLogLevel
//        /// </summary>
//        /// <param name="level"></param>
//        /// <returns></returns>
//        private NLog.LogLevel ToNLogLevel(LogLevel level) => level switch
//        {
//            LogLevel.Trace => NLog.LogLevel.Trace,
//            LogLevel.Debug => NLog.LogLevel.Debug,
//            LogLevel.Info => NLog.LogLevel.Info,
//            LogLevel.Warn => NLog.LogLevel.Warn,
//            LogLevel.Error => NLog.LogLevel.Error,
//            LogLevel.Fatal => NLog.LogLevel.Fatal,
//            _ => NLog.LogLevel.Debug,
//        };

//        public override void Debug(string message)
//        {
//            base.Debug(message);
//            _logger.Debug(message);
//        }

//        public override void Info(string message)
//        {
//            base.Info(message);
//            _logger.Info(message);
//        }

//        public override void Warn(string message)
//        {
//            base.Warn(message);
//            _logger.Warn(message);
//        }

//        public override void Error(string message)
//        {
//            base.Error(message);
//            _logger.Error(message);
//        }

//        public override void Error(string message, Exception ex)
//        {
//            base.Error(message);
//            _logger.Error(ex, message);
//        }

//        public override void Fatal(string message)
//        {
//            base.Fatal(message);
//            _logger.Fatal(message);
//        }

//        public override void Fatal(string message, Exception ex)
//        {
//            base.Fatal(message);
//            _logger.Fatal(ex, message);
//        }

//        public override void Dispose()
//        {
//            base.Dispose();
//            LogManager.Shutdown();
//        }
//    }
//}

