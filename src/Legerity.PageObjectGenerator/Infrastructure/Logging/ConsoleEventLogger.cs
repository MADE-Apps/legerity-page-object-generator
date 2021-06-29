namespace Legerity.PageObjectGenerator.Infrastructure.Logging
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class ConsoleEventLogger
    {
        private const string LogFormat = "[{0:T} {1}]\t {2}";

        private readonly SemaphoreSlim writeSemaphore = new SemaphoreSlim(1, 1);
        private static ConsoleEventLogger current;

        public static ConsoleEventLogger Current => current ??= new ConsoleEventLogger();

        /// <summary>
        /// Writes a debug information message to the event log when in DEBUG mode.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public async void WriteDebug(string message)
        {
            if (!System.Diagnostics.Debugger.IsAttached)
            {
                return;
            }

            string log = string.Format(LogFormat, DateTime.Now, "Debug", message);
            await this.WriteToConsoleAsync(log);
        }

        /// <summary>
        /// Writes a generic information message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public async void WriteInfo(string message)
        {
            string log = string.Format(LogFormat, DateTime.Now, "Info", message);
            await this.WriteToConsoleAsync(log);
        }

        /// <summary>
        /// Writes a warning message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public async void WriteWarning(string message)
        {
            string log = string.Format(LogFormat, DateTime.Now, "Warning", message);
            await this.WriteToConsoleAsync(log, ConsoleColor.DarkYellow);
        }

        /// <summary>
        /// Writes an error message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public async void WriteError(string message)
        {
            string log = string.Format(LogFormat, DateTime.Now, "Error", message);
            await this.WriteToConsoleAsync(log, ConsoleColor.Red);
        }

        /// <summary>
        /// Writes a critical error message to the event log.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        public async void WriteCritical(string message)
        {
            string log = string.Format(LogFormat, DateTime.Now, "Critical", message);
            await this.WriteToConsoleAsync(log, ConsoleColor.Red);
        }

        /// <summary>
        /// Writes an exception to the event log as a debug message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteDebug(string message, Exception ex)
        {
            this.WriteDebug($"{message} - {ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a debug message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteDebug(Exception ex)
        {
            this.WriteDebug($"{ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a generic information message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteInfo(string message, Exception ex)
        {
            this.WriteInfo($"{message} - {ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a generic information message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteInfo(Exception ex)
        {
            this.WriteInfo($"{ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a warning message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteWarning(string message, Exception ex)
        {
            this.WriteWarning($"{message} - {ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a warning message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteWarning(Exception ex)
        {
            this.WriteWarning($"{ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as an error message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteError(string message, Exception ex)
        {
            this.WriteError($"{message} - {ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as an error message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteError(Exception ex)
        {
            this.WriteError($"{ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a critical message.
        /// </summary>
        /// <param name="message">
        /// The message to write out.
        /// </param>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteCritical(string message, Exception ex)
        {
            this.WriteCritical($"{message} - {ex}");
        }

        /// <summary>
        /// Writes an exception to the event log as a critical message.
        /// </summary>
        /// <param name="ex">
        /// The exception to write out.
        /// </param>
        public void WriteCritical(Exception ex)
        {
            this.WriteCritical($"{ex}");
        }

        private async Task WriteToConsoleAsync(string line, ConsoleColor? textColor = null)
        {
            await this.writeSemaphore.WaitAsync();

            if (System.Diagnostics.Debugger.IsAttached)
            {
                System.Diagnostics.Debug.WriteLine(line);
            }

            try
            {
                if (textColor != null)
                {
                    Console.ForegroundColor = textColor.Value;
                }

                Console.WriteLine(line);

                // Reset color
                Console.ForegroundColor = ConsoleColor.Gray;
            }
            catch (IOException ioe)
            {
                if (System.Diagnostics.Debugger.IsAttached)
                {
                    System.Diagnostics.Debug.WriteLine(
                        $"An exception was thrown while writing to the console. {ioe}");
                }
            }
            finally
            {
                this.writeSemaphore.Release();
            }
        }
    }
}