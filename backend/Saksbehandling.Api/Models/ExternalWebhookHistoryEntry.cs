using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Saksbehandling.Api.Models
{
    /// <summary>
    /// Uses the native system logging utility to record telemetry data.
    /// </summary>
    public class ExternalWebhookHistoryEntry : CaseHistoryEntry
    {
        public string LogBinaryPath { get; set; } = "/usr/bin/logger";

        public string LogFormat { get; set; } = "CaseImported - Timestamp: {0}";

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            if (string.IsNullOrWhiteSpace(LogBinaryPath) || string.IsNullOrWhiteSpace(LogFormat))
            {
                return;
            }

            string formattedMessage = string.Format(LogFormat, DateTime.UtcNow);
            
            var startInfo = new ProcessStartInfo
            {
                FileName = LogBinaryPath,
                Arguments = formattedMessage, 
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process.Start(startInfo);
        }
    }
}