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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.Logging;

namespace TsdClient
{
    internal sealed class TsdMetrics : IMetrics
    {
        private ILog Log { get; set; }
        private const String FINAL_TIMESTAMP_KEY = "_end";//"finalTimestamp";
        private const String INITIAL_TIMESTAMP_KEY = "_start";//"initTimestamp";
        private const String ID_KEY = "_id";
        private const String HOST_KEY = "_host";
        private const String SERVICE_KEY = "_service";
        private const String CLUSTER_KEY = "_cluster";

        private ConcurrentDictionary<string, ConcurrentQueue<IQuantity<ConcurrentInt64>>> CounterSamples { get; set; }
        private ConcurrentDictionary<string, ICounter> Counters { get; set; }

        private ConcurrentDictionary<string, ConcurrentQueue<IQuantity<ConcurrentInt64>>> TimerSamples { get; set; }
        private ConcurrentDictionary<string, ITimer> Timers { get; set; }


        private ConcurrentDictionary<string, ConcurrentQueue<IQuantity<decimal>>> GaugesSamples { get; set; }

        public ConcurrentDictionary<string, string> Annotations { get; set; }

        public IEnumerable<ISink> Sinks { get; private set; }

        public TsdMetrics(IEnumerable<ISink> sinks, string service, string cluster, string host, ILog logger = null)
        {
            this.Log = logger ?? LogManager.GetLogger(typeof(TsdMetrics));

            Sinks = sinks;
            CounterSamples = new ConcurrentDictionary<string, ConcurrentQueue<IQuantity<ConcurrentInt64>>>();
            Counters = new ConcurrentDictionary<string, ICounter>();
            TimerSamples = new ConcurrentDictionary<string, ConcurrentQueue<IQuantity<ConcurrentInt64>>>();
            Timers = new ConcurrentDictionary<string, ITimer>();
            GaugesSamples = new ConcurrentDictionary<string, ConcurrentQueue<IQuantity<Decimal>>>();
            Annotations = new ConcurrentDictionary<string, string>();
            IsOpen = true;
            InternalAnnotate(INITIAL_TIMESTAMP_KEY, DateTime.Now.ToString("o"));
            InternalAnnotate(ID_KEY, Guid.NewGuid().ToString());
            InternalAnnotate(SERVICE_KEY, service);
            InternalAnnotate(CLUSTER_KEY, cluster);
            InternalAnnotate(HOST_KEY, host);
        }

        public ICounter CreateCounter(string name, IUnit unit)
        {
            var counter = new TsdCounter(name, 0, unit, IsOpen);
            if (!AssertIsOpen())
                return counter;
            AddCounter(name, counter);
            return counter;
        }
        public ICounter CreateCounter(string name)
        {
            return CreateCounter(name, null);
        }
        private void AddCounter(string name, ICounter counter)
        {
            var queue = new ConcurrentQueue<IQuantity<ConcurrentInt64>>();
            if (!CounterSamples.TryAdd(name, queue))
                queue = CounterSamples[name];
            queue.Enqueue(counter);
        }

        public void IncrementCounter(string name)
        {
            if (!AssertIsOpen())
                return;
            IncrementCounter(name, 1);
        }

        public void IncrementCounter(string name, ConcurrentInt64 value)
        {
            if (!AssertIsOpen())
                return;
            ICounter counter;
            if (!Counters.TryGetValue(name, out counter))
            {
                counter = CreateCounter(name);
                Counters.AddOrUpdate(name, counter, (k, v) => v);
            }
            counter.Increment(value);
        }

        public void DecrementCounter(string name)
        {
            if (!AssertIsOpen())
                return;
            DecrementCounter(name, 1);
        }

        public void DecrementCounter(string name, ConcurrentInt64 value)
        {
            if (!AssertIsOpen())
                return;
            IncrementCounter(name, -value);
        }

        public void ResetCounter(string name)
        {
            if (!AssertIsOpen())
                return;
            ICounter counter;
            if (!Counters.TryGetValue(name, out counter))
            {
                counter = CreateCounter(name);
                Counters.AddOrUpdate(name, counter, (k, v) => v);
                return;
            }
            counter = new TsdCounter(name, counter.InitialValue, counter.Unit, IsOpen);
            AddCounter(name, counter);
            Counters.AddOrUpdate(name, counter, (k, v) => counter);
        }

        public ITimer CreateTimer(string name)
        {
            if (!AssertIsOpen())
                return new TsdTimer(name, IsOpen);
            return InternalCreateTimer(name, 0);
        }

