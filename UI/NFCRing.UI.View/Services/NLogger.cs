using NLog;
using ILogger = NFCRing.UI.ViewModel.Services.ILogger;

namespace NFCRing.UI.View.Services
{
    public class NLogger : ILogger
    {
        private readonly Logger _logger;

        public string Name { get; }

        public NLogger()
        {
            LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration("nlog.config");

            _logger = LogManager.GetLogger("main");

            Name = _logger.Name;
        }

        public void Debug(string message)
        {
            _logger.Debug(message);
        }

        public void Trace(string message)
        {
            _logger.Trace(message);
        }

        public void Info(string message)
        {
            _logger.Info(message);
        }

        public void Warning(string message)
        {
            _logger.Warn(message);
        }

        public void Error(string message)
        {
            _logger.Error(message);
        }
    }
}
