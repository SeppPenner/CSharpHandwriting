using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;

namespace NeuralNetworkLibrary
{
    public class HiPerfTimer
    {
        private readonly long _freq;

        private long _startTime, _stopTime;

        public HiPerfTimer()
        {
            _startTime = 0;
            _stopTime = 0;
            MbStarted = false;
            MbStoped = true;

            if (QueryPerformanceFrequency(out _freq) == false)
                throw new Win32Exception();
        }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private bool MbStarted { get; set; }

        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public bool MbStoped { get; private set; }

        // Returns the duration of the timer (in seconds)

        // ReSharper disable once UnusedMember.Global
        public double Duration => (_stopTime - _startTime) / (double) _freq;

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(
            out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(
            out long lpFrequency);

        // Start the timer

        public void Start()
        {
            // lets do the waiting threads there work

            Thread.Sleep(0);
            QueryPerformanceCounter(out _startTime);
            MbStarted = true;
            MbStoped = false;
        }

        // Stop the timer

        // ReSharper disable once UnusedMember.Global
        public void Stop()
        {
            QueryPerformanceCounter(out _stopTime);
            MbStarted = false;
            MbStoped = true;
        }
    }
}