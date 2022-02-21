using SRTPluginBase;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace SRTPluginProviderRE2
{
    public class SRTPluginProviderRE2 : IPluginProducer
    {
        private Process process;
        private GameMemoryRE2Scanner gameMemoryScanner;
        private Stopwatch stopwatch;
        public IPluginInfo Info => new PluginInfo();
        public bool Available
        {
            get
            {
                if (gameMemoryScanner != null && !gameMemoryScanner.ProcessRunning)
                {
                    process = GetProcess();
                    if (process != null)
                        gameMemoryScanner.Initialize(process); // Re-initialize and attempt to continue.
                }

                return gameMemoryScanner != null && gameMemoryScanner.ProcessRunning;
            }
        }

        public int Startup()
        {
            process = GetProcess();
            gameMemoryScanner = new GameMemoryRE2Scanner(process);
            stopwatch = new Stopwatch();
            stopwatch.Start();
            return 0;
        }

        public int Shutdown()
        {
            gameMemoryScanner?.Dispose();
            gameMemoryScanner = null;
            stopwatch?.Stop();
            stopwatch = null;
            return 0;
        }

        public object PullData()
        {
            try
            {
                if (!Available) // Not running? Bail out!
                    return null;

                if (stopwatch.ElapsedMilliseconds >= 2000L)
                {
                    gameMemoryScanner.UpdatePointers();
                    stopwatch.Restart();
                }
                return gameMemoryScanner.Refresh();
            }
            catch (Win32Exception ex)
            {
                //if ((ProcessMemory.Win32Error)ex.NativeErrorCode != ProcessMemory.Win32Error.ERROR_PARTIAL_COPY)
                    //hostDelegates.ExceptionMessage(ex);// Only show the error if its not ERROR_PARTIAL_COPY. ERROR_PARTIAL_COPY is typically an issue with reading as the program exits or reading right as the pointers are changing (i.e. switching back to main menu).

                return null;
            }
            catch// (Exception ex)
            {
                //hostDelegates.ExceptionMessage(ex);
                return null;
            }
        }

        private Process GetProcess() => Process.GetProcessesByName("re2")?.FirstOrDefault();

        public bool Equals(IPlugin? other) => (this as IPlugin).Equals(other);

        public bool Equals(IPluginProducer? other) => (this as IPluginProducer).Equals(other);
    }
}
