#region License
/*
ErrorLogger

A library that logs unhandled application exceptions to a file.
Copyright (C) 2019 VPKSoft, Petteri Kautonen

Contact: vpksoft@vpksoft.net

This file is part of ErrorLogger.

ErrorLogger is free software: you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ErrorLogger is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with ErrorLogger.  If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Threading;
using VPKSoft.Utils;

namespace VPKSoft.ErrorLogger
{
    /// <summary>
    /// Logs application errors, but does not prevent the application from
    /// crashing.
    /// </summary>
    public class ExceptionLogger: IDisposable
    {
        /// <summary>
        /// FileStream for the "app_messages.log" file.
        /// </summary>
        private FileStream fs = null;

        /// <summary>
        /// StreamWriter for the "app_messages.log" file.
        /// </summary>
        private StreamWriter sw = null;

        /// <summary>
        /// FileStream for the "trace_error.log" file.
        /// </summary>
        private FileStream fs_error = null;

        /// <summary>
        /// TextWriterTraceListener for the "trace_error.log" file.
        /// </summary>
        private TextWriterTraceListener tw = null;

        /// <summary>
        /// As this is a singleton class, we keep its instance here.
        /// </summary>
        private static ExceptionLogger _Instance = null;

        /// <summary>
        /// If the Bound() method was already called.
        /// </summary>
        private static bool bound = false;

        /// <summary>
        /// Log truncate thread. 
        /// </summary>
        private static Thread logTruncation = null;

        /// <summary>
        /// Thread start for log truncate thread. 
        /// </summary>
        /// 
        private static ThreadStart logTruncationStart = null;

        /// <summary>
        /// Should the log truncate thread be running.
        /// </summary>
        private static volatile bool logTruncationStopped = false;

        /// <summary>
        /// Dummy object of thread locking.
        /// </summary>
        private static object _lock = new object();


        /// <summary>
        /// Creates an instance of this class if not already created.
        /// </summary>
        private static void MakeInstance()
        {
            if (_Instance == null)
            {
                _Instance = new ExceptionLogger();
            }
        }

        /// <summary>
        /// An empty private constructor.
        /// </summary>
        private ExceptionLogger()
        {
            ToggleThreads(false);
        }

        private static void Truncation()
        {
            int msCount = 0;
            while (!logTruncationStopped)
            {                
                Thread.Sleep(1000);
                msCount++;
                if (msCount > 3600)
                {
                    TrucateLog(10000, _Instance.instanceCount);
                    msCount = 0;
                }
            }
        }

        /// <summary>
        /// Toggles the state of log truncate thread.
        /// </summary>
        /// <param name="stop">True if the log truncate thread should be stopped,
        /// <para/>otherwise false.</param>
        private static void ToggleThreads(bool stop)
        {
            logTruncationStopped = stop;
            if (stop && logTruncation != null)
            {
                logTruncation.Join();
            }
            else
            {
                logTruncationStart = new ThreadStart(Truncation);
                logTruncation = new Thread(logTruncationStart);
                logTruncation.Start();
            }
        }

        /// <summary>
        /// Binds the exception ExceptionLogger to an application.
        /// <para/>The type of the application is automatically detected.
        /// <para/>Supported types are Windows Forms and Windows Presentation Foundation.
        /// </summary>
        public static void Bind()
        {
            if (bound)
            {
                return;
            }
            bound = true;
            MakeInstance();
            _Instance.CreateStreams();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (!WPF)
            {
                System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            }
        }

        /// <summary>
        /// Binds the exception ExceptionLogger to an application.
        /// <para/>The type of the application is automatically detected.
        /// <para/>Supported types are Windows Forms and Windows Presentation Foundation.
        /// </summary>
        /// <param name="instanceCount">If multiple instances of the application is running the instance number allows to prevent file access exceptions.</param>
        public static void Bind(int instanceCount)
        {
            if (bound)
            {
                return;
            }
            bound = true;
            MakeInstance();
            _Instance.instanceCount = instanceCount;
            _Instance.CreateStreams();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            if (!WPF)
            {
                System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            }
        }

        /// <summary>
        /// A delegate for the ApplicationCrash event.
        /// </summary>
        public delegate void OnApplicationCrash();

        /// <summary>
        /// An event that is raised when the application is about to crash.
        /// </summary>
        public static event OnApplicationCrash ApplicationCrash = null;

        /// <summary>
        /// A delegate for the ApplicationCrashData event.
        /// </summary>
        public delegate void OnApplicationCrashData(ApplicationCrashEventArgs e);

        /// <summary>
        /// An event that is raised when the application is about to crash with additional exception data.
        /// </summary>
        public static event OnApplicationCrashData ApplicationCrashData = null;

        // if multiple instances of the application is running the instance number allows to prevent file access exceptions..
        private int instanceCount = 0;

        /// <summary>
        /// Creates the streams for "trace_error.log" and "app_messages.log" files.
        /// </summary>
        private void CreateStreams()
        {
            string logFile = GetAppSettingsFolder() + "trace_error.log" + (instanceCount > 0 ? instanceCount.ToString() : string.Empty);
            string appLogFile = GetAppSettingsFolder() + "app_messages.log" + (instanceCount > 0 ? instanceCount.ToString() : string.Empty);
            if (!Directory.Exists(GetAppSettingsFolder()))
            {
                Directory.CreateDirectory(GetAppSettingsFolder());
            }

            if (tw == null && bound)
            {
                fs_error = new FileStream(logFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                tw = new TextWriterTraceListener(fs_error, "error_log");

                Trace.Listeners.Add(tw);
                Trace.AutoFlush = true;
            }

            if (fs == null && bound)
            {
                fs = new FileStream(appLogFile, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            }

            if (sw == null && bound)
            {
                sw = new StreamWriter(fs);
            }
        }

        /// <summary>
        /// Unbinds the exception ExceptionLogger from the application.
        /// </summary>
        public static void UnBind()
        {
            if (_Instance != null)
            {
                _Instance.Dispose();
                _Instance = null;
                bound = false;
            }
        }

        /// <summary>
        /// Truncates the logs to a given line amount from the beginning of the file.
        /// </summary>
        /// <param name="lineCount">The amount of lines to leave to the log files.
        /// <para/>Also lines which doesn't start with a string ""MESSAGE BEGIN"
        /// <para/>will be truncated.</param>
        /// <param name="instanceCount">If multiple instances of the application is running the instance number allows to prevent file access exceptions.</param>
        private static void TrucateLog(int lineCount, int instanceCount)
        {
            lock (_lock)
            {

                string logFile = GetAppSettingsFolder() + "trace_error.log" + (instanceCount > 0 ? instanceCount.ToString() : string.Empty);
                string appLogFile = GetAppSettingsFolder() + "app_messages.log" + (instanceCount > 0 ? instanceCount.ToString() : string.Empty);
                try
                {
                    if (_Instance == null)
                    {
                        return;
                    }
                    _Instance.DisposeStreams();
                    List<string> lines = new List<string>(File.ReadAllLines(appLogFile));
                    if (lines.Count > lineCount)
                    {
                        while (lines.Count >= lineCount)
                        {
                            lines.RemoveAt(0);
                        }

                        while (lines.Count > 0 && !lines[0].StartsWith("MESSAGE BEGIN"))
                        {
                            lines.RemoveAt(0);
                        }                     

                        File.WriteAllLines(appLogFile, lines.ToArray());
                    }

                    lines = new List<string>(File.ReadAllLines(logFile));
                    if (lines.Count > lineCount)
                    {
                        AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
                        while (lines.Count >= lineCount)
                        {
                            lines.RemoveAt(0);
                        }

                        while (lines.Count > 0 && !lines[0].StartsWith("MESSAGE BEGIN"))
                        {
                            lines.RemoveAt(0);
                        }

                        File.WriteAllLines(logFile, lines.ToArray());
                        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                    }
                    _Instance.CreateStreams();
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// Logs an exception into the file "trace_error.log".
        /// </summary>
        /// <param name="additionalMessage">An additional message to log with the exception.</param>
        /// <param name="e">An exception which stack trace to log.</param>
        public static void LogError(Exception e, string additionalMessage)
        {
            lock (_lock)
            {
                Trace.WriteLine("MESSAGE BEGIN ---------------------------------------------------------------------------------------------");
                Trace.WriteLine("Application Error [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] (" + MainAppInfo.VersionString + " / " + MainAppInfo.Title + ")");
                Trace.WriteLine("Error Message     [" + e.Message + "]");
                Trace.WriteLine("Additional data   [" + additionalMessage + "]");
                Trace.WriteLine("Stack trace:");
                Trace.WriteLine(e.StackTrace);
                Trace.WriteLine("MESSAGE END -----------------------------------------------------------------------------------------------");
                Trace.WriteLine(string.Empty);
            }
        }

        /// <summary>
        /// Logs an exception into the file "trace_error.log".
        /// </summary>
        /// <param name="e">An exception which stack trace to log.</param>
        public static void LogError(Exception e)
        {
            lock (_lock)
            {
                Trace.WriteLine("MESSAGE BEGIN ---------------------------------------------------------------------------------------------");
                Trace.WriteLine("Application Error [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] (" + MainAppInfo.VersionString + " / " + MainAppInfo.Title + ")");
                Trace.WriteLine("Error Message     [" + e.Message + "]");
                Trace.WriteLine("Stack trace:");
                Trace.WriteLine(e.StackTrace);
                Trace.WriteLine("MESSAGE END -----------------------------------------------------------------------------------------------");
                Trace.WriteLine(string.Empty);
            }
        }

        /// <summary>
        /// Writes a log message to a file called "trace_error.log" into a directory
        /// <para/>%LOCALAPPDATA%\[Application product name]
        /// </summary>
        /// <param name="sender">The source of the unhandled exception event.</param>
        /// <param name="e">An UnhandledExceptionEventArgs that contains the event data.</param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogError((Exception)e.ExceptionObject);
            ApplicationCrash?.Invoke();

            // raise the application crash with additional data if subscribed..
            if (ApplicationCrashData != null)
            {
                var args = new ApplicationCrashEventArgs
                {
                    Exception = (Exception)e.ExceptionObject,
                    IsTerminating = e.IsTerminating,
                    Args = e,
                    Sender = sender,
                };
                ApplicationCrashData?.Invoke(args);
            }
        }

        /// <summary>
        /// Logs an application message with a timestamp to a file called
        /// <para/>"app_messages.log" at directory %LOCALAPPDATA%\[Application product name].
        /// </summary>
        /// <param name="message">A application message to log.</param>
        public static void LogMessage(string message)
        {
            lock (_lock)
            {                
                _Instance.sw.WriteLine("MESSAGE BEGIN ---------------------------------------------------------------------------------------------");
                _Instance.sw.WriteLine("Application Log   [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] (" + MainAppInfo.VersionString + " / " + MainAppInfo.Title + ")");
                _Instance.sw.WriteLine("Message           [" + message + "]");
                _Instance.sw.WriteLine("MESSAGE END -----------------------------------------------------------------------------------------------");
                _Instance.sw.WriteLine(string.Empty);
                _Instance.sw.Flush();
            }
        }


        /// <summary>
        /// True if the application is a ASP.NET application.
        /// </summary>
        public static bool ASP
        {
            get
            {
                return System.Web.HttpContext.Current == null;
            }
        }

        /// <summary>
        /// True if the application is a Windows Forms Application
        /// </summary>
        public static bool WinForms
        {
            get
            {
                return System.Windows.Forms.Application.OpenForms.Count == 0;
            }
        }

        /// <summary>
        /// True if the application is a Windows Presentation Foundation application
        /// </summary>
        public static bool WPF
        {
            get
            {
                return System.Windows.Application.Current != null;
            }
        }

        /// <summary>
        /// Just returns the default writable data directory for "non-roaming" applications.
        /// </summary>
        /// <returns>A writable data directory for "non-roaming" applications.</returns>
        static string GetAppSettingsFolder()
        {
            if (!WPF)
            {
                string appName = System.Windows.Forms.Application.ProductName;
                foreach (char chr in Path.GetInvalidFileNameChars())
                {
                    appName = appName.Replace(chr, '_');
                }

                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + appName + @"\";
            }
            else
            {
                string appName = System.Windows.Application.ResourceAssembly.GetName().Name;
                foreach (char chr in Path.GetInvalidFileNameChars())
                {
                    appName = appName.Replace(chr, '_');
                }

                return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\" + appName + @"\";
            }
        }

        /// <summary>
        /// Disposes the streams for "trace_error.log" and "app_messages.log" files.
        /// </summary>
        private void DisposeStreams()
        {
            if (tw != null)
            {
                try
                {
                    Trace.Listeners.Remove(tw);
                    tw.Dispose();
                    fs_error.Dispose();
                    tw = null;
                }
                catch
                {

                }
            }

            if (sw != null)
            {
                try
                {
                    sw.Dispose();
                    sw = null;
                }
                catch
                {
                    // Do nothing..
                }
            }

            if (fs != null)
            {
                try
                {
                    fs.Dispose();
                    fs = null;
                }
                catch
                {
                    // Do nothing..
                }
            }
        }

        /// <summary>
        /// Disposes application log FileStream and StreamWriter.
        /// </summary>
        public void Dispose()
        {
            ToggleThreads(true);
            AppDomain.CurrentDomain.UnhandledException -= CurrentDomain_UnhandledException;
            DisposeStreams();
        }

        /// <summary>
        /// Disposes application log FileStream and StreamWriter.
        /// </summary>
        ~ExceptionLogger()
        {
            Dispose();
            bound = false;
        }
    }
}

