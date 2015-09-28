/*
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace TsdClient
{
    internal sealed class TsdTimer : ITimer
    {
        private volatile Boolean isOpen;
        private Boolean IsOpen { get { return isOpen; } set { isOpen = value; } }

        private ILog Log { get; set; }
        public string Name { get; private set; }
        public TsdTimer(string name, bool isOpen)
            : this(name, isOpen, LogManager.GetLogger(typeof(TsdTimer)))
        {

        }
        public TsdTimer(string name, bool isOpen, ILog logger)
        {
            Log = logger;
            IsOpen = isOpen;
            Name = name;
            Unit = Units.NanoSecond;
            if (isOpen)
                Start();
            else
                IsStopped = true;
        }
        public TsdTimer(string name, Int64 elapsed, bool isOpen)
            : this(name, isOpen)
        {
            this.elapsed = elapsed;
        }
        public Int64 StartTime { get { return Interlocked.Read(ref startTime); } }

        private Int64 startTime;
        private Int64 elapsed;


        public Int64 Elapsed
        {
            get
            {
                if (!IsStopped)
                    Log.Error(m => m("Trying to read a non stopped timer: {0}", Name));
                return Interlocked.Read(ref elapsed);
            }
        }

        private IUnit unit;

        public IUnit Unit
        {
            get
            {
                lock (this)
                    return unit;
            }
            set
            {
                lock (this)
                    unit = value;
            }
        }

        private void Start()
        {
            // if(IsStarted)
            //    throw new InvalidOperationException("Trying to start an already started timer: " + Name);
            startTime = DateTime.Now.Ticks;
            IsStopped = false;
        }

        public Boolean IsStarted
        {
            get
            {
                return !IsStopped;
            }
        }

        public void Stop()
        {
            if (!IsOpen)
                Log.Error(m => m("Trying to stop a closed timer: {0}", Name));
            else if (IsStopped)
                Log.Error(m => m("Trying to stop an already stopped timer: {0}", Name));
            else
            {
                elapsed = (DateTime.Now.Ticks - StartTime) * 100;
                IsStopped = true;
            }
        }

        private volatile Boolean isStopped;
        public Boolean IsStopped
        {
            get { return isStopped; }
            private set { isStopped = value; }
        }

        public void Dispose()
        {
            //if (!IsStopped)
                Stop();
        }
        public override string ToString()
        {
            if (IsStopped)
                return String.Format("Timer: {0} StartTime: {1} Elapsed: {2}", Name, StartTime, Elapsed);
            return String.Format("Timer: {0} Running StartTime: {1}", Name, StartTime);
        }

        public ConcurrentInt64 Value
        {
            get { return Elapsed; }
        }
    }
}
