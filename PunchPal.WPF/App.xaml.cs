using PunchPal.Tools;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace PunchPal.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex _procMutex;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            _procMutex = new System.Threading.Mutex(true, NameTools.AppMutex, out var result);
            if (!result && !CheckProcessVersion())
            {
                ConnectNamedPipe();
                Current.Shutdown();
                Process.GetCurrentProcess().Kill();
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            Dispatcher.UnhandledException += DispatcherOnUnhandledException;
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        private void ConnectNamedPipe()
        {
            try
            {
                using (var clientStream = new NamedPipeClientStream(".", NameTools.NamedPipe, PipeDirection.InOut, PipeOptions.None))
                {
                    clientStream.Connect(500);
                    var data = Encoding.UTF8.GetBytes("{\"type\": \"show\"}");
                    clientStream.Write(data, 0, data.Length);
                }
            }
            catch (Exception)
            {
            }
        }

        private bool CheckProcessVersion()
        {
            var currentProcess = Process.GetCurrentProcess();
            var processes = Process.GetProcessesByName(currentProcess.ProcessName);
            var killed = false;
            foreach (var process in processes)
            {
                if (process.Id == currentProcess.Id)
                {
                    continue;
                }

                if (currentProcess.MainModule != null && process.MainModule?.FileVersionInfo.FileVersion !=
                    currentProcess.MainModule.FileVersionInfo.FileVersion)
                {
                    Trace.WriteLine("Kill process " + process.Id);
                    process.Kill();
                    process.WaitForExit();
                    killed = true;
                }
            }
            return killed;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
            _procMutex?.ReleaseMutex();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            if (exception != null)
            {
                Trace.WriteLine("UnhandledCurrentDomainException: " + exception);
            }
        }

        private static void DispatcherOnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var exception = e.Exception;
            if (exception != null)
            {
                Trace.WriteLine("UnhandledDispatcherException: " + exception);
            }
        }
    }

}
