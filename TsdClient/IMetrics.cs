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
using System.Threading.Tasks;

namespace TsdClient
{
    public interface IMetrics : IDisposable
    {
        /**
     * Create and initialize a counter sample. It is valid to create multiple 
     * <code>Counter</code> instances with the same name, even concurrently, 
     * each will record a unique sample for the counter of the specified name. 
     * 
     * @param name The name of the counter.
     * @return <code>Counter</code> instance for recording a counter sample.
     */
        ICounter CreateCounter(String name, IUnit unit);
        ICounter CreateCounter(String name);
        /**
         * Increment the specified counter by 1. All counters are initialized to
         * zero.
         * 
         * @param name The name of the counter.
         */
        void IncrementCounter(String name);

        /**
         * Increment the specified counter by the specified amount. All counters are
         * initialized to zero.
         * 
         * @param name The name of the counter.
         * @param value The amount to increment by.
         */
        void IncrementCounter(String name, ConcurrentInt64 value);

        /**
         * Decrement the specified counter by 1. All counters are initialized to
         * zero.
         * 
         * @param name The name of the counter.
         */
        void DecrementCounter(String name);

        /**
         * Decrement the specified counter by the specified amount. All counters are
         * initialized to zero.
         * 
         * @param name The name of the counter.
         * @param value The amount to decrement by.
         */
        void DecrementCounter(String name, ConcurrentInt64 value);

        /**
         * Reset the counter to zero. This most commonly used to record a zero-count
         * for a particular counter. If clients wish to record set count metrics
         * then all counters should be reset before conditionally invoking increment
         * and/or decrement.
         * 
         * @param name The name of the counter.
         */
        void ResetCounter(String name);

        /**
         * Create and start a timer. It is valid to create multiple <code>Timer</code>
         * instances with the same name, even concurrently, each will record a
         * unique sample for the timer of the specified name. 
         * 
         * @param name The name of the timer.
         * @return <code>Timer</code> instance for recording a timing sample.
         */
        ITimer CreateTimer(String name);

        /**
         * Start the specified timer measurement.
         * 
         * @param name The name of the timer.
         */
        void StartTimer(String name);

        /**
         * Stop the specified timer measurement.
         * 
         * @param name The name of the timer.
         */
        void StopTimer(String name);

        /**
         * Set the timer to the specified value. This is most commonly used to 
         * record timers from external sources that are not integrated with metrics.
         * 
         * @param name The name of the timer.
         * @param duration The duration of the timer.
         * @param unit The time unit of the timer.
         */
        void SetTimer(String name, Int64 duration, TimeUnit unit);

        /**
         * Set the specified gauge reading. 
         * 
         * @param name The name of the gauge.
         * @param value The reading on the gauge
         */
        void SetGauge(String name, double value);

        /**
         * Set the specified gauge reading with a well-known unit. 
         * 
         * @param name The name of the gauge.
         * @param value The reading on the gauge
         * @param unit The unit of the value.
         */
        void SetGauge(String name, double value, IUnit unit);

        /**
         * Set the specified gauge reading. 
         * 
         * @param name The name of the gauge.
         * @param value The reading on the gauge
         */
        void SetGauge(String name, Int64 value);

        /**
         * Set the specified gauge reading with a well-known unit. 
         * 
         * @param name The name of the gauge.
         * @param value The reading on the gauge
         * @param unit The unit of the value.
         */
        void SetGauge(String name, Int64 value, IUnit unit);

        /**
         * Add an attribute that describes the captured metrics or context.
         * 
         * @param key The name of the attribute.
         * @param value The value of the attribute.
         */
        void Annotate(String key, String value);

        /**
         * Accessor to determine if this <code>Metrics</code> instance is open or
         * closed. Once closed an instance will not record new data.
         * 
         * @return True if and only if this <code>Metrics</code> instance is open.
         */
        bool IsOpen { get; }
    }
}
