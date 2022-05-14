// <copyright file="Logging.cs" company="UnFoundBug">
// Copyright (c) UnFoundBug. All rights reserved.
// </copyright>

namespace UnFoundBug.LightLink
{
    using System;
    using System.Text;
    using Sandbox.ModAPI;

    /// <summary>
    /// Logging wrapper.
    /// </summary>
    internal static class Logging
    {
        private static object loggingLock = new object();

        private static int loggingLevel = 0;

        /// <summary>
        /// Write debug level log.
        /// </summary>
        /// <param name="log">Line to write.</param>
        public static void Debug(string log)
        {
            if (loggingLevel >= 3)
            {
                InternalWrite("Debug", log);
            }
        }

        /// <summary>
        /// Write warn level log.
        /// </summary>
        /// <param name="log">Line to write.</param>
        public static void Warn(string log)
        {
            if (loggingLevel >= 2)
            {
                InternalWrite("Warn", log);
            }
        }

        /// <summary>
        /// Write error level log.
        /// </summary>
        /// <param name="log">Line to write.</param>
        public static void Error(string log)
        {
            if (loggingLevel >= 1)
            {
                InternalWrite("Error", log);
            }
        }

        private static void InternalWrite(string level, string log)
        {
            lock (loggingLock)
            {
                using (var writer = MyAPIGateway.Utilities.WriteFileInLocalStorage("LightingLink.log", typeof(Logging)))
                {
                    StringBuilder sbLogLine = new StringBuilder();
                    sbLogLine.Append(DateTime.Now.ToString("o"));
                    sbLogLine.Append("\t");
                    sbLogLine.Append(level);
                    sbLogLine.Append("\t");
                    sbLogLine.Append(log);
                }
            }
        }
    }
}
