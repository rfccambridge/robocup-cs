using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;

namespace RFC.Utilities
{
    public delegate void LoopFunction();

    /// <summary>
    /// A simple class for calling a function over and over with a certain period.
    /// </summary>
    public class FunctionLoop
    {
        private LoopFunction loopFn;
        private int sync;
        private System.Timers.Timer timer;

        double desiredPeriod;

        private HighResTimer periodTimer;
        private HighResTimer loopTimer;

        private double observedPeriod;

        private volatile bool isRunning;
        private Object isRunningLock = new Object();

        private object functionRunningLock;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fn">the function to call periodically</param>
        /// <param name="lockObject">the object to lock when calling the function, should be the same object as any message handlers that share data</param>
        public FunctionLoop(LoopFunction fn, object lockObject)
        {
            loopFn = fn;
            sync = 0;
            timer = new System.Timers.Timer();
            timer.AutoReset = true;
            timer.Elapsed += Elapsed;
            desiredPeriod = 1.0;
            timer.Interval = desiredPeriod * 1000;

            periodTimer = new HighResTimer();
            loopTimer = new HighResTimer();

            this.functionRunningLock = lockObject;
        }

        /// <summary>
        /// Starts the loop.
        /// </summary>
        public void Start()
        {
            lock (isRunningLock)
            {
                if (isRunning)
                    throw new Exception("Loop for " + loopFn.Method.Name + " already started!");

                isRunning = true;
                periodTimer.Start();
                timer.Start();
            }
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        public void Stop()
        {
            lock (isRunningLock)
            {
                if (!isRunning)
                    throw new Exception("Loop for " + loopFn.Method.Name + " already started!");
                timer.Stop();
                isRunning = false;
            }
        }

        /// <summary>
        /// Checks if the loop is running
        /// </summary>
        public bool IsRunning()
        {
            return isRunning;
        }

        /// <summary>
        /// Gets the period at which the loop has been set to fire, in seconds
        /// </summary>
        public double GetPeriod()
        {
            return desiredPeriod;
        }

        /// <summary>
        /// Sets the period at which the loop should fire, in seconds
        /// </summary>
        public void SetPeriod(double period)
        {
            desiredPeriod = period;
            timer.Interval = period * 1000;
        }

        /// <summary>
        /// Gets the empirical time in seconds that the last loop appeared to take.
        /// </summary>
        public double GetObservedPeriod()
        {
            return observedPeriod;
        }

        /// <summary>
        /// Call this within the looping function to gets the time in seconds that
        /// that iteration of the loop has spent so far.
        /// </summary>
        public double GetLoopDuration()
        {
            loopTimer.Stop();
            return loopTimer.Duration;
        }


        private void Elapsed(object sender, ElapsedEventArgs e)
        {
            if (Interlocked.CompareExchange(ref sync, 1, 0) == 0)
            {
                periodTimer.Stop();
                observedPeriod = periodTimer.Duration;
                periodTimer.Start();

                loopTimer.Start();

                try
                {
                    lock (functionRunningLock)
                    { 
                        loopFn();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    sync = 0;
                }
            }
        }
    }
}