using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SharedLayer.Nlog
{
    public class LoggerService : ILoggerService
    {
        private static NLog.ILogger logger = LogManager.GetCurrentClassLogger();
        public void LogDebug(string message)
        {
            logger.Debug(message);
        }

        public void LogError(Exception ex)
        {
            var filteredStackTrace = FilterStackTrace(ex.StackTrace);
            var exception = new
            {
                message = ex.Message,
                exception = filteredStackTrace,
                innerException = ex.InnerException,
                source = ex.Source,
            };
            string exTostring = JsonSerializer.Serialize(exception);
            logger.Error(exTostring);
        }

        public void LogInfo(string message)
        {
            logger.Info(message);
        }

        public void LogTrace(string message)
        {
            logger.Trace(message);
        }

        public void LogWarn(string message)
        {
            logger.Warn(message);
        }

        private string FilterStackTrace(string stackTrace)
        {
            // Exclude lines that contain certain namespaces or method patterns
            var lines = stackTrace?.Split(Environment.NewLine);
            return lines != null
                ? string.Join(Environment.NewLine, lines.Where(line => !line.Contains("Microsoft.AspNetCore.Mvc") &&
                                                                       !line.Contains("Microsoft.AspNetCore.Routing") &&
                                                                       !line.Contains("Microsoft.Extensions.Logging") &&
                                                                       !line.Contains("System.Runtime.CompilerServices")))
                : stackTrace;
        }
    }
}
