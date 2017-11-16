namespace NFCRing.UI.ViewModel.Services
{
    public interface ILogger
    {
        string Name { get; }

        /// <summary>
        /// Log message to debug level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Debug(string message);

        /// <summary>
        /// Log message to trace level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Trace(string message);

        /// <summary>
        /// Log message to info level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Info(string message);

        /// <summary>
        /// Log message to warning level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Warning(string message);

        /// <summary>
        /// Log message to error level.
        /// </summary>
        /// <param name="message">The message.</param>
        void Error(string message);
    }
}