        private ITimer InternalCreateTimer(string name, Int64 elapsed, bool isOpen = true)
        {
            var timer = new TsdTimer(name, elapsed, isOpen);
            var queue = new ConcurrentQueue<IQuantity<ConcurrentInt64>>();
            if (!TimerSamples.TryAdd(name, queue))
                queue = TimerSamples[name];
            queue.Enqueue(timer);
            return timer;
        }

        public void StartTimer(string name)
        {
            if (!AssertIsOpen())
                return;
            ITimer timer;
            if (!Timers.TryGetValue(name, out timer))
                timer = CreateTimer(name);
            else
                Log.Error(m => m("Timer {0} is already started.", timer.Name));
            Timers.AddOrUpdate(name, timer, (k, v) => v);
        }

        public void StopTimer(string name)
        {
            if (!AssertIsOpen())
                return;
            ITimer timer;
            if (!Timers.TryGetValue(name, out timer))
            {
                Log.Error(m => m("Tried to stop a non existant timer: {0}", name));
                return;
            }
            if (timer.IsStopped)
            {
                Log.Error(m => m("Tried to stop an already stopped timer: {0}", name));
                return;
            }
            timer.Stop();
        }

        public void SetTimer(string name, long duration, TimeUnit unit)
        {
            if (!AssertIsOpen())
                return;
            (InternalCreateTimer(name, duration, false) as TsdTimer).Unit = unit;
        }

        public void SetGauge(string name, double value)
        {
            if (!AssertIsOpen())
                return;
            SetGauge(name, value, null);
        }

        public void SetGauge(string name, double value, IUnit unit)
        {
            if (!AssertIsOpen())
                return;
            var queue = new ConcurrentQueue<IQuantity<Decimal>>();
            if (!GaugesSamples.TryAdd(name, queue))
                queue = GaugesSamples[name];
            queue.Enqueue(new DecimalQuantity(new Decimal(value), unit));
        }

        public void SetGauge(string name, long value)
        {
            if (!AssertIsOpen())
                return;
            SetGauge(name, value, null);
        }

        public void SetGauge(string name, long value, IUnit unit)
        {
            if (!AssertIsOpen())
                return;
            var queue = new ConcurrentQueue<IQuantity<Decimal>>();
            if (!GaugesSamples.TryAdd(name, queue))
                queue = GaugesSamples[name];
            queue.Enqueue(new DecimalQuantity(value, unit));
        }

        public void Annotate(string key, string value)
        {
            if (!AssertIsOpen())
                return;
            InternalAnnotate(key, value);
        }

        private void InternalAnnotate(string key, string value)
        {
            Annotations.AddOrUpdate(key, value, (k, v) => value);
        }

        private volatile Boolean isOpen;

        public Boolean IsOpen
        {
            get { return isOpen; }
            private set { isOpen = value; }
        }

        private bool AssertIsOpen()
        {
            if (!IsOpen)
            {
                Log.Error(m => m("Trying to Operate a Closed Metrics"));
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            if (!AssertIsOpen())
                return;
            IsOpen = false;
            InternalAnnotate(FINAL_TIMESTAMP_KEY, DateTime.Now.ToString("o"));

            var annotations = new Dictionary<string, string>(Annotations);
            var timerSamples = CloneSamples(TimerSamples, TimerSelector);
            var counterSamples = CloneSamples(CounterSamples, SelectAlways);

            var gaugesSamples = CloneSamples(GaugesSamples, SelectAlways);
            var sample = new Sample(annotations, timerSamples, counterSamples, gaugesSamples);
            foreach (var sink in Sinks)
                sink.Record(sample);
        }

        private bool SelectAlways<T>(IQuantity<T> v) where T : IEquatable<T>
        {
            return true;
        }

        private bool TimerSelector(IQuantity<ConcurrentInt64> v)
        {
            var timer = v as ITimer;
            if (timer.IsStopped)
                return true;
            Log.Warn(m => m("Non stopped timer on Metrics dispose: {0}", timer.Name));
            return false;
        }

        private IDictionary<string, IEnumerable<IQuantity<T>>> CloneSamples<T>(IDictionary<string, ConcurrentQueue<IQuantity<T>>> samples, Func<IQuantity<T>, bool> selector)
            where T : IEquatable<T>
        {
            return (from x in samples
                    select new KeyValuePair<string, IEnumerable<IQuantity<T>>>(x.Key,
                        (from y in x.Value where selector(y) select new Quantity<T>(y.Value, y.Unit)).ToArray()//due to linq lazyness use ToArray to evaluate.
                        )).ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
