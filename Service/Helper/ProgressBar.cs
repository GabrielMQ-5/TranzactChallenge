﻿using System;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// An ASCII progress bar
/// </summary>

namespace Service.Helper
{
    public class ProgressBar : IDisposable, IProgress<double>
    {
        private const int blockCount = 10;
        private const string animation = @"|/-\";
        
        private readonly TimeSpan animationInterval = TimeSpan.FromSeconds(1.0 / 8);
        private readonly Timer timer;
        private readonly string relatedFile;        

        private string currentText = string.Empty;
        private double currentProgress = 0;
        private bool disposed = false;
        private int animationIndex = 0;

        public static List<string> exceptionsEncountered = new();

        public ProgressBar(string fileName)
        {
            timer = new Timer(TimerHandler, new AutoResetEvent(false), TimeSpan.FromSeconds(1.0 / 8), TimeSpan.FromSeconds(1.0 / 8));
            relatedFile = fileName;

            if (!Console.IsOutputRedirected)
            {
                ResetTimer();
            }
        }

        public void Report(double value)
        {
            try
            {
                value = Math.Max(0, Math.Min(1, value));
                Interlocked.Exchange(ref currentProgress, value);
            }
            catch (Exception ex)
            {
                exceptionsEncountered.Add(ex.Message);
            }
        }

        private void TimerHandler(object state)
        {
            lock (timer)
            {
                if (disposed) return;

                int progressBlockCount = (int)(currentProgress * blockCount);
                int percent = (int)(currentProgress * 100);
                currentText = string.Format("{0} : [{1}{2}] {3,4}% {4}",
                    relatedFile,
                    new string('#', progressBlockCount),
                    new string('-', blockCount - progressBlockCount),
                    percent,
                    (percent < 100) ? animation[animationIndex++ % animation.Length] : ' ');
                
                ResetTimer();
            }
        }

        private void ResetTimer()
        {
            if (currentProgress >= 1) Dispose();
            else timer.Change(animationInterval, TimeSpan.FromMilliseconds(-1));
        }

        public void Dispose()
        {
            lock (timer)
            {
                disposed = true;
                timer.Dispose();
            }
        }

        public string GetRelatedFile()
        {
            return relatedFile;
        }

        public string GetCurrentText()
        {
            return currentText;
        }
    }
}
